using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class MainMenuController : MonoBehaviour
{
    public GameObject[] musicOnIcons;
    public GameObject[] musicOffIcons;
    public GameObject[] sfxOnIcons;
    public GameObject[] sfxOffIcons;

    private bool isLoadingScene = false;
    private AsyncOperation preloadedInGame = null;

    void Start()
    {
        // 메인 화면 BGM 재생
        if (BGMManager.Instance != null)
        {
            BGMManager.Instance.PlayMainScreenBGM();
        }

        // 아이콘 상태 업데이트
        UpdateMusicIconUI();
        UpdateSFXIconUI();

        // InGame 씬 프리로드 시작 (백그라운드)
        StartCoroutine(PreloadInGameScene());
    }

    // 게임 시작 버튼
    public void StartGame()
    {
        if (isLoadingScene) return;

        if (SFXManager.Instance != null)
        {
            SFXManager.Instance.Play("Button");
        }
        
        // 프리로드된 씬이 있으면 즉시 활성화, 없으면 일반 로딩
        if (preloadedInGame != null && preloadedInGame.progress >= 0.9f)
        {
            isLoadingScene = true;
            preloadedInGame.allowSceneActivation = true;
            preloadedInGame = null; // 사용 완료
        }
        else
        {
            StartCoroutine(LoadSceneAsync("InGame"));
        }
    }

    // 비동기 씬 로딩
    private IEnumerator LoadSceneAsync(string sceneName)
    {
        isLoadingScene = true;

        // 비동기로 씬 로드 시작
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneName);
        
        // allowSceneActivation을 false로 설정하면 로딩 완료 후 수동으로 활성화 가능
        asyncLoad.allowSceneActivation = false;

        // 로딩 진행률 체크 (0.9까지만 진행됨)
        while (asyncLoad.progress < 0.9f)
        {
            yield return null;
        }

        // 로딩 완료, 씬 활성화
        asyncLoad.allowSceneActivation = true;
    }

    // InGame 씬 프리로드 (메인 메뉴 로드 시 백그라운드에서 실행)
    private IEnumerator PreloadInGameScene()
    {
        // 프리로드 초기화
        preloadedInGame = null;

        // InGame 씬 프리로드 (90%까지)
        preloadedInGame = SceneManager.LoadSceneAsync("InGame");
        preloadedInGame.allowSceneActivation = false; // 자동 활성화 방지

        // 90%까지 로드될 때까지 대기
        while (preloadedInGame.progress < 0.9f)
        {
            yield return null;
        }

        Debug.Log("InGame 씬 프리로드 완료 - 시작 버튼 클릭 시 즉시 전환 가능");
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

    public void ToggleMusicMute()
    {
        if (BGMManager.Instance != null)
        {
            BGMManager.Instance.ToggleMute();
            UpdateMusicIconUI();
        }
    }

    public void ToggleSFXMute()
    {
        if (SFXManager.Instance != null)
        {
            SFXManager.Instance.ToggleMute();
            UpdateSFXIconUI();
        }
    }

    private void UpdateMusicIconUI()
    {
        bool isSoundOn = BGMManager.Instance != null && !BGMManager.Instance.IsMuted();

        if (musicOnIcons != null)
        {
            foreach (GameObject icon in musicOnIcons)
            {
                if (icon != null) icon.SetActive(isSoundOn);
            }
        }

        if (musicOffIcons != null)
        {
            foreach (GameObject icon in musicOffIcons)
            {
                if (icon != null) icon.SetActive(!isSoundOn);
            }
        }
    }

    private void UpdateSFXIconUI()
    {
        bool isSoundOn = SFXManager.Instance != null && !SFXManager.Instance.IsMuted();

        if (sfxOnIcons != null)
        {
            foreach (GameObject icon in sfxOnIcons)
            {
                if (icon != null) icon.SetActive(isSoundOn);
            }
        }

        if (sfxOffIcons != null)
        {
            foreach (GameObject icon in sfxOffIcons)
            {
                if (icon != null) icon.SetActive(!isSoundOn);
            }
        }
    }
}
