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
            // 업적 달성 시에만 description 표시
            if(AchievementManager.Instance.IsAchievementUnlocked(data.id))
                descriptionText.text = data.description;
            else
                descriptionText.text = "??????";


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
        
        // 아이콘 클릭으로도 보상 수령 가능하게 버튼 리스너 연결 (완료된 경우에만)
        if (iconImage != null)
        {
            var iconBtn = iconImage.GetComponent<Button>();
            if (iconBtn != null)
            {
                iconBtn.onClick.RemoveAllListeners();
                if (canClaim)
                {
                    iconBtn.onClick.AddListener(OnClaimRewardClicked);
                }
            }
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

        bool success = AchievementManager.Instance.ClaimReward(currentData.id);

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
    }
    
    private void UpdateProgress(AchievementData data)
    {
        if (progressSlider != null)
        {
            progressSlider.maxValue = 100f; // 퍼센트 값이므로 최대값 100
            
            // RankManager에서 업적 달성률 가져오기
            if (RankManager.Instance != null && RankManager.Instance.IsLoggedIn)
            {
                RankManager.Instance.GetAchievementRate(data.id, (rate) =>
                {
                    progressSlider.value = rate;
                    
                    // progress text도 업데이트
                    if (progressText != null)
                    {
                        progressText.enabled = true;
                        progressText.gameObject.SetActive(true);
                        
                        // 업적 달성 시 등수도 함께 표시
                        if (data.IsCompleted)
                        {
                            RankManager.Instance.GetAchievementOrder(data.id, (order) =>
                            {
                                progressText.text = $"Top {rate:F1}% (#{order})";
                            });
                        }
                        else
                        {
                            progressText.text = $"{rate:F0}%";
                        }
                    }
                });
            }
        }
        
        if (progressText != null)
        {
            progressText.enabled = true;
            progressText.gameObject.SetActive(true);
            
            float percentage = (data.currentValue / (float)data.targetValue) * 100f;
            progressText.text = $"{percentage:F0}%";
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
