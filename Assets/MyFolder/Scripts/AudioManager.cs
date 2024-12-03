using System;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager instance;
    public AudioSource audioSource;
    [SerializeField] private AudioClip[] audioClip;
    private void Awake()
    {
        instance ??= this;
        audioSource = GetComponent<AudioSource>();
    }

    public void PlayNormal()
    {
        audioSource.PlayOneShot(audioClip[0]);
    }
    
    public void PlayTanTan()
    {
        audioSource.PlayOneShot(audioClip[1]);
    }
}
