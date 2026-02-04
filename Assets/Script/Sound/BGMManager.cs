using System.Collections;
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

    // 플랫폼별 볼륨 보정
    private float platformVolumeMultiplier = 1f;

    // [추가] 페이드 효과가 중복 실행되지 않도록 관리하는 변수
    private Coroutine currentFadeCoroutine;

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

        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.loop = true;
        audioSource.playOnAwake = false;

        SetPlatformVolumeMultiplier();
        LoadVolumeSettings();

        // 시작할 때는 설정된 볼륨으로 적용
        ApplyVolume();
    }

    // 외부에서 호출: 특정 볼륨으로 부드럽게 조절
    public void FadeTo(float targetVolume, float duration)
    {
        // 기존에 실행 중이던 페이드가 있다면 중지 (꼬임 방지)
        if (currentFadeCoroutine != null)
        {
            StopCoroutine(currentFadeCoroutine);
        }
        currentFadeCoroutine = StartCoroutine(FadeRoutine(targetVolume, duration));
    }

    // 실제 페이드를 수행하는 코루틴
    private IEnumerator FadeRoutine(float targetVolume, float duration)
    {
        float startVolume = audioSource.volume;
        float timer = 0f;

        while (timer < duration)
        {
            timer += Time.unscaledDeltaTime; // TimeScale이 0이어도 소리는 줄어들게
            audioSource.volume = Mathf.Lerp(startVolume, targetVolume, timer / duration);
            yield return null;
        }

        audioSource.volume = targetVolume;
        currentFadeCoroutine = null;
    }

    // 인게임 BGM을 0에서부터 설정된 볼륨까지 서서히 켬 (페이드 인)
    public void PlayInGameBGMWithFadeIn(float duration)
    {
        // 1. 클립 교체 및 재생
        PlayBGM(inGameBGM);

        // 2. 소리를 일단 0으로 끔
        audioSource.volume = 0f;

        // 3. 목표 볼륨 계산: 사용자가 설정해둔 볼륨 크기 (0~10단계)를 실수(0.0~1.0)로 변환
        float targetVolume = (currentVolumeLevel / (float)MAX_VOLUME_LEVEL) * platformVolumeMultiplier;

        // 4. 0 -> 목표 볼륨으로 페이드 인
        FadeTo(targetVolume, duration);
    }

    public void IncreaseVolume()
    {
        currentVolumeLevel = Mathf.Min(MAX_VOLUME_LEVEL, currentVolumeLevel + 1);
        ApplyVolume();
        SaveVolumeSettings();
    }

    public void DecreaseVolume()
    {
        currentVolumeLevel = Mathf.Max(0, currentVolumeLevel - 1);
        ApplyVolume();
        SaveVolumeSettings();
    }

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

    public int GetCurrentVolumeLevel()
    {
        return currentVolumeLevel;
    }

    public bool IsMuted()
    {
        return currentVolumeLevel == 0;
    }

    // AudioSource에 현재 설정된 볼륨을 즉시 적용
    private void ApplyVolume()
    {
        // 페이드 중일 때는 강제로 볼륨을 바꾸면 튀는 소리가 날 수 있으므로,
        // 페이드 코루틴이 없을 때만 즉시 적용
        if (audioSource != null && currentFadeCoroutine == null)
        {
            float volume = currentVolumeLevel / (float)MAX_VOLUME_LEVEL;
            audioSource.volume = volume * platformVolumeMultiplier;
        }
    }

    public void PlayMainScreenBGM()
    {
        // 메인 BGM은 즉시 원래 볼륨으로 재생
        PlayBGM(mainScreenBGM);
        ApplyVolume();
    }

    public void PlayInGameBGM()
    {
        PlayBGM(inGameBGM);
        ApplyVolume();
    }

    public void RestartInGameBGM()
    {
        if (audioSource == null || inGameBGM == null) return;

        audioSource.Stop();
        audioSource.clip = inGameBGM;
        audioSource.Play();
        ApplyVolume();
    }

    private void PlayBGM(AudioClip clip)
    {
        if (audioSource == null || clip == null) return;

        if (audioSource.clip == clip && audioSource.isPlaying)
        {
            return;
        }

        audioSource.clip = clip;
        audioSource.Play();
    }

    public void StopBGM()
    {
        if (audioSource != null)
        {
            audioSource.Stop();
        }
    }

    private void SaveVolumeSettings()
    {
        PlayerPrefs.SetInt(VOLUME_PREFS_KEY, currentVolumeLevel);
        PlayerPrefs.SetInt(PREVIOUS_VOLUME_PREFS_KEY, previousVolumeLevel);
        PlayerPrefs.Save();
    }

    private void LoadVolumeSettings()
    {
        if (PlayerPrefs.HasKey(VOLUME_PREFS_KEY))
        {
            currentVolumeLevel = PlayerPrefs.GetInt(VOLUME_PREFS_KEY, 3);
            previousVolumeLevel = PlayerPrefs.GetInt(PREVIOUS_VOLUME_PREFS_KEY, MAX_VOLUME_LEVEL);
        }
    }

    private void SetPlatformVolumeMultiplier()
    {
#if UNITY_ANDROID || UNITY_IOS
        platformVolumeMultiplier = 0.6f;
#else
        platformVolumeMultiplier = 1.0f;
#endif
    }
}