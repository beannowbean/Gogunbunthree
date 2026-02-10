using UnityEngine;
using TMPro;
using System.Collections;

/// <summary>
/// 닉네임 변경 UI 컨트롤러
/// </summary>
public class NicknameUIController : MonoBehaviour
{
    [Header("UI References")]
    public GameObject nicknamePanel;      // NickNamePanel 오브젝트
    public TMP_InputField nicknameInput; // 입력창 (InputField)
    public TextMeshProUGUI statusText;   // 에러 메시지 등을 표시할 텍스트

    // 내부 변수
    Coroutine fadeCoroutine;    // 실행 중인 페이드 코루틴 저장용

    // 닉네임 변경 패널 열기
    public void OpenNicknamePanel()
    {
        nicknamePanel.SetActive(true);
        
        // 서버에서 현재 닉네임을 받아와 입력창에 미리 채워넣기
        nicknameInput.text = RankManager.Instance.currentNickname;
        
        if (statusText != null) statusText.text = "";
    }

    // Apply 버튼을 눌렀을 때 호출
    public void OnClickApply()
    {
        string newName = nicknameInput.text;

        // 글자수 제한 체크 (2자 이상 / 15자 이하)
        if (newName.Length < 2)
        {
            ShowStatusText("Nickname is too short!\n(Min 2 characters)");
            return;
        }
        if (newName.Length > 15)
        {
            ShowStatusText("Nickname is too long!\n(Max 15 characters)");
            return;
        }

        RankManager.Instance.ChangeNickname(newName, (success, error) =>
        {
            if (success)
            {
                // 성공 시 패널 닫기
                nicknamePanel.SetActive(false);
            }
            else
            {
                // 실패 시 에러 메시지 표시
                ShowStatusText(error);
            }
        });
    }

    // Cancel 버튼을 눌렀을 때 호출
    public void OnClickCancel()
    {
        nicknamePanel.SetActive(false);
    }

    // 메시지 띄우고 페이드 아웃 시작 함수
    private void ShowStatusText(string message)
    {
        if (statusText == null) return;

        // 이미 돌고 있는 코루틴이 있다면 정지 (중첩 방지)
        if (fadeCoroutine != null) StopCoroutine(fadeCoroutine);

        statusText.text = message;
        fadeCoroutine = StartCoroutine(FadeOutStatusText());
    }

    // 메시지 페이드 아웃 코루틴
    IEnumerator FadeOutStatusText()
    {
        // 텍스트를 불투명하게 설정
        Color originalColor = statusText.color;
        originalColor.a = 1f;
        statusText.color = originalColor;

        yield return new WaitForSeconds(2f);

        // 1초 동안 부드럽게 투명해짐
        float duration = 1f; 
        float currentTime = 0f;

        while (currentTime < duration)
        {
            currentTime += Time.deltaTime;
            float alpha = Mathf.Lerp(1f, 0f, currentTime / duration);
            
            originalColor.a = alpha;
            statusText.color = originalColor;
            
            yield return null;
        }

        statusText.text = ""; // 완전히 사라지면 텍스트 비우기
    }
}