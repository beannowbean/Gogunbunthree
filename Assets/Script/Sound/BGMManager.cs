using UnityEngine;

public class BGMManager : MonoBehaviour
{
    public static BGMManager Instance;

    public AudioClip mainScreenBGM;  // MainScreenBGM.mp3
    public AudioClip inGameBGM;      // InGameBGM.mp3

    private AudioSource audioSource;

    // Audio volume settings
    private const int MAX_VOLUME_LEVEL = 10;
    private const string VOLUME_PREFS_KEY = "BGM_Volume";
    private const string PREVIOUS_VOLUME_PREFS_KEY = "BGM_PreviousVolume";

    private int currentVolumeLevel = 3;
    private int previousVolumeLevel = MAX_VOLUME_LEVEL;

    // 플랫폼별 볼륨 보정 (모바일 기기에서 소리가 더 크게 들리므로 보정)
    private float platformVolumeMultiplier = 1f;

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
        
        // 플랫폼별 볼륨 배율 설정
        SetPlatformVolumeMultiplier();
        
        // 저장된 볼륨 설정 로드
        LoadVolumeSettings();
        
        ApplyVolume();
    }

    // 볼륨 레벨 증가
    public void IncreaseVolume()
    {
        currentVolumeLevel = Mathf.Min(MAX_VOLUME_LEVEL, currentVolumeLevel + 1);
        ApplyVolume();
        SaveVolumeSettings();
    }

    // 볼륨 레벨 감소
    public void DecreaseVolume()
    {
        currentVolumeLevel = Mathf.Max(0, currentVolumeLevel - 1);
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
        }
        else
        {
            currentVolumeLevel = (previousVolumeLevel > 0) ? previousVolumeLevel : MAX_VOLUME_LEVEL;
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

    // 실제 오디오 볼륨 적용 (AudioSource에 적용하는 로직)
    private void ApplyVolume()
    {
        if (audioSource != null)
        {
            float volume = currentVolumeLevel / (float)MAX_VOLUME_LEVEL;
            audioSource.volume = volume * platformVolumeMultiplier;
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
    }

    // BGM 정지
    public void StopBGM()
    {
        if (audioSource != null)
        {
            audioSource.Stop();
        }
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
            currentVolumeLevel = PlayerPrefs.GetInt(VOLUME_PREFS_KEY, 3);
            previousVolumeLevel = PlayerPrefs.GetInt(PREVIOUS_VOLUME_PREFS_KEY, MAX_VOLUME_LEVEL);
        }
    }

    // 플랫폼별 볼륨 배율 설정
    private void SetPlatformVolumeMultiplier()
    {
#if UNITY_ANDROID || UNITY_IOS
        // 모바일에서는 볼륨을 60%로 감소 (모바일 기기에서 소리가 더 크게 들림)
        platformVolumeMultiplier = 0.6f;
#else
        // 데스크톱(Windows, Mac, Linux)에서는 기본값 사용
        platformVolumeMultiplier = 1.0f;
#endif
    }
}
