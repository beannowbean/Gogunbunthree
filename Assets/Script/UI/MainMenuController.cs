using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenuController : MonoBehaviour
{
    [Header("UI Containers")]
    public RectTransform uiContainer; // 버튼들 컨테이너

    [Header("Title Image Animation")]
    public RectTransform titleImageRect;
    public float titleSlideDuration = 0.7f;
    private Vector2 titleFinalPos;

    // [추가] 헬리콥터 관련 변수
    [Header("Helicopter Animation")]
    public RectTransform helicopterRect; // 헬리콥터 이미지
    public AudioSource heliAudioSource; // 헬리콥터 오디오 소스

    // 좌표 및 크기 설정
    private Vector2 heliStartPos = new Vector2(-100, 6000);
    private Vector2 heliSizeLarge = new Vector2(8000, 8000);

    private Vector2 heliLandPos = new Vector2(-100, 4000); // 내려올 위치

    private Vector2 heliFinalPos = new Vector2(400, 1000); // 날아갈 위치
    private Vector2 heliSizeSmall = new Vector2(100, 100); // 작아질 크기

    // [추가] 캐릭터 관련 변수
    [Header("Character Animation")]
    public RectTransform characterRect; // 캐릭터 이미지

    // [추가] 캐릭터의 애니메이터를 제어하기 위한 변수
    public Animator characterAnimator;

    // 캐릭터 초기값 (100, 0 / 2000, 2000)
    private Vector2 charStartPos = new Vector2(-100, 0);
    private Vector2 charSizeLarge = new Vector2(2000, 2000);

    // 캐릭터 최종값 (390, 955 / 25, 25)
    private Vector2 charFinalPos = new Vector2(390, 955);
    private Vector2 charSizeSmall = new Vector2(25, 25);

    void Start()
    {
        // 1. 타이틀 이미지 위로 숨기기 (기존 코드)
        if (titleImageRect != null)
        {
            titleFinalPos = titleImageRect.anchoredPosition;
            float offScreenY = Screen.height / 2f + titleImageRect.rect.height;
            titleImageRect.anchoredPosition = new Vector2(titleFinalPos.x, offScreenY);
        }

        // [추가] 2. 헬리콥터 초기 위치 및 크기 설정 (거대하게 위쪽에 대기)
        if (helicopterRect != null)
        {
            helicopterRect.anchoredPosition = heliStartPos;
            helicopterRect.sizeDelta = heliSizeLarge;
        }

        // ... (BGM 재생 등 기타 Start 로직)
    }

    public void StartGame()
    {
        if (isLoadingScene) return;
        isLoadingScene = true; // 중복 클릭 방지

        UIController.tutorialSkip = true;
        UIController.isRestarting = false;

        if (SFXManager.Instance != null) SFXManager.Instance.Play("Button");
        if (PlayerAchivementList.Instance != null) PlayerAchivementList.Instance.Newbie();

        // --- 애니메이션 시작 ---

        // 1. 타이틀 이미지 내려오기
        if (titleImageRect != null) StartCoroutine(SlideInTitle());

        // 2. 버튼들 오른쪽으로 사라지기
        if (uiContainer != null) StartCoroutine(SlideOutUI());

        // [중요] 3. 헬리콥터 연출 시작! (씬 로딩은 이 안에서 처리함)
        if (helicopterRect != null)
        {
            StartCoroutine(HelicopterSequence());
        }
        else
        {
            // 헬리콥터가 없으면 그냥 바로 로딩 (안전장치)
            LoadInGameScene();
        }
    }

    // 헬리콥터 전체 연출 코루틴
    IEnumerator HelicopterSequence()
    {
        characterAnimator.enabled = false;

        // 1단계: 1초 대기 (Start 버튼 누르고 1초 뒤 시작)
        yield return new WaitForSeconds(1.0f);

        BGMManager.Instance.FadeTo(0f, 2.0f);

        heliAudioSource.volume = 0f;
        heliAudioSource.Play();      
        StartCoroutine(FadeAudioSource(heliAudioSource, 1.0f, 2.0f));

        // 2단계: -100, 390 좌표까지 1초동안 내려옴
        float timer = 0f;
        float duration = 1.0f;
        Vector2 startPos = helicopterRect.anchoredPosition;

        while (timer < duration)
        {
            timer += Time.unscaledDeltaTime;
            float t = timer / duration;
            // EaseOut (천천히 도착)
            t = Mathf.Sin(t * Mathf.PI * 0.5f);

            helicopterRect.anchoredPosition = Vector2.Lerp(startPos, heliLandPos, t);
            yield return null;
        }
        helicopterRect.anchoredPosition = heliLandPos;

        // 헬기와 캐릭터 모두 50만큼 아래로
        Vector2 heliBounceDest = heliLandPos + new Vector2(0, -50);
        Vector2 charBounceDest = charStartPos + new Vector2(0, -50);

        // [내려가기]
        // 캐릭터도 같이 코루틴 시작 (StartCoroutine만 하면 동시에 실행됨)
        if (characterRect != null) StartCoroutine(MoveTo(characterRect, charBounceDest, 0.2f));
        yield return StartCoroutine(MoveTo(helicopterRect, heliBounceDest, 0.2f)); // 헬기가 끝날 때까지 대기

        // [올라오기]
        if (characterRect != null) StartCoroutine(MoveTo(characterRect, charStartPos, 0.2f));
        yield return StartCoroutine(MoveTo(helicopterRect, heliLandPos, 0.2f));

        LoadInGameScene();

        StartCoroutine(FadeAudioSource(heliAudioSource, 0f, 1.0f));

        // 4단계: 400, 1000 좌표로 이동하면서 + 100, 100 크기로 작아짐 (1초 동안)
        timer = 0f;
        duration = 1.0f; // 이동 시간
        Vector2 currentPos = helicopterRect.anchoredPosition;
        Vector2 currentSize = helicopterRect.sizeDelta;

        // 캐릭터 현재 상태 (혹시 모르니 현재 위치 받아옴)
        Vector2 charCurPos = characterRect != null ? characterRect.anchoredPosition : charStartPos;
        Vector2 charCurSize = characterRect != null ? characterRect.sizeDelta : charSizeLarge;

        while (timer < duration)
        {
            timer += Time.unscaledDeltaTime;
            float t = timer / duration;
            // 가속도 붙어서 슝 날아가게 (EaseIn)
            float easeT = t * t;

            // 위치 이동
            helicopterRect.anchoredPosition = Vector2.Lerp(currentPos, heliFinalPos, easeT);
            // 크기 변경 (8000 -> 100)
            helicopterRect.sizeDelta = Vector2.Lerp(currentSize, heliSizeSmall, easeT);

            characterRect.anchoredPosition = Vector2.Lerp(charCurPos, charFinalPos, easeT);
            characterRect.sizeDelta = Vector2.Lerp(charCurSize, charSizeSmall, easeT);

            yield return null;
        }
        helicopterRect.anchoredPosition = heliFinalPos;
        helicopterRect.sizeDelta = heliSizeSmall;

        characterRect.anchoredPosition = charFinalPos;
        characterRect.sizeDelta = charSizeSmall;
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

    // 위치 이동을 도와주는 헬퍼 함수
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

    // 씬 로딩 페이드 아웃 시작
    IEnumerator TriggerSceneLoadFade()
    {
        // 헬기가 날아가기 시작하고 0.5초 뒤에 화면 하얗게 만들기 시작
        yield return new WaitForSeconds(0.5f);
        LoadInGameScene();
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

    // (기존 코드 유지)
    IEnumerator SlideInTitle()
    {
        // ... (아까 작성해주신 내용 그대로)
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

    // (기존 코드 유지)
    IEnumerator SlideOutUI()
    {
        // ... (아까 작성해주신 내용 그대로)
        float timer = 0f;
        Vector2 startPos = uiContainer.anchoredPosition;
        Vector2 targetPos = new Vector2(startPos.x + Screen.width * 1.5f, startPos.y);
        while (timer < 0.5f)
        {
            timer += Time.unscaledDeltaTime;
            float t = timer / 0.5f;
            float easeT = t * t;
            uiContainer.anchoredPosition = Vector2.Lerp(startPos, targetPos, easeT);
            yield return null;
        }
        uiContainer.anchoredPosition = targetPos;
    }

        // Hook 스킨 설정
        if (hookIndex >= 0 && hookIndex < Customize.Instance.hookSkins.Count)
        {
            Hook.currentSkin = Customize.Instance.hookSkins[hookIndex];
            Player.currentRopeMaterial = Customize.Instance.ropeSkins[ropeIndex];
                
        }
        else
        {
            Hook.currentSkin = null;
            Player.currentRopeMaterial = null;
        }

        // Helicopter 스킨 설정
        if (heliIndex >= 0 && heliIndex < Customize.Instance.helicopterSkins.Count)
            Helicopter.currentSkin = Customize.Instance.helicopterSkins[heliIndex];
        else
            Helicopter.currentSkin = null;

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
    // 변수 선언 등 기타 필요한 부분들...
    private bool isLoadingScene = false;
}
