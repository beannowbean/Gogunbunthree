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

        // 아이콘 설정: 보상 종류와 인덱스에 따라 Customize의 아이콘 사용
        if (iconImage != null)
        {
            Sprite iconSprite = null;
            // 보상 타입에 따라 아이콘 리스트 선택
            switch (data.rewardType)
            {
                case RewardType.PlayerSkin:
                    if (Customize.Instance != null && data.rewardIndex >= 0 && data.rewardIndex < Customize.Instance.playerSkinIcons.Count)
                        iconSprite = Customize.Instance.playerSkinIcons[data.rewardIndex];
                    break;
                case RewardType.BagSkin:
                    if (Customize.Instance != null && data.rewardIndex >= 0 && data.rewardIndex < Customize.Instance.bagSkinIcons.Count)
                        iconSprite = Customize.Instance.bagSkinIcons[data.rewardIndex];
                    break;
                case RewardType.BeanieSkin:
                    if (Customize.Instance != null && data.rewardIndex >= 0 && data.rewardIndex < Customize.Instance.beanieSkinIcons.Count)
                        iconSprite = Customize.Instance.beanieSkinIcons[data.rewardIndex];
                    break;
                case RewardType.HookSkin:
                    if (Customize.Instance != null && data.rewardIndex >= 0 && data.rewardIndex < Customize.Instance.hookSkinIcons.Count)
                        iconSprite = Customize.Instance.hookSkinIcons[data.rewardIndex];
                    break;
                case RewardType.HelicopterSkin:
                    if (Customize.Instance != null && data.rewardIndex >= 0 && data.rewardIndex < Customize.Instance.helicopterSkinIcons.Count)
                        iconSprite = Customize.Instance.helicopterSkinIcons[data.rewardIndex];
                    break;
                // 필요시 추가 스킨 타입 처리
            }
            
            iconImage.sprite = iconSprite;
        }

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
        bool isRewardClaimed = AchievementManager.Instance != null && 
                              AchievementManager.Instance.IsRewardClaimed(data.id);


        // 보상 수령 버튼 (완료 + 미수령 + 보상 있음)
        bool canClaim = data.IsCompleted && !isRewardClaimed;
        if (claimRewardButton != null)
        {
            claimRewardButton.gameObject.SetActive(canClaim);
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
        Debug.Log($"[AchievementItem] Claim button clicked for '{achievementId}'");

        if (AchievementManager.Instance == null)
        {
            Debug.LogWarning("[AchievementItem] AchievementManager.Instance is null. Cannot claim reward.");
            return;
        }

        if (currentData == null)
        {
            Debug.LogWarning("[AchievementItem] currentData is null. Cannot claim reward.");
            return;
        }

        // 상태 로그(완료 여부, 보상 수령 여부, 보상 타입)
        bool isUnlocked = AchievementManager.Instance.IsAchievementUnlocked(currentData.id);
        bool isRewardClaimed = AchievementManager.Instance.IsRewardClaimed(currentData.id);
        Debug.Log($"[AchievementItem] State before claim - IsCompleted: {currentData.IsCompleted}, IsUnlocked(PlayerPrefs): {isUnlocked}, IsRewardClaimed: {isRewardClaimed}, RewardType: {currentData.rewardType}, RewardIndex: {currentData.rewardIndex}");

        bool success = AchievementManager.Instance.ClaimReward(currentData.id);
        Debug.Log($"[AchievementItem] ClaimReward returned: {success} for '{currentData.id}'");

        if (success)
        {
            // UI 업데이트
            SetupRewardUI(currentData);
            // 사운드 재생
            if (SFXManager.Instance != null)
            {
                SFXManager.Instance.Play("Button");
            }
        }
        else
        {
            // 실패 시 원인 추가 검사 로그
            isRewardClaimed = AchievementManager.Instance.IsRewardClaimed(currentData.id);
            isUnlocked = AchievementManager.Instance.IsAchievementUnlocked(currentData.id);
            Debug.LogWarning($"[AchievementItem] Claim failed - IsCompleted: {currentData.IsCompleted}, IsUnlocked(PlayerPrefs): {isUnlocked}, IsRewardClaimed: {isRewardClaimed}, RewardType: {currentData.rewardType}, RewardIndex: {currentData.rewardIndex}");
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
