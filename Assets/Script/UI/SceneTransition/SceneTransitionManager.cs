using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SceneTransitionManager : MonoBehaviour
{
    public static SceneTransitionManager Instance;

    [Header("UI Reference")]
    public CanvasGroup fadeCanvasGroup; // 투명도 조절을 위한 CanvasGroup
    public float fadeDuration = 1.0f;   // 페이드 되는 시간 (초)

    private void Awake()
    {
        // 싱글톤 설정 (씬이 넘어가도 파괴되지 않게 함)
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    private void Start()
    {
        // 게임 시작 시(메인 메뉴) 화면이 밝아지도록
        StartCoroutine(Fade(0f));
    }

    // 외부에서 이 함수를 호출해서 씬을 이동합니다.
    // 예: SceneTransitionManager.Instance.LoadScene("InGame");
    public void LoadScene(string sceneName)
    {
        StartCoroutine(LoadSceneRoutine(sceneName));
    }

    IEnumerator LoadSceneRoutine(string sceneName)
    {
        // 1. 페이드 아웃 (화면이 점점 어두워짐, Alpha 0 -> 1)
        yield return StartCoroutine(Fade(1f));

        // 2. 비동기 씬 로딩 시작
        // 로딩이 진행되는 동안 게임이 멈추지 않음
        AsyncOperation operation = SceneManager.LoadSceneAsync(sceneName);

        // 로딩이 완료되어도 바로 씬을 넘기지 않고 잠시 대기 (원한다면)
        operation.allowSceneActivation = false;

        // 로딩이 90% 이상 될 때까지 대기
        while (!operation.isDone)
        {
            // progress가 0.9에 도달하면 로딩 완료로 간주
            if (operation.progress >= 0.9f)
            {
                // 로딩 완료 후 씬 전환 허용
                operation.allowSceneActivation = true;
            }
            yield return null;
        }

        // 3. 씬 전환 후 페이드 인 (화면이 점점 밝아짐, Alpha 1 -> 0)
        // 새 씬이 로드된 후 한 프레임 대기 (안정성)
        yield return new WaitForSeconds(0.5f);
        yield return StartCoroutine(Fade(0f));
    }

    // 투명도(Alpha)를 조절하는 코루틴
    IEnumerator Fade(float targetAlpha)
    {
        fadeCanvasGroup.blocksRaycasts = true; // 페이드 중 터치 방지

        float startAlpha = fadeCanvasGroup.alpha;
        float time = 0f;

        while (time < fadeDuration)
        {
            time += Time.unscaledDeltaTime; // TimeScale이 0이어도 작동하도록
            fadeCanvasGroup.alpha = Mathf.Lerp(startAlpha, targetAlpha, time / fadeDuration);
            yield return null;
        }

        fadeCanvasGroup.alpha = targetAlpha;
        fadeCanvasGroup.blocksRaycasts = (targetAlpha == 1f); // 다 어두워졌을 때만 터치 방지 유지, 밝아지면 터치 가능
    }
}