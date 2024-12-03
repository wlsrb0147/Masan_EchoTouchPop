using System;
using System.IO;
using System.IO.Ports;
using System.Threading;
using UnityEngine;

public class RFID_Controller : MonoBehaviour
{
    private JsonSaver _jsonSaver;
    private GameManager _gameManager;
    
    private SerialPort[] _serialPorts; // 여러 포트를 관리하는 배열
    private string[] _portNames;
    private const int BaudRate = 9600; // BaudRate 설정
    private Thread[] _readThreads; // 각 포트별 데이터 수신 스레드
    private bool[] _isRunning; // 각 스레드의 실행 상태
    private bool _isApplicationQuitting;


    private void Awake()
    {
        _jsonSaver = JsonSaver.instance;
        _gameManager = GameManager.instance;
        Application.quitting += OnApplicationQuit;
    }

    private void Start()
    {
        _portNames = _jsonSaver.settings.portNames;
        
        int portCount = _portNames.Length;
        _serialPorts = new SerialPort[portCount];
        _readThreads = new Thread[portCount];
        _isRunning = new bool[portCount];

        // 모든 포트를 초기화
        for (int i = 0; i < portCount; i++)
        {
            InitializePort(i);
        }
    }

    private void InitializePort(int index)
    {
        try
        {
            _serialPorts[index] = new SerialPort(_portNames[index], BaudRate);
            _serialPorts[index].Open();

            _isRunning[index] = true;

            // 데이터 수신을 위한 스레드 시작
            _readThreads[index] = new Thread(() => ReadData(index));
            _readThreads[index].Start();

            Debug.Log($"Port {_portNames[index]} opened successfully.");
        }
        catch (Exception ex)
        {
            Debug.LogError($"Failed to open port {_portNames[index]}: {ex.Message}");
        }
    }

    private void ReadData(int index)
    {
        try
        {
            while (_isRunning[index])
            {
                if (_serialPorts[index] != null && _serialPorts[index].IsOpen)
                {
                    _serialPorts[index].DiscardInBuffer();
                    string data = _serialPorts[index].ReadLine()
                        .Replace("\u0003", "") // 제어 문자 제거
                        .Replace("\u0002", "") // 다른 제어 문자 제거
                        .Trim();
                    Debug.Log($"Data from {_portNames[index]}: {data}");

                    MainThreadDispatcher.RunOnMainThread(() =>
                    {
                        ProcessData(_portNames[index], data);
                    });
                }
            }
        }
        catch (IOException ex)
        {
            // 포트가 닫힌 경우 발생하는 예외는 무시
            Debug.Log($"Port {_portNames[index]} closed. Stopping read.");
        }
        catch (Exception ex)
        {
            Debug.LogError($"Unexpected error reading from port {_portNames[index]}: {ex.Message}");
        }
    }



    private void ProcessData(string portName, string data)
    {
        if (_isApplicationQuitting) // 종료 중인 경우 처리하지 않음
            return;

        Debug.Log($"Processing data from {portName}: {data}");
        _gameManager.UseRfid();
    }

    private void OnDestroy()
    {
        // 모든 포트를 닫고 스레드 종료
        CloseAllPorts();
    }
    
    private void OnApplicationQuit()
    {
        _isApplicationQuitting = true;
        CloseAllPorts();
    }
    
    private void CloseAllPorts()
    {
        if (_serialPorts == null) return;

        for (int i = 0; i < _serialPorts.Length; i++)
        {
            ClosePort(i);
        }
    }

    private void ClosePort(int index)
    {
        if (_serialPorts[index] != null && _serialPorts[index].IsOpen)
        {
            _isRunning[index] = false;
            _serialPorts[index].Close();
            Debug.Log($"Port {_portNames[index]} closed.");
        }

        if (_readThreads[index] != null && _readThreads[index].IsAlive)
        {
            _readThreads[index].Join();
        }
    }
}