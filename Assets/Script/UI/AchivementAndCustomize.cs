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
            
            // 진척도 로드
            achievement.currentValue = PlayerPrefs.GetInt($"Achievement_{achievement.id}_Progress", 0);
        }
    }

    private void PopulateAchievementList()
    {
        if (scrollViewContent == null)
        {
            Debug.LogError("ScrollViewContent가 연결되지 않았습니다!");
            return;
        }
        
        if (achievementItemPrefab == null)
        {
            Debug.LogError("AchievementItemPrefab이 연결되지 않았습니다!");
            return;
        }

        // 기존 아이템 제거
        foreach (Transform child in scrollViewContent)
        {
            Destroy(child.gameObject);
        }

        // 업적 아이템 생성
        foreach (var achievement in achievements)
        {
            GameObject item = Instantiate(achievementItemPrefab, scrollViewContent);
            item.SetActive(true);
            
            AchievementItem itemComponent = item.GetComponent<AchievementItem>();
            
            if (itemComponent != null)
            {
                itemComponent.Initialize(achievement, temporarySelections[achievement.id], null);
            }
            else
            {
                Debug.LogError("AchievementItem 컴포넌트를 찾을 수 없습니다!");
            }
        }
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
    
    // 업적 진척도 업데이트 메서드
    public void UpdateAchievementProgress(string achievementId, int progressValue)
    {
        var achievement = achievements.Find(a => a.id == achievementId);
        if (achievement != null)
        {
            achievement.currentValue = progressValue;
            PlayerPrefs.SetInt($"Achievement_{achievementId}_Progress", progressValue);
            
            // 목표 달성 시 자동 언락
            if (achievement.IsCompleted && !temporarySelections[achievementId])
            {
                temporarySelections[achievementId] = true;
                PlayerPrefs.SetInt($"Achievement_{achievementId}", 1);
            }
            
            PlayerPrefs.Save();
            PopulateAchievementList();
        }
    }
    
    // 업적 진척도 증가 메서드
    public void IncrementAchievementProgress(string achievementId, int incrementValue = 1)
    {
        var achievement = achievements.Find(a => a.id == achievementId);
        if (achievement != null)
        {
            UpdateAchievementProgress(achievementId, achievement.currentValue + incrementValue);
        }
    }
}

[System.Serializable]
public class AchievementData
{
    public string id;
    public string title;
    public string description;
    public Sprite icon;
    
    [Header("Progress Settings")]
    public AchievementConditionType conditionType;
    public int targetValue; // 목표값 (예: 10번 승리, 1000 점수)
    public int currentValue; // 현재 진척도
    
    public bool IsCompleted => currentValue >= targetValue;
    public float ProgressPercentage => targetValue > 0 ? (float)currentValue / targetValue : 0f;
}

public enum AchievementConditionType
{
    Score,         // 점수 달성
    PlayTime,      // 플레이 시간 (초)
    CollectCoins,     // 코인 획득
    CollectItems,  // 아이템 획득
    Custom         // 커스텀 조건
}