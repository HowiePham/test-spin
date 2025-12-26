using UnityEngine;

public class WheelEffect : MonoBehaviour
{
    [Header("Sounds :")] [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip tickAudioClip;

    private void Awake()
    {
        SetupAudio();
    }

    private void SetupAudio()
    {
        this.audioSource.clip = this.tickAudioClip;
    }

    public void PlayAudio()
    {
        this.audioSource.PlayOneShot(this.audioSource.clip);
    }
}