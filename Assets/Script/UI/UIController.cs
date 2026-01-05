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
    public GameObject pauseButton;              // InGameUI의 일시정지 버튼

    public TextMeshProUGUI PasueScoreText;      // PausePanel의 점수 text
    public TextMeshProUGUI gameOverScoreText;   // GameOverPanel의 점수 text
    public TextMeshProUGUI countdownText;       // Text_ResumeDelay
    public TextMeshProUGUI gameOverOrClearText; // "GAME OVER" or "GAME CLEAR" 텍스트
    public GameObject newRecordText;

    public GameObject[] musicOnIcons;           // 소리 켜짐 아이콘들 (메인 + 일시정지)
    public GameObject[] musicOffIcons;          // 소리 꺼짐 아이콘들 (메인 + 일시정지)
    public GameObject[] sfxOnIcons;
    public GameObject[] sfxOffIcons;
    public Tutorial tutorial;

    // 재개 지연 시간
    public float resumeDelay = 3f;

    public static bool isRestarting = false;
    private bool isGameStarted = false;     // 게임 시작 여부
    private bool isCountingDown = false;    // 재개 카운트다운 여부
    
    public static bool tutorialSkip = false;         // 튜토리얼 스킵 여부 (false = 튜토리얼 실행, true = 스킵)


    void Awake()
    {
        Instance = this;
        
        // 씬이 로드될 때마다 static 변수들을 적절히 초기화
        // isRestarting이 false이면 게임을 처음 시작하는 것이므로 tutorialSkip도 false
        if (!isRestarting)
        {
            tutorialSkip = false;
            Debug.Log("First play - tutorialSkip set to false");
        }
        else
        {
            Debug.Log($"Restarting - tutorialSkip is {tutorialSkip}");
        }
    }

    void Start()
    {
        // 시작 메뉴만 활성화
        if (isRestarting)
        {
            isRestarting = false;
            // tutorialSkip 값은 RestartGame()에서 이미 true로 설정됨
            StartGame();
            // 재시작 시에도 아이콘 업데이트
            UpdateMusicIconUI();
            UpdateSFXIconUI();
        }
        else
        {
            // 처음 시작할 때는 tutorialSkip이 false로 유지됨 (튜토리얼 실행)
            gameStartRoot.SetActive(true);

            gamePauseRoot.SetActive(false);
            confirmationPanel.SetActive(false);
            inGameUIRoot.SetActive(false);
            countdownText.gameObject.SetActive(false);
            gameOverRoot.SetActive(false);


            Time.timeScale = 0f;

            UpdateMusicIconUI();
            UpdateSFXIconUI();

            // 메인 화면 BGM 재생
            if (BGMManager.Instance != null)
            {
                BGMManager.Instance.PlayMainScreenBGM();
            }
        }
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

        // Test: BGM 재생 테스트
        if (Input.GetKeyDown(KeyCode.M))
        {
            if (BGMManager.Instance != null)
            {
                BGMManager.Instance.PlayMainScreenBGM();
                Debug.Log("Playing Main Screen BGM (M key pressed)");
            }
        }

        if (Input.GetKeyDown(KeyCode.I))
        {
            if (BGMManager.Instance != null)
            {
                BGMManager.Instance.PlayInGameBGM();
                Debug.Log("Playing In-Game BGM (I key pressed)");
            }
        }

        if (Input.GetKeyDown(KeyCode.B))
        {
            if (BGMManager.Instance != null)
            {
                BGMManager.Instance.ToggleMute();
                Debug.Log("BGM Mute Toggled (B key pressed)");
            }
        }

        // Test: GameOver with S key
        //if (Input.GetKeyDown(KeyCode.S))
        //{
        //    EndGame();
        //}
    }

    // 게임 일시정지
    public void PauseGame()
    {
        if(tutorial.isPaused == true) return;
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
        
        // 일시정지 버튼만 숨김 (카운트다운 동안)
        if (pauseButton != null)
        {
            pauseButton.SetActive(false);
        }

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

        // 일시정지 버튼 다시 활성화
        if (pauseButton != null)
        {
            pauseButton.SetActive(true);
        }

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
        //gameStartRoot.gameObject.SetActive(true);
        
        // 메인 화면 BGM 재생
        if (BGMManager.Instance != null)
        {
            BGMManager.Instance.PlayMainScreenBGM();
        }
        
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        tutorialSkip = false;  
    }

    // 게임 오버 혹은 클리어 처리
    public void EndGame()
    {
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

        // 아이콘 상태 업데이트
        UpdateMusicIconUI();
        UpdateSFXIconUI();

        // 인게임 BGM 재생
        if (BGMManager.Instance != null)
        {
            BGMManager.Instance.PlayInGameBGM();
        }
    }

    public void RestartGame()
    {
        if (ScoreManager.Instance != null)
        {
            ScoreManager.Instance.ResetScore();
        }

        // Restart 버튼을 누르면 튜토리얼 스킵
        tutorialSkip = true;

        Time.timeScale = 1f;
        isRestarting = true;

        // 현재 씬 재로드: 처음부터 다시 시작
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    // --- Audio Control Methods ---

    public void DecreaseMusicVolume()
    {
        if (BGMManager.Instance != null)
        {
            BGMManager.Instance.DecreaseVolume();
            UpdateMusicIconUI();
        }
    }

    public void IncreaseMusicVolume()
    {
        if (BGMManager.Instance != null)
        {
            BGMManager.Instance.IncreaseVolume();
            UpdateMusicIconUI();
        }
    }

    public void DecreaseSFXVolume()
    {
        if (SFXManager.Instance != null)
        {
            SFXManager.Instance.DecreaseVolume();
            UpdateSFXIconUI();
        }
    }

    public void IncreaseSFXVolume()
    {
        if (SFXManager.Instance != null)
        {
            SFXManager.Instance.IncreaseVolume();
            UpdateSFXIconUI();
        }
    }


    // Music ( Mute / Unmute ) 구현
    public void ToggleMusicMute()
    {
        if (BGMManager.Instance != null)
        {
            BGMManager.Instance.ToggleMute();
            UpdateMusicIconUI();
        }
    }

    // 아이콘을 껐다 켰다 하는 함수
    private void UpdateMusicIconUI()
    {
        // 볼륨이 0보다 크면 On 아이콘 활성화, Off 아이콘 비활성화
        bool isSoundOn = BGMManager.Instance != null && !BGMManager.Instance.IsMuted();

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
        if (SFXManager.Instance != null)
        {
            SFXManager.Instance.ToggleMute();
            UpdateSFXIconUI();
        }
    }

    private void UpdateSFXIconUI()
    {
        bool isSoundOn = SFXManager.Instance != null && !SFXManager.Instance.IsMuted();

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