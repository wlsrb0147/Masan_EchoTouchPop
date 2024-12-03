using System;
using UnityEngine;
using UnityEngine.Video;

public class SetVideoOnHover : MonoBehaviour
{
    private JsonSaver _jsonSaver;
    private VideoPlayer _videoPlayer;
    
    private void Awake()
    {
        _jsonSaver = JsonSaver.instance;
        _videoPlayer = GetComponent<VideoPlayer>();
        _videoPlayer.url = _jsonSaver.hoverMovie[0];
    }
}
