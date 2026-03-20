using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class GolemAudio : MonoBehaviour
{
    [Header("Walk / Footsteps")]
    [SerializeField] private AudioClip[] walkClips;
    [SerializeField, Range(0f, 1f)] private float walkVolume = 0.8f;

    [Header("Kick / Melee")]
    [SerializeField] private AudioClip[] kickClips;
    [SerializeField, Range(0f, 1f)] private float kickVolume = 0.9f;

    [Header("Casting")]
    [SerializeField] private AudioClip[] castClips;
    [SerializeField, Range(0f, 1f)] private float castVolume = 0.9f;

    private AudioSource source;

    private void Awake()
    {
        source = GetComponent<AudioSource>();
        source.playOnAwake = false;
        source.loop = false;
    }

    public void PlayWalkStep()
    {
        PlayRandomClip(walkClips, walkVolume, 0.95f, 1.05f);
    }

    public void PlayKick()
    {
        PlayRandomClip(kickClips, kickVolume, 0.95f, 1.05f);
    }

    public void PlayCast()
    {
        PlayRandomClip(castClips, castVolume, 0.97f, 1.03f);
    }

    private void PlayRandomClip(AudioClip[] clips, float volume, float pitchMin, float pitchMax)
    {
        if (clips == null || clips.Length == 0)
            return;

        int idx = Random.Range(0, clips.Length);
        var clip = clips[idx];
        if (!clip)
            return;

        source.pitch = Random.Range(pitchMin, pitchMax);
        source.PlayOneShot(clip, volume);
    }
}
