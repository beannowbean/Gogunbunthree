using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

public class Achievement : MonoBehaviour
{
    [Header("Achievement UI Elements")]
    [SerializeField] private Button quitButton;
    [SerializeField] private Transform scrollViewContent;
    [SerializeField] private GameObject achievementItemPrefab;

    private Dictionary<string, bool> temporarySelections = new Dictionary<string, bool>();
    private Dictionary<string, AchievementItem> achievementItems = new Dictionary<string, AchievementItem>();

    private void Start()
    {
        InitializeButtons();
        PopulateAchievementList();
    }
    
    private void OnEnable()
    {
        // 패널이 활성화될 때마다 최신 데이터로 갱신
        if (AchievementManager.Instance != null)
        {
            AchievementManager.Instance.RefreshAchievementData();
        }
        PopulateAchievementList();
    }

    private void InitializeButtons()
    {
        if (quitButton != null)
            quitButton.onClick.AddListener(OnQuitClicked);
    }

    private void PopulateAchievementList()
    {
        if (AchievementManager.Instance == null)
        {
            Debug.LogError("[Achievement] AchievementManager가 없습니다!");
            return;
        }

        // 기존 아이템 제거
        foreach (Transform child in scrollViewContent)
        {
            Destroy(child.gameObject);
        }
        achievementItems.Clear();
        temporarySelections.Clear();

        // 업적 아이템 생성
        List<AchievementData> achievements = AchievementManager.Instance.GetAllAchievements();
        
        Debug.Log($"[Achievement] 업적 {achievements.Count}개 표시 중");
        
        foreach (var achievement in achievements)
        {
            bool isUnlocked = AchievementManager.Instance.IsAchievementUnlocked(achievement.id);
            temporarySelections[achievement.id] = isUnlocked;

            GameObject item = Instantiate(achievementItemPrefab, scrollViewContent);
            item.SetActive(true);
            
            AchievementItem itemComponent = item.GetComponent<AchievementItem>();
            
            if (itemComponent != null)
            {
                itemComponent.Initialize(achievement, isUnlocked, null);
                achievementItems[achievement.id] = itemComponent;
            }
        }
    }

    private void OnQuitClicked()
    {
        // MainMenu로 이동 (이미 저장됨)
        SceneManager.LoadScene("MainMenu");
    }
    
    // 업적 리스트 새로고침 (외부에서 호출 가능)
    public void RefreshAchievementList()
    {
        PopulateAchievementList();
    }
    
    // 개별 업적 아이템 UI 업데이트
    public void UpdateAchievementItemUI(string achievementId)
    {
        if (AchievementManager.Instance == null) return;

        if (achievementItems.TryGetValue(achievementId, out AchievementItem item))
        {
            var achievement = AchievementManager.Instance.GetAchievement(achievementId);
            if (achievement != null)
            {
                item.UpdateDisplay(achievement);
                
                // temporarySelections 업데이트
                if (achievement.IsCompleted && !temporarySelections[achievementId])
                {
                    temporarySelections[achievementId] = true;
                }
            }
        }
    }
}
