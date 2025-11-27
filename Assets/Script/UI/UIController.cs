using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

/***************************************************************

게임오브젝트(empty) (예: GameManager) 생성 후 Component로 추가

****************************************************************/

public class UIController : MonoBehaviour
{
    // -- Public Variables --

    public GameObject gamePauseRoot;            // GamePause GameObject
    public GameObject inGameUIRoot;             // 인게임 UI 루트 (일시정지 시 비활성화용)
    public GameObject gameStartRoot;          // 게임 시작 UI 루트
    public GameObject confirmationPanel;      // 종료 확인 패널

    public GameObject gameOverRoot;

    public TextMeshProUGUI scoreValueText;      // 점수 표시용 Text
    public TextMeshProUGUI gameOverScoreText;
    public TextMeshProUGUI gameOverOrClearText; // "GAME OVER" or "GAME CLEAR" 텍스트
    public GameObject newRecordText;

    public GameObject[] musicOnIcons;  // 소리 켜짐 아이콘들 (메인 + 일시정지)
    public GameObject[] musicOffIcons; // 소리 꺼짐 아이콘들 (메인 + 일시정지)
    public GameObject[] sfxOnIcons;
    public GameObject[] sfxOffIcons;



    // Audio volume levels
    private const int MAX_VOLUME_LEVEL = 10;
    private int currentMusicVolumeLevel = 3;
    private int currentSFXVolumeLevel = 4;

    private int previousMusicVolumeLevel = MAX_VOLUME_LEVEL;
    private int previousSFXVolumeLevel = MAX_VOLUME_LEVEL;

    private bool isGameStarted = false;
    private bool isNewRecord = false;

    // 재개 지연 시간
    private bool isCountingDown = false;
    public float resumeDelay = 3f; // 재개 지연 시간
    public TextMeshProUGUI countdownText; // 카운트다운 표시용 TMP 텍스트


    void Start()
    {
        // 1. 시작 시 Pause 메뉴는 끄고
        if (gamePauseRoot != null) gamePauseRoot.SetActive(false);
        if (confirmationPanel != null) confirmationPanel.SetActive(false);

        // 2. 시작 시 인게임 UI도 끈다
        if (inGameUIRoot != null) inGameUIRoot.SetActive(false);

        if (countdownText != null) countdownText.gameObject.SetActive(false);

        if (gameOverRoot != null) gameOverRoot.SetActive(false);

        if (gameStartRoot != null) gameStartRoot.SetActive(true);

        Time.timeScale = 0f;

        UpdateMusicIconUI();
        UpdateSFXIconUI();
    }

    void Update()
    {
        
        
        // Esc 키를 누르면 일시 정지/재개 기능을 토글합니다.
        if (Input.GetKeyDown(KeyCode.Escape) && !isCountingDown && isGameStarted)
        {
            if (gamePauseRoot.activeSelf) ResumeGame();
            else PauseGame();
        }

        if (Input.GetKeyDown(KeyCode.S))
        {
            GameOver();
        }
    }

    // 게임 일시정지 후 메뉴 활성화
    public void PauseGame()
    {
        // 인게임 UI 비활성화
        if (inGameUIRoot != null) inGameUIRoot.SetActive(false);

        if (gamePauseRoot != null)
        {
            gamePauseRoot.SetActive(true);
        }

        
        // 최신 점수 업데이트
        if (ScoreManager.Instance != null)
        {
            UpdateScoreDisplay(ScoreManager.Instance.GetCurrentScore());
        }
        

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

        if (gamePauseRoot != null) gamePauseRoot.SetActive(false);

        if (inGameUIRoot != null) inGameUIRoot.SetActive(true);

        if (countdownText != null) countdownText.gameObject.SetActive(true);

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
    public void ConfirmQuitGame()
    {
        if (ScoreManager.Instance != null)
        {
            ScoreManager.Instance.ResetScore();
        }

        if (gamePauseRoot != null) gamePauseRoot.SetActive(false);
        if (confirmationPanel != null) confirmationPanel.SetActive(false);
        if (gameOverRoot != null) gameOverRoot.SetActive(false);
        if (gameStartRoot != null) gameStartRoot.gameObject.SetActive(true);
    }

    public void CancelQuit()
    {
        // 재확인 팝업만 끄고, 뒤에 있는 Pause 메뉴를 유지합니다.
        if (confirmationPanel != null)
        {
            confirmationPanel.SetActive(false);
        }
    }

    public void QuitGame()
    {
        if (confirmationPanel != null)
        {
            confirmationPanel.SetActive(true);
        }
    }

    public void GameOver()
    {
        // 1. 모든 인게임/팝업 UI 숨기기
        if (inGameUIRoot != null) inGameUIRoot.SetActive(false);
        if (gamePauseRoot != null) gamePauseRoot.SetActive(false);
        if (confirmationPanel != null) confirmationPanel.SetActive(false);

        // 2. 게임 오버 화면 켜기
        if (gameOverRoot != null) gameOverRoot.SetActive(true);

        // 3. 점수 및 기록 표시
        if (ScoreManager.Instance != null)
        {
            UpdateScoreDisplay(ScoreManager.Instance.GetCurrentScore());
        }

        // 4. 뉴 레코드 메시지 활성화/비활성화
        if (newRecordText != null)
        {
            isNewRecord = ScoreManager.Instance != null && ScoreManager.Instance.GetIsNewRecord();
            newRecordText.SetActive(isNewRecord);
        }

        // 5. 게임 시간 멈추기 (TimeScale이 0인 상태에서 이 함수를 호출할 경우도 대비)
        Time.timeScale = 0f;
        isGameStarted = false;
        if (ScoreManager.Instance != null)
        {
            ScoreManager.Instance.ResetScore();
        }
    }

    // 점수 표시
    public void UpdateScoreDisplay(int Score)
    {
        if (scoreValueText != null)
        {
            scoreValueText.text = Score.ToString();
        }
    }

    public void StartGame()
    {
        if (gameStartRoot != null) gameStartRoot.SetActive(false);
        if (inGameUIRoot != null) inGameUIRoot.SetActive(true);

        isGameStarted = true;
        Time.timeScale = 1f;
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

        // Music On 아이콘들 처리
        if (musicOnIcons != null)
        {
            foreach (GameObject icon in musicOnIcons)
            {
                if (icon != null) icon.SetActive(isSoundOn);
            }
        }

        // Music Off 아이콘들 처리
        if (musicOffIcons != null)
        {
            foreach (GameObject icon in musicOffIcons)
            {
                if (icon != null) icon.SetActive(!isSoundOn);
            }
        }
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

        // SFX On 아이콘들 처리
        if (sfxOnIcons != null)
        {
            foreach (GameObject icon in sfxOnIcons)
            {
                if (icon != null) icon.SetActive(isSoundOn);
            }
        }

        // SFX Off 아이콘들 처리
        if (sfxOffIcons != null)
        {
            foreach (GameObject icon in sfxOffIcons)
            {
                if (icon != null) icon.SetActive(!isSoundOn);
            }
        }
    }
}