using System.Collections.Generic;
using UnityEngine;

public class SFXManager : MonoBehaviour
{
    public static SFXManager Instance;

    [System.Serializable]
    public class Sound
    {
        public AudioClip clip; // 이름은 AudioClip.name 사용
        [Range(0f, 1f)]
        public float volume = 1f;
        public bool loop = false;
    }

    public List<Sound> sounds = new List<Sound>();

    [Range(0f, 1f)]
    public float masterVolume = 1f;

    private List<AudioSource> sources = new List<AudioSource>();

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        DontDestroyOnLoad(gameObject);

        foreach (var s in sounds)
        {
            AudioSource src = gameObject.AddComponent<AudioSource>();
            src.clip = s.clip;
            src.loop = s.loop;
            src.playOnAwake = false;
            src.volume = s.volume * masterVolume;

            sources.Add(src);
        }
    }

    public void Play(string clipName, float pitchMin = 1f, float pitchMax = 1f)
    {
        var src = GetSourceByName(clipName);
        if (src == null) return;

        src.pitch = Random.Range(pitchMin, pitchMax);
        src.volume = GetSoundVolume(clipName);
        src.time = 0.1f;
        src.PlayOneShot(src.clip);
    }


    public void Stop(string clipName)
    {
        var src = GetSourceByName(clipName);
        if (src == null) return;

        src.Stop();
    }

    public void SetMasterVolume(float value)
    {
        masterVolume = value;
        foreach (var src in sources)
        {
            Sound sound = sounds.Find(s => s.clip == src.clip);
            src.volume = sound.volume * masterVolume;
        }
    }

    // 🔍 AudioClip 이름으로 AudioSource 찾기
    private AudioSource GetSourceByName(string clipName)
    {
        return sources.Find(src => src.clip.name == clipName);
    }

    // 개별 + 마스터 볼륨 반영
    private float GetSoundVolume(string clipName)
    {
        Sound sound = sounds.Find(s => s.clip.name == clipName);
        return (sound != null ? sound.volume : 1f) * masterVolume;
    }
}
