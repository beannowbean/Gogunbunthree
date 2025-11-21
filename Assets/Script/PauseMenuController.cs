using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

/***************************************************************

게임오브젝트(empty) (예: GameManager) 생성 후 Component로 추가

****************************************************************/

public class PauseMenuController : MonoBehaviour
{
    // -- Public Variables --

    public GameObject gamePauseRoot;            // GamePause GameObject

    public TextMeshProUGUI scoreValueText;      // 점수 표시용 Text

    public GameObject musicOnIcon;  // 예: 소리 나는 스피커 아이콘
    public GameObject musicOffIcon; // 예: X 표시된 스피커 아이콘
    public GameObject sfxOnIcon;
    public GameObject sfxOffIcon;

    // Audio volume levels
    private const int MAX_VOLUME_LEVEL = 10;
    private int currentMusicVolumeLevel = 3;
    private int currentSFXVolumeLevel = 4;

    private int previousMusicVolumeLevel = MAX_VOLUME_LEVEL;
    private int previousSFXVolumeLevel = MAX_VOLUME_LEVEL;

    // 재개 지연 시간
    private bool isCountingDown = false;
    public float resumeDelay = 3f; // 재개 지연 시간
    public TextMeshProUGUI countdownText; // 카운트다운 표시용 TMP 텍스트


    void Start()
    {
        // gamePauseRoot (GamePause) 시 활성화합니다.
        if (gamePauseRoot != null)
        {
            gamePauseRoot.SetActive(false);
        }

        if (countdownText != null)
        {
            countdownText.gameObject.SetActive(false);
        }
    }

    void Update()
    {
        // Esc 키를 누르면 일시 정지/재개 기능을 토글합니다.
        if (Input.GetKeyDown(KeyCode.Escape) && !isCountingDown)
        {
            if (gamePauseRoot.activeSelf)
            {
                ResumeGame();
            }
            else
            {
                PauseGame();
            }
        }
    }

    // 게임 일시정지 후 메뉴 활성화
    public void PauseGame()
    {
        if (gamePauseRoot != null)
        {
            gamePauseRoot.SetActive(true);
        }

        /*
        // 최신 점수 업데이트
        if (ScoreManager.Instance != null)
        {
            UpdateScoreDisplay(ScoreManager.Instance.GetCurrentScore());
        }
        */

        // 시간 멈춤
        Time.timeScale = 0f;
    }

    // 게임 재개
    public void ResumeGame()
    {
        if (isCountingDown)
            return;

        StartCoroutine(ResumeAfterDelay());
    }

    // 버튼의 OnClick 이벤트에 연결될 함수 (ResumeButton에 연결)
    public void OnResumeButtonClicked()
    {
        ResumeGame();
    }

    private IEnumerator ResumeAfterDelay()
    {
        isCountingDown = true;
        float timer = resumeDelay;

        if (gamePauseRoot != null)
        {
            gamePauseRoot.SetActive(false);
        }

        if (countdownText != null)
        {
            countdownText.gameObject.SetActive(true);
        }

        // Time.realtimeSinceStartup을 사용하여 TimeScale = 0f 상태에서도 타이머가 작동하게 합니다.
        while (timer > 0)
        {
            if (countdownText != null)
            {
                countdownText.text = $"STARTING IN\n{Mathf.CeilToInt(timer)}";
            }
            yield return null; // 다음 프레임까지 대기
            timer -= Time.unscaledDeltaTime; // TimeScale에 영향을 받지 않는 시간 사용
        }

        if (countdownText != null)
        {
            countdownText.gameObject.SetActive(false);
        }

        isCountingDown = false;

        // 게임 재개
        Time.timeScale = 1f;
    }

    // 메인 메뉴로 이동
    public void QuitGame()
    {
        // TODO: 메인 메뉴 이동 여부 한 번 더 확인하는 기능 추가

        Debug.Log("Quit Game button clicked. Loading Main Menu...");
        Time.timeScale = 1f;
        // SceneManager.LoadScene("MainMenu");
    }

