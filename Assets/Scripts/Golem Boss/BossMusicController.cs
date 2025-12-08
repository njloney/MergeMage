using UnityEngine;
using System.Collections;

[RequireComponent(typeof(AudioSource))]
public class BossMusicController : MonoBehaviour
{
    [Header("Music Clips")]
    [SerializeField] private AudioClip[] musicClips;

    [Header("Timing")]
    [SerializeField] private Vector2 randomDelayRange = new Vector2(8f, 20f);

    [Header("Playback")]
    [SerializeField, Range(0f, 1f)] private float volume = 0.7f;

    private AudioSource source;
    private Coroutine musicRoutine;

    private void Awake()
    {
        source = GetComponent<AudioSource>();
        source.playOnAwake = false;
        source.loop = false;

        source.spatialBlend = 0f;
        source.volume = volume;
    }

    private void Start()
    {
        PlayRandomMusic();

        musicRoutine = StartCoroutine(MusicLoop());
    }

    private IEnumerator MusicLoop()
    {
        while (true)
        {
            while (source.isPlaying)
                yield return null;

            float delay = Random.Range(randomDelayRange.x, randomDelayRange.y);
            yield return new WaitForSeconds(delay);

            PlayRandomMusic();
        }
    }

    private void PlayRandomMusic()
    {
        if (musicClips == null || musicClips.Length == 0)
            return;

        var clip = musicClips[Random.Range(0, musicClips.Length)];
        if (!clip)
            return;

        source.clip = clip;
        source.Play();
    }

    private void OnDisable()
    {
        if (musicRoutine != null)
            StopCoroutine(musicRoutine);
    }
}
