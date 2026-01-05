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

    private List<AudioSource> sources = new List<AudioSource>();

    // Audio volume settings
    private const int MAX_VOLUME_LEVEL = 10;
    private const string VOLUME_PREFS_KEY = "SFX_Volume";
    private const string PREVIOUS_VOLUME_PREFS_KEY = "SFX_PreviousVolume";

    private int currentVolumeLevel = 4;
    private int previousVolumeLevel = MAX_VOLUME_LEVEL;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        DontDestroyOnLoad(gameObject);

        // 저장된 볼륨 설정 로드
        LoadVolumeSettings();

        foreach (var s in sounds)
        {
            AudioSource src = gameObject.AddComponent<AudioSource>();
            src.clip = s.clip;
            src.loop = s.loop;
            src.playOnAwake = false;
            src.volume = s.volume * (currentVolumeLevel / (float)MAX_VOLUME_LEVEL);

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

    // 🔍 AudioClip 이름으로 AudioSource 찾기
    private AudioSource GetSourceByName(string clipName)
    {
        return sources.Find(src => src.clip.name == clipName);
    }

    // 개별 볼륨 반영
    private float GetSoundVolume(string clipName)
    {
        Sound sound = sounds.Find(s => s.clip.name == clipName);
        float currentVolume = currentVolumeLevel / (float)MAX_VOLUME_LEVEL;
        return (sound != null ? sound.volume : 1f) * currentVolume;
    }

    // 볼륨 레벨 증가
    public void IncreaseVolume()
    {
        currentVolumeLevel = Mathf.Min(MAX_VOLUME_LEVEL, currentVolumeLevel + 1);
        Debug.Log($"SFX Volume: {currentVolumeLevel}");
        ApplyVolume();
        SaveVolumeSettings();
    }

    // 볼륨 레벨 감소
    public void DecreaseVolume()
    {
        currentVolumeLevel = Mathf.Max(0, currentVolumeLevel - 1);
        Debug.Log($"SFX Volume: {currentVolumeLevel}");
        ApplyVolume();
        SaveVolumeSettings();
    }

    // Mute / Unmute 토글
    public void ToggleMute()
    {
        if (currentVolumeLevel > 0)
        {
            previousVolumeLevel = currentVolumeLevel;
            currentVolumeLevel = 0;
            Debug.Log($"SFX Muted. (Saved: {previousVolumeLevel})");
        }
        else
        {
            currentVolumeLevel = (previousVolumeLevel > 0) ? previousVolumeLevel : MAX_VOLUME_LEVEL;
            Debug.Log($"SFX Unmuted. (Restored: {currentVolumeLevel})");
        }
        ApplyVolume();
        SaveVolumeSettings();
    }

    // 현재 볼륨 레벨 가져오기
    public int GetCurrentVolumeLevel()
    {
        return currentVolumeLevel;
    }

    // 볼륨이 켜져 있는지 확인
    public bool IsMuted()
    {
        return currentVolumeLevel == 0;
    }

    // 실제 오디오 볼륨 적용
    private void ApplyVolume()
    {
        float volume = currentVolumeLevel / (float)MAX_VOLUME_LEVEL;
        
        foreach (var src in sources)
        {
            Sound sound = sounds.Find(s => s.clip == src.clip);
            if (sound != null)
            {
                src.volume = sound.volume * volume;
            }
        }
        
        Debug.Log($"SFX Volume applied: {volume}");
    }

    // 볼륨 설정 저장
    private void SaveVolumeSettings()
    {
        PlayerPrefs.SetInt(VOLUME_PREFS_KEY, currentVolumeLevel);
        PlayerPrefs.SetInt(PREVIOUS_VOLUME_PREFS_KEY, previousVolumeLevel);
        PlayerPrefs.Save();
    }

    // 볼륨 설정 로드
    private void LoadVolumeSettings()
    {
        if (PlayerPrefs.HasKey(VOLUME_PREFS_KEY))
        {
            currentVolumeLevel = PlayerPrefs.GetInt(VOLUME_PREFS_KEY, 4);
            previousVolumeLevel = PlayerPrefs.GetInt(PREVIOUS_VOLUME_PREFS_KEY, MAX_VOLUME_LEVEL);
            Debug.Log($"SFX Volume loaded: {currentVolumeLevel}");
        }
    }
}
