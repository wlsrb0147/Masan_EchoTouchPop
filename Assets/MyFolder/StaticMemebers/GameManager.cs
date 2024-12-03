using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.Video;
using Random = UnityEngine.Random;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;
    private PoolManager _poolManager;
    private List<GameObject> _pooledObjects;
    private JsonSaver _jsonSaver;
    
    [SerializeField] private VideoPlayer[] videoPlayers;
    [SerializeField] private RenderTexture renderTexture;

    private VideoPlayer _currentPlayer; // 현재 재생중인 비디오
    private VideoPlayer _nextPlayer; // 다음에 재생할 비디오
    private int _nextPlayerIndex = 1;
    
    private Vector2 _realSpawnInterval; 
    private int _realMaxSpawnableNum;   
    private Vector2 _spawnInterval; // nextScene의 num 기준으로 얘의 x,y값 random 돌려서 값 배치
    private int _maxSpawnableNum; // 노말 한번에 최대 스폰가능 오브젝트 숫자
    private Vector2 _burningInterval; // 버닝 시작시 스폰주기
    private int _burningSpawnableNum; // 버닝때 최대 스폰 숫자
    
    private float _playTime; // 60초로 기억함
    private float _timer;
    [SerializeField] private float burningTime;

    public int score;

    [SerializeField] private VideoPlayer[] itemVideos;
    [SerializeField] private GameObject[] itemImages;
    private CancellationTokenSource _cts;

    public bool isStart;
    
    // invoke Once
    private readonly bool[] _initializeScene = new bool[3];
    private bool _gameStart;
    private bool _isPlaying;
    public bool SpawnTanTan { get; private set; }
    public bool spawnTanTanCheat;

    public bool isBurning;
    
    // 오버레이 영상 오브젝트
    [SerializeField] private GameObject overlay;

    public void AddInventory(int x)
    {
        if (!itemImages[x].activeSelf)
        {
            itemImages[x].SetActive(true);
            itemVideos[x].Play();
        }
    }

    public void StartGame()
    {
        if (!isStart)
        {
            SwitchSideVideo();
        }
    }
    
    private void Awake()
    {
        if (!instance)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
        
        _jsonSaver = JsonSaver.instance;
        _playTime = _jsonSaver.settings.playTime;
        _spawnInterval = _jsonSaver.settings.spawnInterval;
        _maxSpawnableNum = _jsonSaver.settings.maxSpawnableNum;
        _burningInterval = _jsonSaver.settings.burningInterval;
        _burningSpawnableNum = _jsonSaver.settings.burningSpawnableNum;
        
        var config = AudioSettings.GetConfiguration();
        config.numRealVoices = 256; // 최대 동시 재생 가능한 음성 수 증가
        config.numVirtualVoices = 512; // 가상 음성 수 증가
        AudioSettings.Reset(config);
    }

    private void Start()
    {
        _poolManager = PoolManager.Instance;
        InitializeVideo();
        Cursor.visible = false;
    }
    
    private void InitializeVideo()
    {
        isBurning = false;
        foreach (VideoPlayer videoPlayer in videoPlayers)
        {
            if (videoPlayer.isPlaying)
            {
                videoPlayer.Stop();
            }
            
            if (videoPlayer.targetTexture)
            {
                videoPlayer.targetTexture = null;
            }
            
            videoPlayer.Prepare();
        }

        (_currentPlayer, _nextPlayer) = (videoPlayers[0], videoPlayers[1]);
        _currentPlayer.targetTexture = renderTexture;
        
        if (!_currentPlayer.isPlaying)
        {
            _currentPlayer.Play();
        }
        _nextPlayerIndex = 1;
    }

    public void UseRfid()
    {
        if (!_isPlaying)
        {
            Debug.Log("StartGame");
            SwitchSideVideo();
            _isPlaying = true;
        }
    }

    private void SwitchSideVideo()
    {
        _currentPlayer.targetTexture = null; // 그래픽 메모리 해제
        _currentPlayer.Stop();
        _nextPlayer.targetTexture = renderTexture;
        _nextPlayer.Play();
        _currentPlayer.Prepare();

        // 1회 실행하는 코드
        for (int i = 0; i < _initializeScene.Length; i++)
        {
            _initializeScene[i] = false;
        }
        
        
        // 전환 후 현재 플레이어를 멈추고 해제

        ++_nextPlayerIndex;
        if (_nextPlayerIndex >= videoPlayers.Length)
        {
            _nextPlayerIndex = 0;
        }

        if (_nextPlayerIndex == 1)
        {
            // 여기가 진짜 끝
            isBurning = false;
        }

        // 플레이어 전환
        (_currentPlayer, _nextPlayer) = (_nextPlayer, videoPlayers[_nextPlayerIndex]);
    }

    // 영상은 네개
    // 아이들 , 설명, 인트로, 플레이, 엔딩
    
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.M))
        {
            Cursor.visible = !Cursor.visible;
        }
        
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            SwitchSideVideo();
            _playTime = _jsonSaver.settings.playTime;
        }

        switch (_nextPlayerIndex) // currentPlaying + 1
        {
            // intro
            case 1 :
                if (!_initializeScene[0])
                {
                    score = 0;
                    _initializeScene[0] = true;
                    _gameStart = false;
                    _isPlaying = false;
                    isStart = false;
                }
                break;
            // play
            case 2 :
                if (!_initializeScene[1])
                {
                    isStart = true;
                    _cts?.Cancel();
                    _cts = new CancellationTokenSource();
                    overlay.SetActive(true);
                    StartGame(_cts.Token).Forget();
                    for (int i = 0; i < itemImages.Length; i++)
                    {
                        itemVideos[i].targetTexture.Create();
                        itemVideos[i].time = 0;
                        if (!itemVideos[i].isPrepared)
                        {
                            itemVideos[i].Prepare();
                        }
                    }
                    _realMaxSpawnableNum = _maxSpawnableNum;
                    _realSpawnInterval = _spawnInterval;

                    _initializeScene[1] = true;
                }

                if (!_gameStart) return;
                

                _timer -= Time.deltaTime;  
                if (_timer < 0)
                {
                    int k = Random.Range(1, _realMaxSpawnableNum+1);
                    for (int i = 0; i < k; ++i)
                    {
                        _poolManager.Get();
                    }
                    _timer = Random.Range(_realSpawnInterval.x, _realSpawnInterval.y);
                }
                
                break;
            // ending
            case 0:
                if (!_initializeScene[2])
                {
                    _cts?.Cancel();
                    _cts?.Dispose();
                    _cts = null;
                    SpawnTanTan = false;
                    overlay.SetActive(false);
                    
                    for (int i = 0; i < itemVideos.Length; i++)
                    {
                        itemImages[i].SetActive(false);
                        itemVideos[i].targetTexture.Release();
                    }
                    Invoke(nameof(SwitchSideVideo),10f);
                    _poolManager.ReturnAllPooledObjects();
                    
                    _initializeScene[2] = true;
                }
                break;
        }
    }
    

    private async UniTaskVoid StartGame(CancellationToken ct)
    {
        // 동영상 재생 후 21초 뒤 게임시작
        await UniTask.WaitForSeconds(21f, cancellationToken: ct);
        _gameStart = true;
        
        // 게임 끝나기 15초 전 버닝타임 시작
        await UniTask.WaitForSeconds(_playTime - 15f,cancellationToken: ct);
        _realMaxSpawnableNum = _burningSpawnableNum;
        _realSpawnInterval = _burningInterval;
        isBurning = true;
        SpawnTanTan = true;
        
        // 5초 뒤 탄탄이 확정등장
        await UniTask.WaitForSeconds( 5f,cancellationToken: ct);
        spawnTanTanCheat = true;
        _poolManager.Get();
        
        // 10초 뒤 게임종료
        await UniTask.WaitForSeconds( 10f,cancellationToken: ct);
        SwitchSideVideo();
    }
}
