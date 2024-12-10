using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine.Video;
using Random = UnityEngine.Random;

public class Parent : MonoBehaviour, IPointerDownHandler
{
    private JsonSaver _jsonSaver;
    private PoolManager _poolManager;
    private RawImage _rawImage;
    private bool _isChanged;
    private RectTransform _rectTransform;
    private VideoPlayer _videoPlayer;
    private RenderTexture _renderTexture;
    private Vector2 _scaleMinToMax;
    private GameManager _gameManager;
    private Vector2 _sizeDelta;
    
    [SerializeField] private int index;
    private float _time;
    private Vector2 _jsonSpeed;
    private float _speed;

    private bool _isPlaying;
    
    private AudioSource _audioSource;
    [SerializeField] private AudioClip[] audioClip;

    private CancellationTokenSource cts;
    
    private void Awake()
    {
        _jsonSaver = JsonSaver.instance;
        _poolManager = PoolManager.Instance;
        _rawImage = GetComponent<RawImage>();
        _rectTransform = GetComponent<RectTransform>();
        _videoPlayer = GetComponent<VideoPlayer>();
        _videoPlayer.source = VideoSource.Url;
        _renderTexture = new RenderTexture((int)_jsonSaver.settings.buttonData.size.x, (int)_jsonSaver.settings.buttonData.size.y, 24);
        _scaleMinToMax = _jsonSaver.settings.scaleMinToMax;
        
        _videoPlayer.loopPointReached += VideoPlayerOnloopPointReached;
        //_rectTransform.anchoredPosition = _jsonSaver.
        
        _rawImage.texture = _jsonSaver.dirtySprites[index].texture;
        _gameManager = GameManager.instance;
        _sizeDelta = _jsonSaver.settings.buttonData.size;
        _rectTransform.sizeDelta = _sizeDelta;
        _jsonSpeed = _jsonSaver.settings.speed;
        
        _audioSource = GetComponent<AudioSource>();
    }

    private void VideoPlayerOnloopPointReached(VideoPlayer source)
    {
        _isPlaying = false;
    }


    public void SetCleanOn()
    {
        // 방식 생각해봐야함
        // 1. 비디오 플레이어를 정지에서 재생
        // 2. 비디오 플레이어 1-2-3
        // 일단은 이 코드로 다음코드로 넘어가게 해야함

    }
    
    private void OnEnable()
    {
        _time = 20f;

        _speed = _gameManager.isBurning ? _jsonSpeed.y : _jsonSpeed.x;
        
        if (_videoPlayer.targetTexture)
        {
            _videoPlayer.targetTexture = null;
        }
        index = Random.Range(0, _jsonSaver.cleanPath.Length-1);
        
        int x = Random.Range(0,51);

        if (_gameManager.spawnTanTanCheat)
        {
            index = 7;
            _gameManager.spawnTanTanCheat = false;
        }
        
        if (_gameManager.SpawnTanTan && x > 48)
        {
            index = 7;
        }
        _rawImage.texture = _jsonSaver.dirtySprites[index].texture;
        _videoPlayer.url = _jsonSaver.cleanPath[index];
        _videoPlayer.Prepare();
        _rawImage.raycastTarget = true;
        _isChanged = false;


        float scaleOffset = Random.Range(_scaleMinToMax.x, _scaleMinToMax.y);
        Vector3 scale = Vector3.one * scaleOffset;
        scale.z = 1;
        transform.localScale = scale;
        transform.rotation = Quaternion.Euler(Vector3.zero);
        
        cts?.Cancel();
        cts = new CancellationTokenSource();
        
        SerOff(cts.Token).Forget();
    }

    
    private void Update()
    {
        if (!_isPlaying)
        {
            transform.position = new Vector3(transform.position.x, transform.position.y - _speed * Time.deltaTime, transform.position.z);

           if (_isChanged)
           {
               transform.Rotate(0,0,20f * Time.deltaTime);
           }
        }
        _time -= Time.deltaTime;

        if (_time < 0)
        {
            
        }
    }

    private async UniTaskVoid SerOff(CancellationToken ct)
    {
        while (true)
        {
            if (_rectTransform.anchoredPosition.y < -650)
            {
                _poolManager.ReleaseAndRemoveList(gameObject);
            }

            await UniTask.Delay(200, cancellationToken: ct);
        }
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (!_isChanged)
        {
            _rawImage.raycastTarget = false;
            _videoPlayer.time = 0;
            _videoPlayer.targetTexture = _renderTexture;
            _rawImage.texture = _renderTexture;
            _isChanged = true;
            _videoPlayer.Play();
            _isPlaying = true;
            ++_gameManager.score;
            _gameManager.AddInventory(index);

            
            
            if (index != 7)
            {
                _audioSource.clip = audioClip[0];
            }
            else
            {
                _audioSource.clip = audioClip[1];
                _gameManager.score += +9;
            }

            if (_audioSource.isPlaying)
            {
                _audioSource.Stop();
            }
            _audioSource.Play();
        }
    }

    private void OnDisable()
    {
        _videoPlayer.targetTexture = null;
        cts?.Cancel();
        cts?.Dispose();
        cts = null;
    }
    
    
}
