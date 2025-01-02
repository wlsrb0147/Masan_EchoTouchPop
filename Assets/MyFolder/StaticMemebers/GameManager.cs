using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using TMPro;
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
    private bool _gameStart;
    private bool _isPlaying;
    public bool SpawnTanTan { get; private set; }
    public bool spawnTanTanCheat;

    public bool isBurning;
    public TMP_Text text;

    private int index;
    // 오버레이 영상 오브젝트
    [SerializeField] private GameObject overlay;

    private API_Call API_Call;

    public GameObject debugger;
    private bool _debuggerOn;

    public void AddInventory(int x)
    {
        if (!itemImages[x].activeSelf)
        {
            itemImages[x].SetActive(true);
            itemVideos[x].Play();
        }
    }

    public VideoPlayer GetVideo0()
    {
        return videoPlayers[0];
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

        API_Call = GetComponent<API_Call>();
    }

    private void Start()
    {
        _poolManager = PoolManager.Instance;
        InitializeVideo();
        Cursor.visible = false;
        debugger.SetActive(false);
    }

    private void InitializeVideo()
    {
        PlayIdle();
    }

    public void UseRfid()
    {
        if (!_isPlaying)
        {
            // 안씀
            Debug.Log("StartGame");
            SwitchSideVideo();
            _isPlaying = true;
        }
    }

    private void SwitchSideVideo()
    {
        switch (index)
        {
            case 0:
                PlayPlay();
                break;
            case 1:
                PlayEnd();
                break;
            case 2:
                PlayIdle();
                break;
        }
    }

    public void PlayIdle()
    {
        API_Call.SetIdle();
        videoPlayers[2].targetTexture = null;
        videoPlayers[2].Stop();
        videoPlayers[0].targetTexture = renderTexture;
        videoPlayers[0].Play();
        videoPlayers[1].Prepare();
        _gameStart = false;
        _isPlaying = false;
        isStart = false;
        text.text = "";
        score = 0;

    }

    public void PlayPlay()
    {
        videoPlayers[0].targetTexture = null;
        videoPlayers[0].Stop();
        videoPlayers[0].time = 0;
        videoPlayers[1].targetTexture = renderTexture;
        videoPlayers[1].Play();
        videoPlayers[2].Prepare();
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
    }

    public void PlayEnd()
    {
        videoPlayers[1].targetTexture = null;
        videoPlayers[1].Stop();
        videoPlayers[1].time = 0;
        videoPlayers[2].targetTexture = renderTexture;
        videoPlayers[2].Play();
        videoPlayers[0].Prepare();
        
        _cts?.Cancel();
        _cts?.Dispose();
        _cts = null;
        SpawnTanTan = false;
        isBurning = false;
        _gameStart = false;
        overlay.SetActive(false);

        for (int i = 0; i < itemVideos.Length; i++)
        {
            itemImages[i].SetActive(false);
            itemVideos[i].targetTexture.Release();
        }


        // 기후 위기 관련
        // 기후가 더 안정적으로 변했다 라고 하며 될듯
        switch (score)
        {
            case < 30 :
                text.text = $"총 {score}점을 획득하셨어요";
                break;
            case < 50 :
                text.text = $"총 {score}점을 획득하셨어요\n여러분의 노력으로 기후가 조금 회복되었어요" ;
                break;
            case > 50 and < 60 :
                text.text = $"총 {score}점을 획득하셨어요\n여러분의 노력으로 기후가 약간 회복되었어요";
                break; 
            case > 60 and < 70 :
                text.text = $"총 {score}점을 획득하셨어요!\n여러분의 노력으로 기후가 많이 회복되었어요!";
                break;
            case > 70 and < 80 :
                text.text = $"총 {score}점을 획득하셨어요!!\n여러분의 노력으로 기후가 상당히 회복되었어요!!";
                break;
            case > 80 :
                text.text = $"총 {score}점을 획득하셨어요!!!\n여러분의 노력으로 기후가 완전히 회복되었어요!!!";
                break;
        }
        
        //text.text = $"총 {score}점을 획득하셨어요!";

        Invoke(nameof(PlayIdle), 10f);
        _poolManager.ReturnAllPooledObjects();
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

        if (Input.GetKeyDown(KeyCode.I))
        {
            _debuggerOn = !_debuggerOn;
            debugger.SetActive(_debuggerOn);
        }
        
        if (!_gameStart) return;


        _timer -= Time.deltaTime;
        if (_timer < 0)
        {
            int k = Random.Range(2, _realMaxSpawnableNum + 1);
            for (int i = 0; i < k; ++i)
            {
                _poolManager.Get();
            }

            _timer = Random.Range(_realSpawnInterval.x, _realSpawnInterval.y);
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
        await UniTask.WaitForSeconds( 12f,cancellationToken: ct);
        PlayEnd();
    }
}
