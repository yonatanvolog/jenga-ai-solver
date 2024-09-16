using UnityEngine;

public class AudioPlayer : MonoBehaviour
{
    [SerializeField] 
    private AudioClip audioClip;

    [SerializeField, Range(0f, 1f)] 
    private float volume = 1f;

    [SerializeField, Range(-3f, 3f)] 
    private float pitch = 1f;

    private AudioSource audioSource;

    void Start()
    {
        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.clip = audioClip;
        audioSource.loop = true;
        audioSource.volume = volume;
        audioSource.pitch = pitch;
        audioSource.Play();
    }

    void Update()
    {
        audioSource.volume = volume;
        audioSource.pitch = pitch;
    }
}