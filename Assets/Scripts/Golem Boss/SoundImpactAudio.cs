using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class SpellImpactAudio : MonoBehaviour
{
    [Header("Impact Sounds")]
    [SerializeField] private AudioClip[] impactClips;
    [SerializeField, Range(0f, 1f)] private float volume = 0.9f;
    [SerializeField] private Vector2 pitchRange = new Vector2(0.95f, 1.05f);

    private AudioSource source;
    private bool played;

    private void Awake()
    {
        source = GetComponent<AudioSource>();
        source.playOnAwake = false;
        source.loop = false;
    }

    public void PlayImpactSound()
    {
        if (played)
            return;

        played = true;

        if (impactClips == null || impactClips.Length == 0)
            return;

        var clip = impactClips[Random.Range(0, impactClips.Length)];
        if (!clip)
            return;

        source.pitch = Random.Range(pitchRange.x, pitchRange.y);
        source.PlayOneShot(clip, volume);
    }
}
