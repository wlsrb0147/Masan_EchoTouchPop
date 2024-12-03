using UnityEngine;
using UnityEngine.Video;

public class SetItemVideo : MonoBehaviour
{
    [SerializeField] private int x;
    private VideoPlayer _videoPlayer;
    private void Awake()
    {
        _videoPlayer = GetComponent<VideoPlayer>();
        _videoPlayer.source = VideoSource.Url;
        _videoPlayer.url = JsonSaver.instance.itemPath[x];
    }
}
