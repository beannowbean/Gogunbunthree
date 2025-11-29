using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

/*
 * 튜토리얼 타일 생성 method 들어있는 script에서 참조 필요
 * 변수 형식: boolean
 * UIController.Instance.IsFirstPlay
 */

public class UIController : MonoBehaviour
{
    public static UIController Instance;

    public GameObject gamePauseRoot;            // PauseePanel Root
    public GameObject inGameUIRoot;             // InGamePanel Root
    public GameObject gameStartRoot;            // StartPanel Root
    public GameObject confirmationPanel;        // QuitReconfirm Root
    public GameObject gameOverRoot;             // GameOverPanel Root

    public TextMeshProUGUI PasueScoreText;      // PausePanel의 점수 text
    public TextMeshProUGUI gameOverScoreText;   // GameOverPanel의 점수 text
    public TextMeshProUGUI countdownText;       // Text_ResumeDelay
    public TextMeshProUGUI gameOverOrClearText; // "GAME OVER" or "GAME CLEAR" 텍스트
    public GameObject newRecordText;

    public GameObject[] musicOnIcons;           // 소리 켜짐 아이콘들 (메인 + 일시정지)
    public GameObject[] musicOffIcons;          // 소리 꺼짐 아이콘들 (메인 + 일시정지)
    public GameObject[] sfxOnIcons;
    public GameObject[] sfxOffIcons;



    // Audio volume levels
    private const int MAX_VOLUME_LEVEL = 10;

    private int currentMusicVolumeLevel = 3;
    private int currentSFXVolumeLevel = 4;

    // 음소거 전 저장용
    private int previousMusicVolumeLevel = MAX_VOLUME_LEVEL;
    private int previousSFXVolumeLevel = MAX_VOLUME_LEVEL;

    // 재개 지연 시간
    public float resumeDelay = 3f;

    private bool isGameStarted = false;     // 게임 시작 여부
    private bool isCountingDown = false;    // 재개 카운트다운 여부
    
    public bool isFirstPlay = true;         // 첫 플레이 여부


    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        // 시작 메뉴만 활성화
        gameStartRoot.SetActive(true);

        gamePauseRoot.SetActive(false);
        confirmationPanel.SetActive(false);
        inGameUIRoot.SetActive(false);
        countdownText.gameObject.SetActive(false);
        gameOverRoot.SetActive(false);


        Time.timeScale = 0f;

        UpdateMusicIconUI();
        UpdateSFXIconUI();
    }

    void Update()
    {
        // 게임시작 후에, ResumeCountdown이 아닐 때, Escape 키로 일시정지 토글
        if (Input.GetKeyDown(KeyCode.Escape) && !isCountingDown && isGameStarted)
        {
            if (confirmationPanel.activeSelf) confirmationPanel.SetActive(false); 
            else if (gamePauseRoot.activeSelf) ResumeGame();
            else PauseGame();
        }

        // Test: GameOver with S key
        if (Input.GetKeyDown(KeyCode.S))
        {
            EndGame();
        }
    }

    // 게임 일시정지
    public void PauseGame()
    {
        // 시간 멈춤
        Time.timeScale = 0f;

        inGameUIRoot.SetActive(false);
        gamePauseRoot.SetActive(true);

        // 최신 점수 업데이트
        UpdateScoreDisplay(ScoreManager.Instance.GetCurrentScore());
    }

    // 게임 재개
    public void ResumeGame()
    {
        // Countdown이 진행 중이면, confirmationPanel이 켜져있으면 무시
        if (isCountingDown) return;
        if (confirmationPanel.activeSelf) return;

        StartCoroutine(ResumeAfterDelay());
    }

    // Countdown 후 Resume
    private IEnumerator ResumeAfterDelay()
    {
        isCountingDown = true;
        float timer = resumeDelay;

        // Ui 전환
        gamePauseRoot.SetActive(false);
        inGameUIRoot.SetActive(true);

        // Countdown text 활성화
        countdownText.gameObject.SetActive(true);

        // Time.realtimeSinceStartup을 사용하여 TimeScale = 0f 상태에서도 타이머가 작동하게 합니다.
        while (timer > 0)
        {
            countdownText.text = $"STARTING IN\n{Mathf.CeilToInt(timer)}";
            yield return null; // 다음 프레임까지 대기
            timer -= Time.unscaledDeltaTime; // TimeScale에 영향을 받지 않는 시간 사용
        }

        // Countdown text 비활성화
        countdownText.gameObject.SetActive(false);

        isCountingDown = false;

        // 게임 재개
        Time.timeScale = 1f;
    }

    // 재확인 팝업만 끄고, 뒤에 있는 Pause 메뉴를 유지
    public void CancelQuit()
    {
        confirmationPanel.SetActive(false);
    }

    // 종료 재확인 팝업 켜기
    public void QuitGame()
    {
        confirmationPanel.SetActive(true);
    }

    // 게임 종료 후, 메인 메뉴로 이동
    public void ConfirmQuitGame()
    {
        isGameStarted = false;

        ScoreManager.Instance.ResetScore();

        // UI 정리
        gamePauseRoot.SetActive(false);
        confirmationPanel.SetActive(false);
        gameOverRoot.SetActive(false);

        // 시작 메뉴로 돌아가기
        gameStartRoot.gameObject.SetActive(true);
    }

    // 게임 오버 혹은 클리어 처리
    public void EndGame()
    {
        isFirstPlay = false;

        Time.timeScale = 0f;
        isGameStarted = false;

        // in-game UI 및 일시정지 UI 비활성화
        inGameUIRoot.SetActive(false);
        gamePauseRoot.SetActive(false);
        confirmationPanel.SetActive(false);

        // game over UI 활성화
        gameOverRoot.SetActive(true);

        // Game Over / Game Clear 텍스트 설정
        if (ScoreManager.Instance.isCleared)
        {
            gameOverOrClearText.text = "GAME\nCLEAR";
        }
        else
        {
            gameOverOrClearText.text = "GAME\nOVER";
        }

        // 점수 표시
        UpdateScoreDisplay(ScoreManager.Instance.GetFinalScore());

        // 4. 뉴 레코드 메시지 활성화/비활성화
        bool isNewRecord = ScoreManager.Instance != null && ScoreManager.Instance.GetIsNewRecord();
        newRecordText.SetActive(isNewRecord);

        // 점수 초기화
        ScoreManager.Instance.ResetScore();
    }

    // 점수 표시
    public void UpdateScoreDisplay(int Score)
    {
        if (isGameStarted)
        {
            // PausePanel 점수 업데이트
            PasueScoreText.text = Score.ToString();
        }
        else
        {
            // GameOverPanel 점수 업데이트
            gameOverScoreText.text = Score.ToString();
        }
    }

    public void StartGame()
    {
        gameStartRoot.SetActive(false);
        gameOverRoot.SetActive(false);
        inGameUIRoot.SetActive(true);

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