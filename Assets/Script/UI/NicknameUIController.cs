using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Text.RegularExpressions;
using System.Linq;

/// <summary>
/// 닉네임 변경 UI 컨트롤러
/// </summary>
public class NicknameUIController : MonoBehaviour
{
    [Header("UI References")]
    public GameObject nicknamePanel;      // NickNamePanel 오브젝트
    public TMP_InputField nicknameInput; // 입력창 (InputField)
    public TextMeshProUGUI conditionLength;   // 길이 조건
    public TextMeshProUGUI errorMessage;   // 중복 조건
    public Button applyButton;            // Apply 버튼
    public CanvasGroup applyButtonCanvasGroup;  // Apply 버튼의 CanvasGroup (투명도 조절용)

    [Header("Condition Settings")]
    private Color validColor;
    private Color invalidColor;

    private void Awake()
    {
        // HEX 컬러 파싱
        ColorUtility.TryParseHtmlString("#00968B", out validColor);
        ColorUtility.TryParseHtmlString("#C80000", out invalidColor);
    }

    private void Start()
    {
        // Input 필드에 리스너 추가
        if (nicknameInput != null)
        {
            nicknameInput.onValueChanged.AddListener(OnNicknameInputChanged);
        }
    }

    // 닉네임 변경 패널 열기
    public void OpenNicknamePanel()
    {
        nicknamePanel.SetActive(true);
        
        // 서버에서 현재 닉네임을 받아와 입력창에 미리 채워넣기
        nicknameInput.text = RankManager.Instance.currentNickname;
        
        // 에러 메시지 초기화
        if (errorMessage != null)
            errorMessage.text = "";
        
        // 조건 체크
        ValidateNickname(nicknameInput.text, true);
    }
    
    // Input 필드 값이 변경될 때마다 호출
    private void OnNicknameInputChanged(string value)
    {
        ValidateNickname(value, false);
    }
    
    // 닉네임 조건 검증 함수
    private void ValidateNickname(string nickname, bool isInitial)
    {
        // 길이 조건 체크 (2~15자)
        bool isLengthValid = nickname.Length >= 2 && nickname.Length <= 15;
        UpdateConditionText(conditionLength, "LENGTH between 2 and 15", isLengthValid);
        
        // Apply 버튼 상태 업데이트
        bool canApply = isLengthValid;
        UpdateApplyButtonState(canApply);
    }
    
    // 적절한 단어인지 확인 (형식만 체크, 금지어는 서버에서 검증)
    private bool CheckAppropriateWord(string nickname)
    {
        if (string.IsNullOrEmpty(nickname)) return false;
        
        // 특수문자 체크 (영문, 숫자, 공백, 일부 특수문자만 허용)
        if (!Regex.IsMatch(nickname, @"^[a-zA-Z0-9\s_\-]+$"))
            return false;
        
        return true;
    }
    
    // 조건 텍스트 업데이트
    private void UpdateConditionText(TextMeshProUGUI textComponent, string baseText, bool isValid)
    {
        if (textComponent == null) return;
        
        if (isValid)
        {
            textComponent.text = baseText + " (O)";
            textComponent.color = validColor;
        }
        else
        {
            textComponent.text = baseText + " (X)";
            textComponent.color = invalidColor;
        }
    }
    
    // Apply 버튼 상태 업데이트 (활성화/비활성화 및 투명도 조절)
    private void UpdateApplyButtonState(bool isValid)
    {
        if (applyButton != null)
        {
            applyButton.interactable = isValid;
        }
        
        if (applyButtonCanvasGroup != null)
        {
            // 조건 만족: 불투명, 조건 불만족: 반투명
            applyButtonCanvasGroup.alpha = isValid ? 1f : 0.75f;
        }
    }

    // Apply 버튼을 눌렀을 때 호출
    public void OnClickApply()
    {
        string newName = nicknameInput.text;
        
        // 기본 조건 체크 (길이, 적절한 단어)
        bool isLengthValid = newName.Length >= 2 && newName.Length <= 15;
        bool isAppropriateWord = CheckAppropriateWord(newName);
        
        // 기본 조건 불만족 시 서버 요청하지 않음
        if (!isLengthValid || !isAppropriateWord)
        {
            return;
        }

        // 서버에 닉네임 변경 요청 (중복 체크)
        RankManager.Instance.ChangeNickname(newName, (success, error) =>
        {
            if (success)
            {                
                // 에러 메시지 초기화
                if (errorMessage != null)
                    errorMessage.text = "";
                
                // 패널 닫기
                // 최초 닉네임 변경 시 플래그 저장
                PlayerPrefs.SetInt("HasSetNickname", 1);
                PlayerPrefs.Save();
                // 성공 시 패널 닫기
                nicknamePanel.SetActive(false);
            }
            else
            {
                // 에러 메시지 표시
                if (errorMessage != null)
                {
                    errorMessage.text = error;
                    errorMessage.color = invalidColor;
                }
            }
        });
    }

    // Cancel 버튼을 눌렀을 때 호출
    public void OnClickCancel()
    {
        // 최초 설정 여부 확인
        if (RankManager.Instance != null && !RankManager.Instance.HasSetNickname())
        {
            // 최초 설정 전이라면 창을 닫지 않고 경고 메시지 표시
            ShowStatusText("You must set a nickname to start!");
            return;
        }
        nicknamePanel.SetActive(false);
    }
}