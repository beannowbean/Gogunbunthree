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

        // 잠금 상태 표시
        if (lockOverlay != null)
            lockOverlay.gameObject.SetActive(!isUnlocked);
        
        // 진척도 UI 업데이트
        UpdateProgress(data);
    }
    
    private void UpdateProgress(AchievementData data)
    {
        // targetValue가 0이면 진척도 UI 숨김
        bool showProgress = data.targetValue > 0 && !data.IsCompleted;
        
        if (progressContainer != null)
        {
            progressContainer.SetActive(showProgress);
        }
        
        if (progressSlider != null && data.targetValue > 0)
        {
            progressSlider.maxValue = data.targetValue;
            progressSlider.value = data.currentValue;
            Debug.Log($"Slider 설정: {data.currentValue}/{data.targetValue}");
        }
        
        if (progressText != null && data.targetValue > 0)
        {
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
}