    // 점수 표시
    public void UpdateScoreDisplay(int Score)
    {
        if (scoreValueText != null)
        {
            scoreValueText.text = Score.ToString();
        }
    }


    // --- Audio Control Methods ---

    public void DecreaseMusicVolume()
    {
        currentMusicVolumeLevel = Mathf.Max(0, currentMusicVolumeLevel - 1);
        Debug.Log($"Music Volume: {currentMusicVolumeLevel}");
        UpdateMusicIconUI();
    }

    public void IncreaseMusicVolume()
    {
        currentMusicVolumeLevel = Mathf.Min(MAX_VOLUME_LEVEL, currentMusicVolumeLevel + 1);
        Debug.Log($"Music Volume: {currentMusicVolumeLevel}");
        UpdateMusicIconUI();
    }

    public void DecreaseSFXVolume()
    {
        currentSFXVolumeLevel = Mathf.Max(0, currentSFXVolumeLevel - 1);
        Debug.Log($"SFX Volume: {currentSFXVolumeLevel}");
        UpdateSFXIconUI();
    }

    public void IncreaseSFXVolume()
    {
        currentSFXVolumeLevel = Mathf.Min(MAX_VOLUME_LEVEL, currentSFXVolumeLevel + 1);
        Debug.Log($"SFX Volume: {currentSFXVolumeLevel}");
        UpdateSFXIconUI();
    }


    // Music ( Mute / Unmute ) 구현
    public void ToggleMusicMute()
    {
        // 소리가 켜져 있으면 Mute
        if (currentMusicVolumeLevel > 0)
        {
            previousMusicVolumeLevel = currentMusicVolumeLevel; // 현재 볼륨 저장
            currentMusicVolumeLevel = 0; // 볼륨 0으로 설정
            Debug.Log($"Music Muted. (Saved: {previousMusicVolumeLevel})");
        }
        // 소리가 꺼져 있다면 Unmute
        else
        {
            // 저장된 볼륨이 0이라면 맥스 볼륨으로, 아니면 저장된 볼륨으로 복구
            currentMusicVolumeLevel = (previousMusicVolumeLevel > 0) ? previousMusicVolumeLevel : MAX_VOLUME_LEVEL;
            Debug.Log($"Music Unmuted. (Restored: {currentMusicVolumeLevel})");
        }

        // 아이콘 상태 갱신
        UpdateMusicIconUI();
    }

    // 아이콘을 껐다 켰다 하는 함수
    private void UpdateMusicIconUI()
    {
        // 볼륨이 0보다 크면 On 아이콘 활성화, Off 아이콘 비활성화
        bool isSoundOn = currentMusicVolumeLevel > 0;

        if (musicOnIcon != null) musicOnIcon.SetActive(isSoundOn);
        if (musicOffIcon != null) musicOffIcon.SetActive(!isSoundOn);
    }


    // 효과음(SFX) ( Mute / Unmute ) 구현

    public void ToggleSFXMute()
    {
        if (currentSFXVolumeLevel > 0)
        {
            previousSFXVolumeLevel = currentSFXVolumeLevel;
            currentSFXVolumeLevel = 0;
            Debug.Log($"SFX Muted. (Saved: {previousSFXVolumeLevel})");
        }
        else
        {
            currentSFXVolumeLevel = (previousSFXVolumeLevel > 0) ? previousSFXVolumeLevel : MAX_VOLUME_LEVEL;
            Debug.Log($"SFX Unmuted. (Restored: {currentSFXVolumeLevel})");
        }

        UpdateSFXIconUI();
    }

    private void UpdateSFXIconUI()
    {
        bool isSoundOn = currentSFXVolumeLevel > 0;

        if (sfxOnIcon != null) sfxOnIcon.SetActive(isSoundOn);
        if (sfxOffIcon != null) sfxOffIcon.SetActive(!isSoundOn);
    }
}