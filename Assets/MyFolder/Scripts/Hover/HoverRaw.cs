using UnityEngine;
using UnityEngine.Video;

public class HoverRaw : MonoBehaviour
{
    [SerializeField] private RenderTexture renderTexture;
    [SerializeField] private VideoPlayer videoPlayer;


    private void OnEnable()
    {
        videoPlayer.targetTexture = renderTexture;
        videoPlayer.Play();
    }
    
    private void OnDisable()
    {
        if (videoPlayer is not null)
        {
            videoPlayer.Stop();
            renderTexture.Release();
            videoPlayer.targetTexture = null;
            videoPlayer.Prepare();
        }
    }
}
