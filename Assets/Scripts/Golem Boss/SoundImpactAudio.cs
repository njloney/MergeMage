using UnityEngine;

public class SpellImpactAudio : MonoBehaviour
{
    [Header("Impact Sounds")]
    [SerializeField] private AudioClip[] impactClips;
    [SerializeField, Range(0f, 1f)] private float volume = 0.9f;
    [SerializeField] private Vector2 pitchRange = new Vector2(0.95f, 1.05f);
    [SerializeField] private bool playOnlyOncePerInstance = true;

    private bool played;

    private void OnEnable()
    {
        played = false;
    }

    public void PlayImpactSound()
    {
        if (playOnlyOncePerInstance && played)
            return;

        played = true;

        if (impactClips == null || impactClips.Length == 0)
            return;

        var clip = impactClips[Random.Range(0, impactClips.Length)];
        if (!clip)
            return;

        Debug.Log("[SpellImpactAudio] PlayImpactSound on " + name);

        // Create a temporary audio object at this position
        // This is independent of the projectile's own GameObject.
        float pitch = Random.Range(pitchRange.x, pitchRange.y);

        // Use a helper coroutine runner
        PlayClipAtPointWithPitch(clip, transform.position, volume, pitch);
    }

    private void PlayClipAtPointWithPitch(AudioClip clip, Vector3 pos, float vol, float pitch)
    {
        var go = new GameObject("SpellImpactAudio_" + clip.name);
        go.transform.position = pos;

        var src = go.AddComponent<AudioSource>();
        src.clip = clip;
        src.spatialBlend = 1f; // 3D
        src.volume = vol;
        src.pitch = pitch;
        src.rolloffMode = AudioRolloffMode.Linear;
        src.minDistance = 5f;
        src.maxDistance = 40f;
        src.Play();

        Object.Destroy(go, clip.length / Mathf.Max(0.1f, pitch));
    }
}
