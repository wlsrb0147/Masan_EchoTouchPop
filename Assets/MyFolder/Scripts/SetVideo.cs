using UnityEngine;
using UnityEngine.Video;

public class SetVideo : MonoBehaviour
{
    private string _path;
    private VideoPlayer _videoPlayer;
    [SerializeField] private int index;
    
    private void Awake()
    {
        _videoPlayer = GetComponent<VideoPlayer>();
        _videoPlayer.source = VideoSource.Url;
        _path = JsonSaver.instance.backgroundPath[index];
        _videoPlayer.url = _path;
    }
}
