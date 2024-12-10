using System;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.Video;

public class API_Call : MonoBehaviour
{
    private const string API_URL = "http://192.168.0.254:8500/api/status_ecopop.cfm";
    private const string API_Idle = "http://192.168.0.254:8500/api/status_ecopop_update.cfm?status=idle";
    private const string API_Play = "http://192.168.0.254:8500/api/status_ecopop_update.cfm?status=play";
    private GameManager gameManager;
    private bool isIdle;
    private string currentState = "idle";
    private bool showLog;
    private VideoPlayer videoPlayer;
    
    private void Awake()
    {
        gameManager = GameManager.instance;
    }
    
    private void Start()
    {
        videoPlayer = gameManager.GetVideo0();
        
        string apiUrl = "http://192.168.0.254:8500/api/status_ecopop_update.cfm?status=idle";

        // API 요청
        SendApiRequestAsync(apiUrl, res =>
        {
            Debug.Log($"API Response for First Program: {res}");
        }).Forget();
        RepeatApiCallAsync().Forget();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.S))
        {
            showLog = !showLog;
        }
    }

    private async UniTaskVoid RepeatApiCallAsync()
    {
        while (true)
        {
            var startTime = Time.realtimeSinceStartup; // 호출 시작 시간 기록

            try
            {
                await CallApiAsync();
            }
            catch (Exception ex)
            {
                Debug.LogError($"Unexpected error in API call loop: {ex.Message}");
            }

            // 호출 간격 유지
            var elapsedTime = Time.realtimeSinceStartup - startTime; // 실행에 걸린 시간 계산
            var delay = Mathf.Max(1f - elapsedTime, 0); // 초과 시간 보정
            await UniTask.Delay(TimeSpan.FromSeconds(delay), DelayType.DeltaTime);
        }
    }
    
    //  api 호출
    private async UniTask CallApiAsync()
    {
        const int maxRetries = 3;
        int attempt = 0;

        while (attempt < maxRetries)
        {
            bool isSuccess = await SendApiRequestAsync(API_URL, ProcessResponse);

            if (isSuccess)
                return;

            attempt++;
            Debug.LogError("Failed to send API call request : " + attempt + " / " + maxRetries);
            await UniTask.Delay(1000); // 재시도 간격
        }

        Debug.LogError("Max retries reached. Skipping this API call.");
    }

    // API 변경
    private async UniTask<bool> SendApiRequestAsync(string url, Action<string> onSuccess)
    {
        using var webRequest = UnityWebRequest.Get(url);

        try
        {
            await webRequest.SendWebRequest();

            if (webRequest.result == UnityWebRequest.Result.Success)
            {
                string response = webRequest.downloadHandler.text.Trim();
                onSuccess(response);
                return true;
            }

            Debug.LogError($"API Error: {webRequest.error}");
            
        }
        catch (Exception ex)
        {
            Debug.LogError($"Exception during API request: {ex.Message}");
        }

        return false;
    }
    
    
    private void ProcessResponse(string response)
    {
        if (showLog)
        {
            Debug.Log(response);
        }

        if (response.Equals("start") &&  videoPlayer.isPlaying)
        {
            HandleStartState();
            return;
        }
        
        if (response.Equals(currentState)) return;

        if (response.Equals("start") &&  currentState != "idle")
        {
            // idle이 아닌데 바코드 찍었다면, api 복구
            string apiUrl = $"http://192.168.0.254:8500/api/status_ecopop_update.cfm?status={currentState}";

            // API 요청
            SendApiRequestAsync(apiUrl, res =>
            {
                Debug.Log($"API Response for First Program: {res}");
            }).Forget();
            
            return;
        }
        
        currentState = response;

        switch (ParseResponse(response))
        {
            case ResponseState.Idle:
                HandleIdleState();
                break;
            case ResponseState.Start:
                HandleStartState();
                break;
            case ResponseState.Play:
                HandlePlayState();
                break;
            case ResponseState.Unknown:
            default:
                Debug.LogWarning($"Unknown response state: {response}");
                break;
        }
    }

    private void HandleIdleState()
    {
    }

    private void HandleStartState()
    {
        gameManager.PlayPlay();
    }

    private void HandlePlayState()
    {
        
    }

    public void SetIdle()
    {
        SendApiRequestAsync(API_Idle, res =>
        {
            Debug.Log($"API Response for First Program: {res}");
        }).Forget();
    }
    
    private ResponseState ParseResponse(string response)
    {
        return response switch
        {
            "idle" => ResponseState.Idle,
            "start" => ResponseState.Start,
            "play" => ResponseState.Play,
            _ => ResponseState.Unknown // 알 수 없는 상태
        };
    }
    
    private enum ResponseState
    {
        Idle,
        Start,
        Play,
        Unknown
    }
}
