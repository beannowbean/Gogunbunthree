using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenuController : MonoBehaviour
{
    [Header("UI Containers (Buttons)")]
    public RectTransform uiContainer; // 버튼들을 감싸고 있는 부모 객체
    public float uiSlideDuration = 0.5f;

    [Header("Title Image Animation")]
    public RectTransform titleImageRect; // 게임 로고 이미지
    public float titleSlideDuration = 0.7f;
    private Vector2 titleFinalPos;

    [Header("Skin UI References")]
    public Image bodyImage;
    public Image beanieImage;
    public Image bagImage;
    public Image hookImage;
    public Image helicopterImage;

    [Header("Data Reference")]
    public SkinDatabase skinData;

    [Header("Helicopter Animation")]
    public RectTransform helicopterRect; // 헬리콥터 이미지
    public AudioSource heliAudioSource;  // 헬리콥터 소리가 나는 AudioSource

    // 헬리콥터 좌표 및 크기 설정
    private Vector2 heliStartPos = new Vector2(-100, 6000);
    private Vector2 heliSizeLarge = new Vector2(8000, 8000);
    private Vector2 heliLandPos = new Vector2(-100, 3100);
    private Vector2 heliFinalPos = new Vector2(400, 1000);
    private Vector2 heliSizeSmall = new Vector2(100, 100);

    [Header("Character Animation")]
    public RectTransform characterRect; // 캐릭터 이미지
    public Animator characterAnimator;  // 캐릭터 애니메이터

    // 캐릭터 좌표 및 크기 설정
    private Vector2 charStartPos = new Vector2(-100, 0);
    private Vector2 charSizeLarge = new Vector2(2000, 2000);
    private Vector2 charFinalPos = new Vector2(390, 955);
    private Vector2 charSizeSmall = new Vector2(25, 25);

    [Header("Settings & Audio UI")]
    public GameObject Settings; // 설정 팝업창
    public GameObject[] musicOnIcons; // 뮤직 켜짐 아이콘들
    public GameObject[] musicOffIcons; // 뮤직 꺼짐 아이콘들
    public GameObject[] sfxOnIcons;   // 효과음 켜짐 아이콘들
    public GameObject[] sfxOffIcons;  // 효과음 꺼짐 아이콘들

    // 상태 변수
    private bool isLoadingScene = false;


    void Start()
    {
        UpdateMainMenuCharacterSkin();

        // 1. 타이틀 이미지 위치 초기화
        if (titleImageRect != null)
        {
            titleFinalPos = titleImageRect.anchoredPosition;
            float offScreenY = Screen.height / 2f + titleImageRect.rect.height;
            titleImageRect.anchoredPosition = new Vector2(titleFinalPos.x, offScreenY);
        }

        // 2. 헬리콥터 초기화
        if (helicopterRect != null)
        {
            helicopterRect.anchoredPosition = heliStartPos;
            helicopterRect.sizeDelta = heliSizeLarge;
        }

        // 3. 캐릭터 초기화
        if (characterRect != null)
        {
            characterRect.anchoredPosition = charStartPos;
            characterRect.sizeDelta = charSizeLarge;
        }

        // 4. 메인 BGM 재생
        if (BGMManager.Instance != null)
        {
            BGMManager.Instance.PlayMainScreenBGM();
        }
    }

    public void StartGame()
    {
        if (isLoadingScene) return;

        isLoadingScene = true;
        UIController.tutorialSkip = true;
        UIController.isRestarting = false;

        // 버튼 클릭 효과음 & 업적
        if (SFXManager.Instance != null) SFXManager.Instance.Play("Button");
        if (PlayerAchivementList.Instance != null) PlayerAchivementList.Instance.Newbie();


        // 1. 타이틀 이미지 내려오기
        if (titleImageRect != null) StartCoroutine(SlideInTitle());

        // 2. 버튼들 오른쪽으로 날리기 (SlideOutUI)
        if (uiContainer != null) StartCoroutine(SlideOutUI());

        // 3. 헬리콥터 시퀀스
        if (helicopterRect != null)
        {
            StartCoroutine(HelicopterSequence());
        }
        else
        {
            // 헬리콥터가 없으면 데이터 저장 후 바로 로딩
            ApplySavedCustomizeStaticsOnly();
            LoadInGameScene();
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

        // 1. 타이틀 이미지 내려오기
        if (titleImageRect != null) StartCoroutine(SlideInTitle());

        // 2. 버튼들 오른쪽으로 날리기 (SlideOutUI)
        if (uiContainer != null) StartCoroutine(SlideOutUI());

        // 3. 헬리콥터 시퀀스
        if (helicopterRect != null)
        {
            StartCoroutine(HelicopterSequence());
        }
        else
        {
            // 헬리콥터가 없으면 데이터 저장 후 바로 로딩
            ApplySavedCustomizeStaticsOnly();
            LoadInGameScene();
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

    public void OpenRanking()
    {
        if (isLoadingScene) return;

        if (SFXManager.Instance != null)
        {
            SFXManager.Instance.Play("Button");
        }

        isLoadingScene = true;

        SceneManager.LoadScene("Ranking", LoadSceneMode.Single);
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

        UpdateMusicIconUI();
        UpdateSFXIconUI();
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

    private float GetTargetHelicopterVolume()
    {
        if (SFXManager.Instance == null) return 1.0f;

        if (SFXManager.Instance.IsMuted()) return 0f;

        float volumeRatio = (float)SFXManager.Instance.GetCurrentVolumeLevel() / 10f;

        return volumeRatio;
    }

    IEnumerator HelicopterSequence()
    {
        if (characterAnimator != null) characterAnimator.enabled = false;

        if (BGMManager.Instance != null) BGMManager.Instance.FadeTo(0f, 2.0f);

        if (heliAudioSource != null)
        {
            heliAudioSource.volume = 0f;
            heliAudioSource.Play();
            float targetVol = GetTargetHelicopterVolume();
            StartCoroutine(FadeAudioSource(heliAudioSource, targetVol, 2.0f));
        }

        // 1단계: 대기
        yield return new WaitForSeconds(1.0f);

        // 2단계: 헬리콥터 하강
        float timer = 0f;
        float duration = 1.0f;
        Vector2 heliCurrentStart = helicopterRect.anchoredPosition;

        while (timer < duration)
        {
            timer += Time.unscaledDeltaTime;
            float t = timer / duration;
            t = Mathf.Sin(t * Mathf.PI * 0.5f);
            helicopterRect.anchoredPosition = Vector2.Lerp(heliCurrentStart, heliLandPos, t);
            yield return null;
        }
        helicopterRect.anchoredPosition = heliLandPos;

        // 3단계: 반동 (Bounce)
        Vector2 heliBounceDest = heliLandPos + new Vector2(0, -50);
        Vector2 charBounceDest = charStartPos + new Vector2(0, -50);

        if (characterRect != null) StartCoroutine(MoveTo(characterRect, charBounceDest, 0.2f));
        yield return StartCoroutine(MoveTo(helicopterRect, heliBounceDest, 0.2f));

        if (characterRect != null) StartCoroutine(MoveTo(characterRect, charStartPos, 0.2f));
        yield return StartCoroutine(MoveTo(helicopterRect, heliLandPos, 0.2f));

        ApplySavedCustomizeStaticsOnly();

        // 씬 로딩 시작
        LoadInGameScene();

        // 헬기 소리 Fade Out
        if (heliAudioSource != null)
        {
            StartCoroutine(FadeAudioSource(heliAudioSource, 0f, 1.0f));
        }

        // 이동 및 축소
        timer = 0f;
        duration = 1.0f;

        Vector2 heliCurPos = helicopterRect.anchoredPosition;
        Vector2 heliCurSize = helicopterRect.sizeDelta;
        Vector2 charCurPos = characterRect != null ? characterRect.anchoredPosition : charStartPos;
        Vector2 charCurSize = characterRect != null ? characterRect.sizeDelta : charSizeLarge;

        while (timer < duration)
        {
            timer += Time.unscaledDeltaTime;
            float t = timer / duration;
            float easeT = t * t;

            helicopterRect.anchoredPosition = Vector2.Lerp(heliCurPos, heliFinalPos, easeT);
            helicopterRect.sizeDelta = Vector2.Lerp(heliCurSize, heliSizeSmall, easeT);

            if (characterRect != null)
            {
                characterRect.anchoredPosition = Vector2.Lerp(charCurPos, charFinalPos, easeT);
                characterRect.sizeDelta = Vector2.Lerp(charCurSize, charSizeSmall, easeT);
            }

            yield return null;
        }

        helicopterRect.anchoredPosition = heliFinalPos;
        helicopterRect.sizeDelta = heliSizeSmall;

        if (characterRect != null)
        {
            characterRect.anchoredPosition = charFinalPos;
            characterRect.sizeDelta = charSizeSmall;
        }
    }

    IEnumerator MoveTo(RectTransform target, Vector2 dest, float time)
    {
        float t = 0f;
        Vector2 start = target.anchoredPosition;
        while (t < time)
        {
            t += Time.unscaledDeltaTime;
            target.anchoredPosition = Vector2.Lerp(start, dest, t / time);
            yield return null;
        }
        target.anchoredPosition = dest;
    }

    IEnumerator SlideInTitle()
    {
        float timer = 0f;
        Vector2 startPos = titleImageRect.anchoredPosition;
        while (timer < titleSlideDuration)
        {
            timer += Time.unscaledDeltaTime;
            float t = timer / titleSlideDuration;
            float easeT = Mathf.Sin(t * Mathf.PI * 0.5f);
            titleImageRect.anchoredPosition = Vector2.Lerp(startPos, titleFinalPos, easeT);
            yield return null;
        }
        titleImageRect.anchoredPosition = titleFinalPos;
    }

    // UI 버튼 퇴장 코루틴
    IEnumerator SlideOutUI()
    {
        float timer = 0f;
        Vector2 startPos = uiContainer.anchoredPosition;
        Vector2 targetPos = new Vector2(startPos.x + Screen.width * 1.5f, startPos.y);

        while (timer < uiSlideDuration)
        {
            timer += Time.unscaledDeltaTime;
            float t = timer / uiSlideDuration;
            float easeT = t * t;
            uiContainer.anchoredPosition = Vector2.Lerp(startPos, targetPos, easeT);
            yield return null;
        }
        uiContainer.anchoredPosition = targetPos;
    }

    IEnumerator FadeAudioSource(AudioSource source, float targetVol, float duration)
    {
        float startVol = source.volume;
        float timer = 0f;
        while (timer < duration)
        {
            timer += Time.unscaledDeltaTime;
            source.volume = Mathf.Lerp(startVol, targetVol, timer / duration);
            yield return null;
        }
        source.volume = targetVol;
    }

    void LoadInGameScene()
    {
        if (SceneTransitionManager.Instance != null)
        {
            SceneTransitionManager.Instance.LoadScene("InGame");
        }
        else
        {
            SceneManager.LoadScene("InGame", LoadSceneMode.Single);
        }
    }

    // 커스터마이징 데이터 적용 함수
    private void ApplySavedCustomizeStaticsOnly()
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

        // 1. 플레이어 스킨
        if (playerIndex >= 0 && playerIndex < Customize.Instance.playerSkins.Count)
            Player.selectedPlayerSkinTexture = Customize.Instance.playerSkins[playerIndex];
        else
            Player.selectedPlayerSkinTexture = null;

        // 2. Hook & Rope 스킨 
        if (hookIndex >= 0 && hookIndex < Customize.Instance.hookSkins.Count)
        {
            Hook.currentSkin = Customize.Instance.hookSkins[hookIndex];
            // ropeIndex가 유효한지 체크 후 적용
            if (ropeIndex >= 0 && ropeIndex < Customize.Instance.ropeSkins.Count)
            {
                Player.currentRopeMaterial = Customize.Instance.ropeSkins[ropeIndex];
            }
        }
        else
        {
            Hook.currentSkin = null;
            Player.currentRopeMaterial = null;
        }

        // 3. Helicopter 스킨 
        if (heliIndex >= 0 && heliIndex < Customize.Instance.helicopterSkins.Count)
            Helicopter.currentSkin = Customize.Instance.helicopterSkins[heliIndex];
        else
            Helicopter.currentSkin = null;

        // 4. 비니 & 가방 스킨
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

    public void UpdateMainMenuCharacterSkin()
    {
        if (skinData == null)
        {
            Debug.LogError("SkinDatabase가 연결되지 않았습니다!");
            return;
        }

        int playerIndex = PlayerPrefs.GetInt("SelectedPlayerSkinIndex", 0);

        if (playerIndex >= 0 && playerIndex < skinData.playerSkins.Count)
        {
            bodyImage.sprite = skinData.playerSkins[playerIndex];
        }
        else
        {
            bodyImage.sprite = skinData.playerSkins[0]; 
        }

        bool isBeanieEquipped = PlayerPrefs.GetInt("SelectedBeanieEquipped", 0) == 1;
        int beanieIndex = PlayerPrefs.GetInt("SelectedBeanieSkinIndex", -1);

        if (beanieImage != null)
        {
            if (isBeanieEquipped && beanieIndex >= 0 && beanieIndex < skinData.beanieSkins.Count)
            {
                beanieImage.gameObject.SetActive(true);
                beanieImage.sprite = skinData.beanieSkins[beanieIndex];
            }
            else
            {
                beanieImage.gameObject.SetActive(false);
            }
        }

        bool isBagEquipped = PlayerPrefs.GetInt("SelectedBagEquipped", 0) == 1;
        int bagIndex = PlayerPrefs.GetInt("SelectedBagSkinIndex", -1);

        if (bagImage != null)
        {
            if (isBagEquipped && bagIndex >= 0 && bagIndex < skinData.bagSkins.Count)
            {
                bagImage.gameObject.SetActive(true);
                bagImage.sprite = skinData.bagSkins[bagIndex];
            }
            else
            {
                bagImage.gameObject.SetActive(false);
            }
        }

        int hookIndex = PlayerPrefs.GetInt("SelectedHookSkinIndex", 0);
        if (hookImage != null)
        {
            // 데이터베이스에 리스트가 있고, 인덱스가 유효한지 확인
            if (skinData.hookSkins != null && hookIndex >= 0 && hookIndex < skinData.hookSkins.Count)
            {
                hookImage.sprite = skinData.hookSkins[hookIndex];
            }
            else if (skinData.hookSkins != null && skinData.hookSkins.Count > 0)
            {
                // 예외 상황(인덱스 오류 등) 시 기본값(0번) 적용
                hookImage.sprite = skinData.hookSkins[0];
            }
        }

        int heliIndex = PlayerPrefs.GetInt("SelectedHelicopterSkinIndex", 0);
        if (helicopterImage != null)
        {
            // 데이터베이스에 리스트가 있고, 인덱스가 유효한지 확인
            if (skinData.helicopterSkins != null && heliIndex >= 0 && heliIndex < skinData.helicopterSkins.Count)
            {
                helicopterImage.sprite = skinData.helicopterSkins[heliIndex];
            }
            else if (skinData.helicopterSkins != null && skinData.helicopterSkins.Count > 0)
            {
                // 예외 상황 시 기본값(0번) 적용
                helicopterImage.sprite = skinData.helicopterSkins[0];
            }
        }

    }
}