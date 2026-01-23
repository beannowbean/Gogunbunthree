using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class AchievementItem : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI titleText;
    [SerializeField] private TextMeshProUGUI descriptionText;
    [SerializeField] private Image iconImage;
    [SerializeField] private Image lockOverlay;
    
    [Header("Progress UI")]
    [SerializeField] private Slider progressSlider;
    [SerializeField] private TextMeshProUGUI progressText;
    [SerializeField] private GameObject progressContainer;
    
    [Header("Reward UI")]
    [SerializeField] private Button claimRewardButton;
    [SerializeField] private GameObject rewardClaimedIndicator;

    private string achievementId;
    private AchievementData currentData;

    public void Initialize(AchievementData data, bool isUnlocked, System.Action<string, bool> callback)
    {
        achievementId = data.id;
        currentData = data;

        // 배경 이미지 활성화
        Image backgroundImage = GetComponent<Image>();
        if (backgroundImage != null)
        {
            backgroundImage.enabled = true;
        }

        if (titleText != null)
        {
            titleText.text = data.title;
            titleText.enabled = true;
            titleText.gameObject.SetActive(true);
        }

        if (descriptionText != null)
        {
            descriptionText.text = data.description;
            descriptionText.enabled = true;
            descriptionText.gameObject.SetActive(true);
        }

        if (iconImage != null && data.icon != null)
            iconImage.sprite = data.icon;

        // 완료 상태 표시 - 진척도가 완료되면 잠금 표시
        if (lockOverlay != null)
            lockOverlay.gameObject.SetActive(data.IsCompleted);
        
        // 진척도 UI 업데이트
        UpdateProgress(data);
        
        // 보상 UI 설정
        SetupRewardUI(data);
    }
    
    /// <summary>
    /// 보상 UI 설정
    /// </summary>
    private void SetupRewardUI(AchievementData data)
    {
        // 버튼 상태 상세 로그
        if (claimRewardButton != null)
        {
            var btnImage = claimRewardButton.GetComponent<UnityEngine.UI.Image>();
            Debug.Log($"[업적UI][ButtonState] Button.enabled={claimRewardButton.enabled}, interactable={claimRewardButton.interactable}, Image.enabled={btnImage?.enabled}, Image.raycastTarget={btnImage?.raycastTarget}");
            var cg = claimRewardButton.GetComponentInParent<CanvasGroup>();
            if (cg != null)
                Debug.Log($"[업적UI][ButtonState] CanvasGroup.interactable={cg.interactable}, blocksRaycasts={cg.blocksRaycasts}, alpha={cg.alpha}");
        }
        bool isRewardClaimed = AchievementManager.Instance != null && 
                              AchievementManager.Instance.IsRewardClaimed(data.id);

        // 로그 추가: 조건별 값 출력
        Debug.Log($"[업적UI] id={data.id}, IsCompleted={data.IsCompleted}, isRewardClaimed={isRewardClaimed}, rewardType={data.rewardType}");

        // 보상 수령 버튼 (완료 + 미수령 + 보상 있음)
        bool canClaim = data.IsCompleted && !isRewardClaimed && data.rewardType != RewardType.None;
        Debug.Log($"[업적UI] canClaim={canClaim}");
        if (claimRewardButton != null)
        {
            claimRewardButton.gameObject.SetActive(canClaim);
            // 임시 우회: 버튼 활성화 시 Button.enabled, Image.enabled를 true로 강제 설정
            var btnImage = claimRewardButton.GetComponent<UnityEngine.UI.Image>();
            if (canClaim)
            {
                claimRewardButton.enabled = true;
                if (btnImage != null) btnImage.enabled = true;
            }
            // 기존 리스너 제거 후 새로 추가
            claimRewardButton.onClick.RemoveAllListeners();
            claimRewardButton.onClick.AddListener(OnClaimRewardClicked);
        }

        // 보상 수령 완료 표시
        if (rewardClaimedIndicator != null)
        {
            rewardClaimedIndicator.SetActive(isRewardClaimed);
        }
    }
    
    /// <summary>
    /// 보상 수령 버튼 클릭
    /// </summary>
    private void OnClaimRewardClicked()
    {
        Debug.Log($"[업적UI] Claim 버튼 클릭: id={currentData?.id}");
        if (AchievementManager.Instance != null && currentData != null)
        {
            bool success = AchievementManager.Instance.ClaimReward(currentData.id);
            Debug.Log($"[업적UI] ClaimReward 결과: success={success}");
            if (success)
            {
                // UI 업데이트
                SetupRewardUI(currentData);
                Debug.Log($"[업적UI] SetupRewardUI 호출 완료");
                // 사운드 재생
                if (SFXManager.Instance != null)
                {
                    SFXManager.Instance.Play("Button");
                }
            }
        }
    }
    
    private void UpdateProgress(AchievementData data)
    {
        // 완료되면 진척도 UI 숨김
        bool showProgress = data.targetValue > 0 && !data.IsCompleted;
        
        if (progressContainer != null)
        {
            progressContainer.SetActive(showProgress);
        }

        
        
        if (showProgress)
        {
            if (progressSlider != null)
            {
                progressSlider.maxValue = data.targetValue;
                progressSlider.value = data.currentValue;
            }
            
            if (progressText != null)
            {
                progressText.enabled = true;
                progressText.gameObject.SetActive(true);
                
                float percentage = (data.currentValue / (float)data.targetValue) * 100f;
                progressText.text = $"{percentage:F0}%";
            }
        }
    }
    
    // 업적 UI 업데이트 메서드
    public void UpdateDisplay(AchievementData data)
    {
        currentData = data;
        
        // 완료 상태 표시 - 진척도가 완료되면 잠금 표시
        if (lockOverlay != null)
            lockOverlay.gameObject.SetActive(data.IsCompleted);
        
        // 진척도 UI 업데이트
        UpdateProgress(data);
        
        // 보상 UI 업데이트
        SetupRewardUI(data);
    }
}
