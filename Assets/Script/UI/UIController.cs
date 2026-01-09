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

    // 프리로드된 씬들
    private AsyncOperation preloadedMainMenu = null;
    private AsyncOperation preloadedInGame = null;


    void Awake()
    {
        Instance = this;
        
        // 씬이 로드될 때마다 static 변수들을 적절히 초기화
        // isRestarting이 false이면 게임을 처음 시작하는 것이므로 tutorialSkip도 false
        if (!isRestarting)
        {
            tutorialSkip = false;
        }
    }

    void Start()
    {
        // InGame 씬에서는 항상 게임을 시작
        gamePauseRoot.SetActive(false);
        confirmationPanel.SetActive(false);
        gameOverRoot.SetActive(false);
        countdownText.gameObject.SetActive(false);
        
        // 재시작인지 확인
        if (isRestarting)
        {
            isRestarting = false;
            // tutorialSkip 값은 RestartGame()에서 이미 true로 설정됨
        }
        else
        {
            // 처음 시작할 때는 tutorialSkip이 false로 유지됨 (튜토리얼 실행)
            tutorialSkip = false;
        }
        
        // 게임 시작
        inGameUIRoot.SetActive(true);
        isGameStarted = true;
        Time.timeScale = 1f;

        UpdateMusicIconUI();
        UpdateSFXIconUI();

        // 인게임 BGM 재생
        if (BGMManager.Instance != null)
        {
            BGMManager.Instance.PlayInGameBGM();
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
    }

    // 게임 일시정지
    public void PauseGame()
    {
        if(tutorial != null && tutorial.isPaused == true) return;
        // 시간 멈춤
        Time.timeScale = 0f;

        inGameUIRoot.SetActive(false);
        gamePauseRoot.SetActive(true);

        // 최신 점수 업데이트
        if (ScoreManager.Instance != null)
        {
            UpdateScoreDisplay(ScoreManager.Instance.GetCurrentScore());
        }
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

        if (ScoreManager.Instance != null)
        {
            ScoreManager.Instance.ResetScore();
        }

        // UI 정리
        gamePauseRoot.SetActive(false);
        confirmationPanel.SetActive(false);
        gameOverRoot.SetActive(false);
        
        Time.timeScale = 1f;
        tutorialSkip = false;
        isRestarting = false;
        
        // 모든 코루틴 중단
        StopAllCoroutines();
        
        // 프리로드된 씬이 있으면 사용, 없으면 직접 로드
        if (preloadedMainMenu != null && preloadedMainMenu.progress >= 0.9f)
        {
            Debug.Log("프리로드된 MainMenu 씬 활성화");
            StartCoroutine(SwitchToPreloadedScene(preloadedMainMenu, "MainMenu"));
        }
        else
        {
            Debug.Log("MainMenu 씬 직접 로드");
            SceneManager.LoadScene("MainMenu", LoadSceneMode.Single);
        }
        
        // 프리로드된 씬 정리
        preloadedMainMenu = null;
        preloadedInGame = null;
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
        
        // 프리로드는 이미 GameOver 코루틴 시작 시점에 시작됨
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

    public void RestartGame()
    {
        // Time.timeScale을 먼저 복구
        Time.timeScale = 1f;
        
        // Restart 상태 설정
        isRestarting = true;
        tutorialSkip = true;
        
        // ScoreManager 리셋
        if (ScoreManager.Instance != null)
        {
            ScoreManager.Instance.ResetScore();
        }

        // 모든 코루틴 중단
        StopAllCoroutines();
        
        // 프리로드된 씬이 있으면 사용, 없으면 직접 로드
        if (preloadedInGame != null && preloadedInGame.progress >= 0.9f)
        {
            Debug.Log("프리로드된 InGame 씬 활성화");
            StartCoroutine(SwitchToPreloadedScene(preloadedInGame, "InGame"));
        }
        else
        {
            Debug.Log("InGame 씬 직접 로드");
            SceneManager.LoadScene("InGame", LoadSceneMode.Single);
        }
        
        // 프리로드된 씬 정리
        preloadedMainMenu = null;
        preloadedInGame = null;
    }

    // 비동기 씬 로딩 (제거 예정 - 프리로드 방식 사용)
    private IEnumerator LoadSceneAsync(string sceneName)
    {
        // 이 함수는 더 이상 사용하지 않음
        yield break;
    }
    
    // Additive로 프리로드된 씬으로 전환 (현재 씬 언로드)
    private IEnumerator SwitchToPreloadedScene(AsyncOperation preloadedScene, string sceneName)
    {
        // 프리로드된 씬 활성화
        preloadedScene.allowSceneActivation = true;
        
        // 씬이 완전히 로드될 때까지 대기
        while (!preloadedScene.isDone)
        {
            yield return null;
        }
        
        // 현재 InGame 씬 언로드
        yield return SceneManager.UnloadSceneAsync("InGame");
        
        // 새 씬을 활성 씬으로 설정
        UnityEngine.SceneManagement.Scene newScene = SceneManager.GetSceneByName(sceneName);
        if (newScene.isLoaded)
        {
            SceneManager.SetActiveScene(newScene);
        }
    }

    // 게임 오버 시 씬들을 백그라운드에서 프리로드
    public void StartPreloadScenes()
    {
        StartCoroutine(PreloadScenesForGameOver());
    }
    
    private IEnumerator PreloadScenesForGameOver()
    {
        Debug.Log("씬 프리로드 시작 (충돌 시점)");
        
        // Additive 모드로 MainMenu 씬 프리로드 (백그라운드에 로드)
        preloadedMainMenu = SceneManager.LoadSceneAsync("MainMenu", LoadSceneMode.Additive);
        preloadedMainMenu.allowSceneActivation = false;
        
        // Additive 모드로 InGame 씬 프리로드 (백그라운드에 로드)
        preloadedInGame = SceneManager.LoadSceneAsync("InGame", LoadSceneMode.Additive);
        preloadedInGame.allowSceneActivation = false;
        
        // 두 씬 모두 로드 완료될 때까지 대기
        while (preloadedMainMenu.progress < 0.9f || preloadedInGame.progress < 0.9f)
        {
            yield return null;
        }
        
        Debug.Log("MainMenu & InGame 씬 프리로드 완료");
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