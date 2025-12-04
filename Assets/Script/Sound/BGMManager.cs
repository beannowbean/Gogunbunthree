using UnityEngine;

public class BGMManager : MonoBehaviour
{
    public static BGMManager Instance;

    public AudioClip mainScreenBGM;  // MainScreenBGM.mp3
    public AudioClip inGameBGM;      // InGameBGM.mp3

    private AudioSource audioSource;

    // Audio volume settings
    private const int MAX_VOLUME_LEVEL = 10;

    private int currentVolumeLevel = 3;
    private int previousVolumeLevel = MAX_VOLUME_LEVEL;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        // AudioSource 컴포넌트 추가
        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.loop = true;
        audioSource.playOnAwake = false;
        
        ApplyVolume();
    }

    // 볼륨 레벨 증가
    public void IncreaseVolume()
    {
        currentVolumeLevel = Mathf.Min(MAX_VOLUME_LEVEL, currentVolumeLevel + 1);
        Debug.Log($"BGM Volume: {currentVolumeLevel}");
        ApplyVolume();
    }

    // 볼륨 레벨 감소
    public void DecreaseVolume()
    {
        currentVolumeLevel = Mathf.Max(0, currentVolumeLevel - 1);
        Debug.Log($"BGM Volume: {currentVolumeLevel}");
        ApplyVolume();
    }

    // Mute / Unmute 토글
    public void ToggleMute()
    {
        if (currentVolumeLevel > 0)
        {
            previousVolumeLevel = currentVolumeLevel;
            currentVolumeLevel = 0;
            Debug.Log($"BGM Muted. (Saved: {previousVolumeLevel})");
        }
        else
        {
            currentVolumeLevel = (previousVolumeLevel > 0) ? previousVolumeLevel : MAX_VOLUME_LEVEL;
            Debug.Log($"BGM Unmuted. (Restored: {currentVolumeLevel})");
        }
        ApplyVolume();
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

    // 실제 오디오 볼륨 적용 (AudioSource에 적용하는 로직)
    private void ApplyVolume()
    {
        if (audioSource != null)
        {
            float volume = currentVolumeLevel / (float)MAX_VOLUME_LEVEL;
            audioSource.volume = volume;
            Debug.Log($"BGM Volume applied: {volume}");
        }
    }

    // 메인 화면 BGM 재생
    public void PlayMainScreenBGM()
    {
        PlayBGM(mainScreenBGM);
    }

    // 인게임 BGM 재생
    public void PlayInGameBGM()
    {
        PlayBGM(inGameBGM);
    }

    // BGM 재생
    private void PlayBGM(AudioClip clip)
    {
        if (audioSource == null || clip == null) return;

        // 같은 클립이 재생 중이면 그대로 유지
        if (audioSource.clip == clip && audioSource.isPlaying)
        {
            return;
        }

        audioSource.clip = clip;
        audioSource.Play();
        Debug.Log($"Playing BGM: {clip.name}");
    }

    // BGM 정지
    public void StopBGM()
    {
        if (audioSource != null)
        {
            audioSource.Stop();
        }
    }
}
