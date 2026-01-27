using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(CanvasScaler))]
public class MainmenuScreen : MonoBehaviour
{
    private CanvasScaler canvasScaler;

    // 세로형 게임 기준 해상도 (보통 1080 x 1920 사용)
    private const float REFERENCE_WIDTH = 1080f;
    private const float REFERENCE_HEIGHT = 1920f;

    void Awake()
    {
        canvasScaler = GetComponent<CanvasScaler>();
        UpdateMatchRate();
    }

    void Update()
    {
        // 에디터에서 테스트할 때 실시간 반영 (빌드 시 제거해도 무방)
        if (Application.isEditor)
        {
            UpdateMatchRate();
        }
    }

    private void UpdateMatchRate()
    {
        float currentAspectRatio = (float)Screen.width / Screen.height;
        float targetAspectRatio = REFERENCE_WIDTH / REFERENCE_HEIGHT;

        // 세로 게임 로직:
        // 1. 길쭉한 폰 (Z Flip, 최신 갤럭시 등) -> 비율이 낮음 (예: 9/21 = 0.42)
        //    -> 가로 폭(Width)에 맞추지 않으면 좌우가 잘릴 수 있음 -> Match Width (0)
        // 2. 뚱뚱한 폰 (아이패드, Z Fold 펼침) -> 비율이 높음 (예: 3/4 = 0.75)
        //    -> 세로 높이(Height)에 맞추지 않으면 위아래가 잘릴 수 있음 -> Match Height (1)

        if (currentAspectRatio < targetAspectRatio)
        {
            // 화면이 기준보다 '홀쭉함' (가로가 좁음) -> 가로(Width) 고정
            canvasScaler.matchWidthOrHeight = 0f;
        }
        else
        {
            // 화면이 기준보다 '뚱뚱함' (가로가 넓음) -> 세로(Height) 고정
            canvasScaler.matchWidthOrHeight = 1f;
        }
    }
}