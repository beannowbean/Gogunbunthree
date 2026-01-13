using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

public class AchievementAndCustomize : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private Button quitButton;
    [SerializeField] private Transform scrollViewContent;
    [SerializeField] private GameObject achievementItemPrefab;

    [Header("Achievement Data")]
    [SerializeField] private List<AchievementData> achievements = new List<AchievementData>();

    private Dictionary<string, bool> temporarySelections = new Dictionary<string, bool>();

    private void Start()
    {
        InitializeButtons();
        LoadAchievements();
        PopulateAchievementList();
    }

    private void InitializeButtons()
    {
        if (quitButton != null)
            quitButton.onClick.AddListener(OnQuitClicked);
    }

    private void LoadAchievements()
    {
        // 기존 저장된 데이터 로드
        foreach (var achievement in achievements)
        {
            bool isUnlocked = PlayerPrefs.GetInt($"Achievement_{achievement.id}", 0) == 1;
            temporarySelections[achievement.id] = isUnlocked;
        }
    }

    private void PopulateAchievementList()
    {
        if (scrollViewContent == null || achievementItemPrefab == null)
            return;

        // 기존 아이템 제거
        foreach (Transform child in scrollViewContent)
        {
            Destroy(child.gameObject);
        }

        // 업적 아이템 생성
        foreach (var achievement in achievements)
        {
            GameObject item = Instantiate(achievementItemPrefab, scrollViewContent);
            AchievementItem itemComponent = item.GetComponent<AchievementItem>();
            
            if (itemComponent != null)
            {
                itemComponent.Initialize(achievement, temporarySelections[achievement.id], OnAchievementToggled);
            }
        }
    }

    private void OnAchievementToggled(string achievementId, bool isSelected)
    {
        temporarySelections[achievementId] = isSelected;
        // 변경 즉시 저장
        PlayerPrefs.SetInt($"Achievement_{achievementId}", isSelected ? 1 : 0);
        PlayerPrefs.Save();
    }

    private void OnQuitClicked()
    {
        // MainMenu로 이동 (이미 저장됨)
        SceneManager.LoadScene("MainMenu");
    }

    // 새로운 업적 추가 메서드
    public void AddAchievement(AchievementData newAchievement)
    {
        achievements.Add(newAchievement);
        temporarySelections[newAchievement.id] = false;
        PopulateAchievementList();
    }
}

[System.Serializable]
public class AchievementData
{
    public string id;
    public string title;
    public string description;
    public Sprite icon;
}

// AchievementItem.cs - 별도 파일로 생성 필요
public class AchievementItem : MonoBehaviour
{
    [SerializeField] private Text titleText;
    [SerializeField] private Text descriptionText;
    [SerializeField] private Image iconImage;
    [SerializeField] private Toggle toggle;

    private string achievementId;
    private System.Action<string, bool> onToggleChanged;

    public void Initialize(AchievementData data, bool isUnlocked, System.Action<string, bool> callback)
    {
        achievementId = data.id;
        onToggleChanged = callback;

        if (titleText != null)
            titleText.text = data.title;

        if (descriptionText != null)
            descriptionText.text = data.description;

        if (iconImage != null && data.icon != null)
            iconImage.sprite = data.icon;

        if (toggle != null)
        {
            toggle.isOn = isUnlocked;
            toggle.onValueChanged.AddListener(OnToggleValueChanged);
        }
    }

    private void OnToggleValueChanged(bool value)
    {
        onToggleChanged?.Invoke(achievementId, value);
    }
}