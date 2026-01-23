using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// 업적 시스템의 핵심 로직을 관리하는 매니저 클래스
/// - 업적 데이터 로드/저장
/// - 진척도 업데이트
/// - 업적 완료 체크
/// </summary>
public class AchievementManager : MonoBehaviour
{
    public static AchievementManager Instance { get; private set; }
    
    private List<AchievementData> achievements = new List<AchievementData>();
    private Dictionary<string, AchievementData> achievementDict = new Dictionary<string, AchievementData>();

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Update()
    {
        // 테스트: O 키로 업적 진척도 초기화
        if (Input.GetKeyDown(KeyCode.O))
        {
            ResetAllAchievementProgress();
        }
    }
    
    /// <summary>
    /// 모든 업적 진척도 초기화 (테스트용)
    /// </summary>
    private void ResetAllAchievementProgress()
    {
        foreach (var achievement in achievements)
        {
            achievement.currentValue = 0;
            PlayerPrefs.SetInt($"Achievement_{achievement.id}_Progress", 0);
            PlayerPrefs.SetInt($"Achievement_{achievement.id}", 0);
            PlayerPrefs.SetInt($"Achievement_{achievement.id}_Reward", 0);
        }
        PlayerPrefs.Save();
        
        Debug.Log("[테스트] 모든 업적 진척도가 초기화되었습니다.");
        
        // UI가 있다면 새로고침
        RefreshAchievementData();
    }

    /// <summary>
    /// 외부에서 업적 등록 (PlayerAchievementList에서 사용)
    /// </summary>
    public void RegisterAchievement(AchievementData achievement)
    {
        if (achievement == null || string.IsNullOrEmpty(achievement.id)) return;

        if (!achievementDict.ContainsKey(achievement.id))
        {
            achievements.Add(achievement);
            achievementDict.Add(achievement.id, achievement);
            LoadAchievementData(achievement);
        }
    }

    /// <summary>
    /// PlayerPrefs에서 업적 데이터 로드
    /// </summary>
    private void LoadAchievementData(AchievementData achievement)
    {
        // 완료 여부 로드
        bool isCompleted = PlayerPrefs.GetInt($"Achievement_{achievement.id}", 0) == 1;
        
        // 진척도 로드
        if (PlayerPrefs.HasKey($"Achievement_{achievement.id}_Progress"))
        {
            achievement.currentValue = PlayerPrefs.GetInt($"Achievement_{achievement.id}_Progress", 0);
        }
        
        // 완료 상태였는데 진척도가 목표에 미달이면 강제로 목표 달성으로 설정
        if (isCompleted && achievement.currentValue < achievement.targetValue)
        {
            achievement.currentValue = achievement.targetValue;
        }
    }
    
    /// <summary>
    /// 모든 업적 데이터를 PlayerPrefs에서 다시 로드
    /// </summary>
    public void RefreshAchievementData()
    {
        foreach (var achievement in achievements)
        {
            LoadAchievementData(achievement);
        }
    }

    /// <summary>
    /// ID로 업적 데이터 가져오기
    /// </summary>
    public AchievementData GetAchievement(string achievementId)
    {
        achievementDict.TryGetValue(achievementId, out AchievementData achievement);
        return achievement;
    }

    /// <summary>
    /// 모든 업적 리스트 가져오기
    /// </summary>
    public List<AchievementData> GetAllAchievements()
    {
        return new List<AchievementData>(achievements);
    }

    /// <summary>
    /// 업적 진척도 업데이트 (절대값)
    /// </summary>
    public bool UpdateAchievementProgress(string achievementId, int progressValue)
    {
        var achievement = GetAchievement(achievementId);
        if (achievement == null) return false;

        bool wasCompleted = achievement.IsCompleted;
        
        achievement.currentValue = progressValue;
        PlayerPrefs.SetInt($"Achievement_{achievement.id}_Progress", progressValue);
        
        // 목표 달성 시 자동 언락
        if (achievement.IsCompleted && !wasCompleted)
        {
            UnlockAchievement(achievementId);
            return true; // 새로 완료됨
        }
        
        PlayerPrefs.Save();
        return false;
    }

    /// <summary>
    /// 업적 진척도 증가 (상대값)
    /// </summary>
    public bool IncrementAchievementProgress(string achievementId, int incrementValue = 1)
    {
        var achievement = GetAchievement(achievementId);
        if (achievement == null) return false;

        return UpdateAchievementProgress(achievementId, achievement.currentValue + incrementValue);
    }

    /// <summary>
    /// 업적 언락 (강제 완료)
    /// </summary>
    public void UnlockAchievement(string achievementId)
    {
        var achievement = GetAchievement(achievementId);
        if (achievement == null) return;

        PlayerPrefs.SetInt($"Achievement_{achievementId}", 1);
        PlayerPrefs.Save();
        
        Debug.Log($"[업적 달성] {achievement.title}!");
        
        // TODO: UI 알림 팝업
    }
    
