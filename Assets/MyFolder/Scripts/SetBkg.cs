using UnityEngine;
using UnityEngine.Video;

public class SetBkg : MonoBehaviour
{
    private VideoPlayer _videoPlayer;
    private void Awake()
    {
        _videoPlayer = GetComponent<VideoPlayer>();
        _videoPlayer.source = VideoSource.Url;
        _videoPlayer.isLooping = true;
        _videoPlayer.url = JsonSaver.instance.bkgPath[0];
        _videoPlayer.Play();
    }
}
