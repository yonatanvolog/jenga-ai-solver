using UnityEngine;

public class AudioPlayer : MonoBehaviour
{
    [SerializeField] // AudioClip field visible in Inspector
    private AudioClip audioClip;

    [SerializeField, Range(0f, 1f)] // Volume field visible in Inspector
    private float volume = 1f;

    [SerializeField, Range(-3f, 3f)] // Pitch field visible in Inspector
    private float pitch = 1f;

    private AudioSource audioSource;

    void Start()
    {
        // Add an AudioSource component if it doesn't exist
        audioSource = gameObject.AddComponent<AudioSource>();

        // Assign the clip, set it to loop, volume, and pitch
        audioSource.clip = audioClip;
        audioSource.loop = true;
        audioSource.volume = volume;
        audioSource.pitch = pitch;

        // Start playing the clip
        audioSource.Play();
    }

    // Update volume and pitch in real-time, if needed
    void Update()
    {
        audioSource.volume = volume;
        audioSource.pitch = pitch;
    }
}