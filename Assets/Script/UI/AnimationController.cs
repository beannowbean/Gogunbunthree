using UnityEngine;

/// <summary>
/// 애니메이션 이벤트를 중계하는 클래스
/// </summary>
public class AnimationController : MonoBehaviour
{
    // 실제 로직이 있는 메인 컨트롤러 연결
    public MainMenuController mainController;

    // 애니메이션 이벤트에서 이 함수 호출
    public void CallSetUIInteractable(int state)
    {
        if (mainController != null)
        {
            mainController.SetUIInteractable(state);
        }
    }

    // 닉네임 체크 애니메이션 이벤트에서 이 함수 호출
    public void CallCheckFirstLoginNickname()
    {
        if (mainController != null)
        {
            mainController.CheckFirstLoginNickname();
        }
    }
}