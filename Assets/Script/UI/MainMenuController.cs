using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class MainMenuController : MonoBehaviour
{
    public GameObject Settings;
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

        // 저장된 커스터마이즈 자동 적용
        ApplySavedCustomize();
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

        UIController.tutorialSkip = true;
        UIController.isRestarting = false;

        if (SFXManager.Instance != null)
        {
            SFXManager.Instance.Play("Button");
        }
        
        // Start 버튼 클릭 시 Newbie 업적 달성
        if (PlayerAchivementList.Instance != null)
        {
            PlayerAchivementList.Instance.Newbie();
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

    // 튜토리얼 버튼
    public void StartTutorial()
    {
        if (isLoadingScene) return;

        UIController.tutorialSkip = false;
        UIController.isRestarting = false;

        if (SFXManager.Instance != null)
        {
            SFXManager.Instance.Play("Button");
        }
        
        // Start 버튼 클릭 시 Newbie 업적 달성
        if (PlayerAchivementList.Instance != null)
        {
            PlayerAchivementList.Instance.Newbie();
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
        
        // Achievement 모드로 설정
        PlayerPrefs.SetString("AchievementCustomizeMode", "Achievement");
        PlayerPrefs.Save();
        
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
        
        // Customize 모드로 설정
        PlayerPrefs.SetString("AchievementCustomizeMode", "Customize");
        PlayerPrefs.Save();
        
        isLoadingScene = true;
        SceneManager.LoadScene("AchivementAndCustomize", LoadSceneMode.Single);
    }

    // Settings 버튼
    public void OpenSettings()
    {
        if (isLoadingScene) return;

        if (SFXManager.Instance != null)
        {
            SFXManager.Instance.Play("Button");
        }

        Settings.SetActive(true);
    }

    // Settings 버튼
    public void CloseSettings()
    {
        if (isLoadingScene) return;

        if (SFXManager.Instance != null)
        {
            SFXManager.Instance.Play("Button");
        }

        Settings.SetActive(false);
    }



    // --- Audio Control Methods ---

    public void DecreaseMusicVolume()
    {
        if (BGMManager.Instance != null)
        {
            SFXManager.Instance.Play("Button");
            BGMManager.Instance.DecreaseVolume();
            UpdateMusicIconUI();
        }
    }

    public void IncreaseMusicVolume()
    {
        if (BGMManager.Instance != null)
        {
            SFXManager.Instance.Play("Button");
            BGMManager.Instance.IncreaseVolume();
            UpdateMusicIconUI();
        }
    }

    public void DecreaseSFXVolume()
    { 
        if (SFXManager.Instance != null)
        {
            SFXManager.Instance.Play("Button");
            SFXManager.Instance.DecreaseVolume();
            UpdateSFXIconUI();
        }
    }

    public void IncreaseSFXVolume()
    {
        if (SFXManager.Instance != null)
        {
            SFXManager.Instance.Play("Button");
            SFXManager.Instance.IncreaseVolume();
            UpdateSFXIconUI();
        }
    }

    public void ToggleMusicMute()
    {
        if (BGMManager.Instance != null)
        {
            SFXManager.Instance.Play("Button");
            BGMManager.Instance.ToggleMute();
            UpdateMusicIconUI();
        }
    }

    public void ToggleSFXMute()
    {
        if (SFXManager.Instance != null)
        {
            SFXManager.Instance.Play("Button");
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

    // 저장된 커스터마이즈 정보가 있으면 적용합니다.
    private void ApplySavedCustomize()
    {
        if (Customize.Instance == null) return;

        int playerIndex = PlayerPrefs.GetInt("SelectedPlayerSkinIndex", -1);
        int ropeIndex = PlayerPrefs.GetInt("SelectedRopeSkinIndex", -1);
        int hookIndex = PlayerPrefs.GetInt("SelectedHookSkinIndex", -1);
        int heliIndex = PlayerPrefs.GetInt("SelectedHelicopterSkinIndex", -1);
        bool beanieEquipped = PlayerPrefs.GetInt("SelectedBeanieEquipped", 0) == 1;
        int beanieSkinIndex = PlayerPrefs.GetInt("SelectedBeanieSkinIndex", -1);
        bool bagEquipped = PlayerPrefs.GetInt("SelectedBagEquipped", 0) == 1;
        int bagSkinIndex = PlayerPrefs.GetInt("SelectedBagSkinIndex", -1);

        // Customize 시스템에 반영 (메인 메뉴나 미리보기에서 즉시 반영)
        Debug.Log($"ApplySavedCustomize loaded: playerIndex={playerIndex}, ropeIndex={ropeIndex}, hookIndex={hookIndex}, heliIndex={heliIndex}, beanieEquipped={beanieEquipped}, beanieSkinIndex={beanieSkinIndex}, bagEquipped={bagEquipped}, bagSkinIndex={bagSkinIndex}");

        if (playerIndex >= 0)
        {
            if (playerIndex < Customize.Instance.playerSkins.Count)
            {
                Customize.Instance.EquipPlayerSkinNumber(playerIndex);
                Debug.Log($"Applied Player skin index {playerIndex}");
            }
            else Debug.LogWarning($"Player skin index {playerIndex} out of range (0..{Customize.Instance.playerSkins.Count - 1})");
        }

        if (ropeIndex >= 0)
        {
            if (ropeIndex < Customize.Instance.ropeSkins.Count)
            {
                Customize.Instance.EquipRopeSkinNumber(ropeIndex);
                Debug.Log($"Applied Rope skin index {ropeIndex}");
            }
            else Debug.LogWarning($"Rope skin index {ropeIndex} out of range (0..{Customize.Instance.ropeSkins.Count - 1})");
        }

        if (hookIndex >= 0)
        {
            if (hookIndex < Customize.Instance.hookSkins.Count)
            {
                Customize.Instance.EquipHookSkinNumber(hookIndex);
                Debug.Log($"Applied Hook skin index {hookIndex}");
            }
            else Debug.LogWarning($"Hook skin index {hookIndex} out of range (0..{Customize.Instance.hookSkins.Count - 1})");
        }

        if (heliIndex >= 0)
        {
            if (heliIndex < Customize.Instance.helicopterSkins.Count)
            {
                Customize.Instance.EquipHelicopterSkinNumber(heliIndex);
                Debug.Log($"Applied Helicopter skin index {heliIndex}");
            }
            else Debug.LogWarning($"Helicopter skin index {heliIndex} out of range (0..{Customize.Instance.helicopterSkins.Count - 1})");
        }

        if (beanieEquipped)
        {
            Customize.Instance.EquipBeanie();
            Debug.Log("Equipped Beanie");
            if (beanieSkinIndex >= 0)
            {
                if (beanieSkinIndex < Customize.Instance.beanieSkins.Count)
                {
                    Customize.Instance.ChangeBeanieSkinNumber(beanieSkinIndex);
                    Debug.Log($"Applied Beanie skin index {beanieSkinIndex}");
                }
                else Debug.LogWarning($"Beanie skin index {beanieSkinIndex} out of range (0..{Customize.Instance.beanieSkins.Count - 1})");
            }
        }
        else
        {
            Customize.Instance.UnequipBeanie();
            Debug.Log("Unequipped Beanie");
        }

        if (bagEquipped)
        {
            Customize.Instance.EquipBag();
            Debug.Log("Equipped Bag");
            if (bagSkinIndex >= 0)
            {
                if (bagSkinIndex < Customize.Instance.bagSkins.Count)
                {
                    Customize.Instance.EquipBagSkinNumber(bagSkinIndex);
                    Debug.Log($"Applied Bag skin index {bagSkinIndex}");
                }
                else Debug.LogWarning($"Bag skin index {bagSkinIndex} out of range (0..{Customize.Instance.bagSkins.Count - 1})");
            }
        }
        else
        {
            Customize.Instance.UnequipBag();
            Debug.Log("Unequipped Bag");
        }

        // Hook/Helicopter static 필드에도 적용하여 이후 생성되는 오브젝트가 바로 반영되도록 함
        if (ropeIndex >= 0 && ropeIndex < Customize.Instance.ropeSkins.Count)
        {
            Hook.currentSkin = Customize.Instance.ropeSkins[ropeIndex];
            Debug.Log($"Set Hook.currentSkin from rope index {ropeIndex}");
        }

        if (hookIndex >= 0 && hookIndex < Customize.Instance.hookSkins.Count)
        {
            Hook.currentSkin = Customize.Instance.hookSkins[hookIndex];
            Debug.Log($"Set Hook.currentSkin from hook index {hookIndex}");
        }

        if (heliIndex >= 0 && heliIndex < Customize.Instance.helicopterSkins.Count)
        {
            Helicopter.currentSkin = Customize.Instance.helicopterSkins[heliIndex];
            Debug.Log($"Set Helicopter.currentSkin from helicopter index {heliIndex}");
        }

        // Player 인스턴스(메인 메뉴에 미리보기 등이 있으면)에 바로 적용
        if (Player.Instance != null)
        {
            if (playerIndex >= 0 && playerIndex < Customize.Instance.playerSkins.Count)
            {
                Player.Instance.ChangeSkinTexture(Customize.Instance.playerSkins[playerIndex]);
                Debug.Log($"Player.Instance: applied player skin index {playerIndex}");
            }

            if (ropeIndex >= 0 && ropeIndex < Customize.Instance.ropeSkins.Count)
            {
                Player.Instance.ChangeRopeMaterial(Customize.Instance.ropeSkins[ropeIndex]);
                Debug.Log($"Player.Instance: applied rope material index {ropeIndex}");
            }

            if (beanieEquipped && beanieSkinIndex >= 0 && beanieSkinIndex < Customize.Instance.beanieSkins.Count)
            {
                Player.Instance.ChangeBeanieSkin(Customize.Instance.beanieSkins[beanieSkinIndex]);
                Debug.Log($"Player.Instance: applied beanie skin index {beanieSkinIndex}");
            }

            if (bagEquipped && bagSkinIndex >= 0 && bagSkinIndex < Customize.Instance.bagSkins.Count)
            {
                Player.Instance.ChangeBagSkin(Customize.Instance.bagSkins[bagSkinIndex]);
                Debug.Log($"Player.Instance: applied bag skin index {bagSkinIndex}");
            }
        }

        // Store selections into Player static fields so the in-game Player (created later) can apply them on Start()
        if (playerIndex >= 0 && playerIndex < Customize.Instance.playerSkins.Count)
            Player.selectedPlayerSkinTexture = Customize.Instance.playerSkins[playerIndex];
        else
            Player.selectedPlayerSkinTexture = null;

        Player.selectedBeanieEquippedStatic = beanieEquipped;
        if (beanieSkinIndex >= 0 && beanieSkinIndex < Customize.Instance.beanieSkins.Count)
            Player.selectedBeanieSkinTexture = Customize.Instance.beanieSkins[beanieSkinIndex];
        else
            Player.selectedBeanieSkinTexture = null;
        Player.selectedBeaniePrefab = Customize.Instance.beaniePrefab;

        Player.selectedBagEquippedStatic = bagEquipped;
        if (bagSkinIndex >= 0 && bagSkinIndex < Customize.Instance.bagSkins.Count)
            Player.selectedBagSkinTexture = Customize.Instance.bagSkins[bagSkinIndex];
        else
            Player.selectedBagSkinTexture = null;
        Player.selectedBagPrefab = Customize.Instance.bagPrefab;
    }
}
