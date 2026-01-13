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
    private AsyncOperation preloadedScene = null;

    void Start()
    {
        // InGame 씬을 백그라운드에서 미리 로드
        StartCoroutine(PreloadInGameScene());
        
        // 메인 화면 BGM 재생
        if (BGMManager.Instance != null)
        {
            BGMManager.Instance.PlayMainScreenBGM();
        }

        // 아이콘 상태 업데이트
        UpdateMusicIconUI();
        UpdateSFXIconUI();
    }

    // 백그라운드에서 InGame 씬 프리로드
    private IEnumerator PreloadInGameScene()
    {
        // 약간의 지연 후 프리로드 시작 (메인 메뉴 초기화를 방해하지 않도록)
        yield return new WaitForSeconds(0.5f);
        
        // 백그라운드 로딩 우선순위 설정
        Application.backgroundLoadingPriority = ThreadPriority.Low;
        
        // 비동기로 씬 프리로드 (Single 모드)
        preloadedScene = SceneManager.LoadSceneAsync("InGame", LoadSceneMode.Single);
        
        // 자동 활성화 방지 (버튼 클릭 시까지 대기)
        preloadedScene.allowSceneActivation = false;
        
        // 90%까지 로드 대기
        while (preloadedScene.progress < 0.9f)
        {
            yield return null;
        }
    }

    // 게임 시작 버튼
    public void StartGame()
    {
        if (isLoadingScene) return;

        if (SFXManager.Instance != null)
        {
            SFXManager.Instance.Play("Button");
        }
        
        isLoadingScene = true;
        
        // 프리로드된 씬이 있으면 즉시 활성화
        if (preloadedScene != null && preloadedScene.progress >= 0.9f)
        {
            preloadedScene.allowSceneActivation = true;
        }
        else
        {
            // 프리로드가 안 된 경우 직접 로드
            SceneManager.LoadScene("InGame", LoadSceneMode.Single);
        }
    }

    // Achievement 버튼
    public void OpenAchievement()
    {
        if (isLoadingScene) return;

        if (SFXManager.Instance != null)
        {
            SFXManager.Instance.Play("Button");
        }
        
        isLoadingScene = true;
        SceneManager.LoadScene("AchivementAndCustomize", LoadSceneMode.Single);
    }

    // Customize 버튼
    public void OpenCustomize()
    {
        if (isLoadingScene) return;

        if (SFXManager.Instance != null)
        {
            SFXManager.Instance.Play("Button");
        }
        
        isLoadingScene = true;
        SceneManager.LoadScene("AchivementAndCustomize", LoadSceneMode.Single);
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
