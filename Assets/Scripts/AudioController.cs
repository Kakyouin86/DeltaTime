using UnityEngine;
using UnityEngine.UI;

public class AudioController : MonoBehaviour
{
    public AudioSource audioSource;

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
    }

    public void PlayAudio()
    {
        if (audioSource != null)
            audioSource.Play();
    }

    public void PauseAudio()
    {
        if (audioSource != null)
            audioSource.Pause();
    }

    public void StopAudio()
    {
        if (audioSource != null)
            audioSource.Stop();
    }
    public void RestartAudio()
    {
        if (audioSource != null)
        {
            audioSource.Stop();
            audioSource.Play();
        }
    }
    public void SetVolume(float value)
    {
        if (audioSource != null)
            audioSource.volume = value;
    }
}