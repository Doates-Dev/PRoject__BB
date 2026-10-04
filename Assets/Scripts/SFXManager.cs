using UnityEngine;
using UnityEngine.Audio;

public class SFXManager : MonoBehaviour
{
    public static SFXManager Instance { get; private set; }

    [Header("Clips")]
    public AudioClip[] topHitClips;          // several variations sound better than one

    [Header("Settings")]
    [Range(0f, 1f)] public float masterVolume = 1f;
    public AudioMixerGroup mixerGroup;       // optional: route to an SFX mixer group
    public int poolSize = 16;                // max simultaneous sounds
    public Vector2 pitchRange = new Vector2(0.9f, 1.1f);

    AudioSource[] pool;
    int nextSource;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        pool = new AudioSource[poolSize];
        for (int i = 0; i < poolSize; i++)
        {
            var src = gameObject.AddComponent<AudioSource>();
            src.playOnAwake = false;
            src.outputAudioMixerGroup = mixerGroup;
            src.spatialBlend = 1f; // 3D sound; set to 0 for flat 2D audio
            src.rolloffMode = AudioRolloffMode.Linear;
            src.maxDistance = 40f;
            src.spatialBlend = 0f; // 2D: no distance falloff
            pool[i] = src;
        }
    }

    public void PlayTopHit(Vector3 position, float intensity = 1f)
    {
        
        Debug.Log("PlayTopHit called");
        Play(topHitClips, position, intensity);

        
    }

    void Play(AudioClip[] clips, Vector3 position, float volume = 1f)
    {
        
        if (clips == null || clips.Length == 0) return;

        AudioClip clip = clips[Random.Range(0, clips.Length)];
        if (clip == null) { Debug.Log("Chosen clip is NULL"); return; }

        AudioSource src = pool[nextSource];
        nextSource = (nextSource + 1) % pool.Length;

        src.transform.position = position;
        src.pitch = Random.Range(pitchRange.x, pitchRange.y);
        
        src.PlayOneShot(clip, Mathf.Clamp01(volume) * masterVolume);
    }
}