    /// <summary>
    /// 보상 수령
    /// </summary>
    public bool ClaimReward(string achievementId)
    {
        var achievement = GetAchievement(achievementId);
        if (achievement == null || !achievement.IsCompleted) return false;
        
        // 이미 수령했는지 확인
        if (IsRewardClaimed(achievementId)) return false;
        
        // 보상 지급
        bool success = GiveReward(achievement);
        
        if (success)
        {
            // 보상 수령 완료 표시
            PlayerPrefs.SetInt($"Achievement_{achievementId}_Reward", 1);
            PlayerPrefs.Save();
            
            Debug.Log($"[보상 수령] {achievement.title} - {achievement.rewardType}");
        }
        
        return success;
    }
    
    /// <summary>
    /// 보상 수령 여부 확인
    /// </summary>
    public bool IsRewardClaimed(string achievementId)
    {
        return PlayerPrefs.GetInt($"Achievement_{achievementId}_Reward", 0) == 1;
    }
    
    /// <summary>
    /// 실제 보상 지급 처리
    /// </summary>
    private bool GiveReward(AchievementData achievement)
    {
        switch (achievement.rewardType)
        {
            case RewardType.None:
                return false;
                
            case RewardType.PlayerSkin:
                // TODO: 플레이어 스킨 해금
                Debug.Log($"플레이어 스킨 해금: {achievement.rewardId}");
                UnlockPlayerSkin(achievement.rewardId);
                return true;
                
            case RewardType.RopeSkin:
                // TODO: 로프 스킨 해금
                Debug.Log($"로프 스킨 해금: {achievement.rewardId}");
                UnlockRopeSkin(achievement.rewardId);
                return true;
                
            case RewardType.Coins:
                // TODO: 코인 지급
                Debug.Log($"코인 지급: {achievement.rewardId}");
                return true;
                
            case RewardType.CustomItem:
                // TODO: 커스텀 아이템 지급
                Debug.Log($"커스텀 아이템 지급: {achievement.rewardId}");
                return true;
                
            default:
                return false;
        }
    }
    
    /// <summary>
    /// 플레이어 스킨 해금 (차후 구현)
    /// </summary>
    private void UnlockPlayerSkin(string skinId)
    {
        // TODO: Customize 시스템과 연동
        PlayerPrefs.SetInt($"PlayerSkin_{skinId}_Unlocked", 1);
        PlayerPrefs.Save();
    }
    
    /// <summary>
    /// 로프 스킨 해금 (차후 구현)
    /// </summary>
    private void UnlockRopeSkin(string skinId)
    {
        // TODO: Customize 시스템과 연동
        PlayerPrefs.SetInt($"RopeSkin_{skinId}_Unlocked", 1);
        PlayerPrefs.Save();
    }

    /// <summary>
    /// 업적 완료 여부 확인
    /// </summary>
    public bool IsAchievementUnlocked(string achievementId)
    {
        return PlayerPrefs.GetInt($"Achievement_{achievementId}", 0) == 1;
    }

    #region Type Specific Update Methods
    
    /// <summary>
    /// Score 타입 업적 업데이트
    /// </summary>
    public bool UpdateScoreAchievement(string achievementId, int score)
    {
        var achievement = GetAchievement(achievementId);
        if (achievement != null && achievement.conditionType == AchievementConditionType.Score)
        {
            return UpdateAchievementProgress(achievementId, score);
        }
        return false;
    }
    
    /// <summary>
    /// PlayTime 타입 업적 업데이트 (초 단위)
    /// </summary>
    public bool UpdatePlayTimeAchievement(string achievementId, int playTimeInSeconds)
    {
        var achievement = GetAchievement(achievementId);
        if (achievement != null && achievement.conditionType == AchievementConditionType.PlayTime)
        {
            return UpdateAchievementProgress(achievementId, playTimeInSeconds);
        }
        return false;
    }
    
    /// <summary>
    /// CollectCoins 타입 업적 업데이트
    /// </summary>
    public bool UpdateCollectCoinsAchievement(string achievementId, int coinCount)
    {
        var achievement = GetAchievement(achievementId);
        if (achievement != null && achievement.conditionType == AchievementConditionType.CollectCoins)
        {
            return UpdateAchievementProgress(achievementId, coinCount);
        }
        return false;
    }
    
    /// <summary>
    /// CollectItems 타입 업적 업데이트
    /// </summary>
    public bool UpdateCollectItemsAchievement(string achievementId, int itemCount)
    {
        var achievement = GetAchievement(achievementId);
        if (achievement != null && achievement.conditionType == AchievementConditionType.CollectItems)
        {
            return UpdateAchievementProgress(achievementId, itemCount);
        }
        return false;
    }
    
    /// <summary>
    /// Custom 타입 업적 업데이트
    /// </summary>
    public bool UpdateCustomAchievement(string achievementId, int customValue)
    {
        var achievement = GetAchievement(achievementId);
        if (achievement != null && achievement.conditionType == AchievementConditionType.Custom)
        {
            return UpdateAchievementProgress(achievementId, customValue);
        }
        return false;
    }
    
    #endregion
}
