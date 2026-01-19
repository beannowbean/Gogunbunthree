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

    private string achievementId;

    public void Initialize(AchievementData data, bool isUnlocked, System.Action<string, bool> callback)
    {
        achievementId = data.id;

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
    }
    
    private void UpdateProgress(AchievementData data)
    {
        // targetValue가 0이면 진척도 UI 숨김, 완료되어도 표시
        bool showProgress = data.targetValue > 0;
        
        if (progressContainer != null)
        {
            progressContainer.SetActive(showProgress);
        }
        
        if (progressSlider != null && data.targetValue > 0)
        {
            progressSlider.maxValue = data.targetValue;
            progressSlider.value = data.currentValue;
        }
        
        if (progressText != null && data.targetValue > 0)
        {
            progressText.enabled = true;
            progressText.gameObject.SetActive(true);
            
            if (data.IsCompleted)
            {
                progressText.text = "COMPLETED";
            }
            else
            {
                float percentage = (data.currentValue / (float)data.targetValue) * 100f;
                progressText.text = $"{percentage:F0}%";
            }
            
        }
    }
    
    // 업적 UI 업데이트 메서드
    public void UpdateDisplay(AchievementData data)
    {
        // 완료 상태 표시 - 진척도가 완료되면 잠금 표시
        if (lockOverlay != null)
            lockOverlay.gameObject.SetActive(data.IsCompleted);
        
        // 진척도 UI 업데이트
        UpdateProgress(data);
    }
    
    // 업적 진척도 업데이트 메서드
    public static void UpdateAchievementProgress(AchievementData achievement, int progressValue)
    {
        if (achievement != null)
        {
            achievement.currentValue = progressValue;
            PlayerPrefs.SetInt($"Achievement_{achievement.id}_Progress", progressValue);
            
            // 목표 달성 시 자동 언락
            if (achievement.IsCompleted)
            {
                PlayerPrefs.SetInt($"Achievement_{achievement.id}", 1);
            }
            
            PlayerPrefs.Save();
        }
    }
    
    // 업적 진척도 증가 메서드
    public static void IncrementAchievementProgress(AchievementData achievement, int incrementValue = 1)
    {
        if (achievement != null)
        {
            UpdateAchievementProgress(achievement, achievement.currentValue + incrementValue);
        }
    }
    
    #region Achievement Type Specific Methods
    
    // Score 타입 업적 업데이트
    public static void UpdateScoreAchievement(AchievementData achievement, int score)
    {
        if (achievement != null && achievement.conditionType == AchievementConditionType.Score)
        {
            UpdateAchievementProgress(achievement, score);
        }
    }
    
    // PlayTime 타입 업적 업데이트 (초 단위)
    public static void UpdatePlayTimeAchievement(AchievementData achievement, int playTimeInSeconds)
    {
        if (achievement != null && achievement.conditionType == AchievementConditionType.PlayTime)
        {
            UpdateAchievementProgress(achievement, playTimeInSeconds);
        }
    }
    
    // CollectCoins 타입 업적 업데이트
    public static void UpdateCollectCoinsAchievement(AchievementData achievement, int coinCount)
    {
        if (achievement != null && achievement.conditionType == AchievementConditionType.CollectCoins)
        {
            UpdateAchievementProgress(achievement, coinCount);
        }
    }
    
    // CollectItems 타입 업적 업데이트
    public static void UpdateCollectItemsAchievement(AchievementData achievement, int itemCount)
    {
        if (achievement != null && achievement.conditionType == AchievementConditionType.CollectItems)
        {
            UpdateAchievementProgress(achievement, itemCount);
        }
    }
    
    // Custom 타입 업적 업데이트
    public static void UpdateCustomAchievement(AchievementData achievement, int customValue)
    {
        if (achievement != null && achievement.conditionType == AchievementConditionType.Custom)
        {
            UpdateAchievementProgress(achievement, customValue);
        }
    }
    
    // 조건 타입에 따라 자동으로 업적 업데이트
    public static void UpdateAchievementByType(AchievementData achievement, int value)
    {
        if (achievement == null) return;
        
        switch (achievement.conditionType)
        {
            case AchievementConditionType.Score:
                UpdateScoreAchievement(achievement, value);
                break;
            case AchievementConditionType.PlayTime:
                UpdatePlayTimeAchievement(achievement, value);
                break;
            case AchievementConditionType.CollectCoins:
                UpdateCollectCoinsAchievement(achievement, value);
                break;
            case AchievementConditionType.CollectItems:
                UpdateCollectItemsAchievement(achievement, value);
                break;
            case AchievementConditionType.Custom:
                UpdateCustomAchievement(achievement, value);
                break;
        }
    }
    
    #endregion
}
