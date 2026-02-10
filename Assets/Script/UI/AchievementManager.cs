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
        if (IsAchievementUnlocked(achievementId)) return;

        PlayerPrefs.SetInt($"Achievement_{achievementId}", 1);
        PlayerPrefs.Save();

        // 매핑 테이블 참조하여 RankManager에 업적 완료 전송
        if (idMapping.TryGetValue(achievementId, out string rankKey))
        {
            if (RankManager.Instance != null)
            {
                RankManager.Instance.CompleteAchievement(rankKey);
                Debug.Log($"[업적 전송] {achievementId} -> {rankKey}");
            }
        }
    }

    // 업적 스크립트 키와 리더보드 키 매핑 테이블
    public readonly Dictionary<string, string> idMapping = new Dictionary<string, string>
    {
        { "Achieve_Newbie", "ach_01" },
        { "Achieve_Dumb", "ach_02" },
        { "Achieve_HitAndRun", "ach_03" },
        { "Achieve_TreasureHunter", "ach_04" },
        { "Achieve_Eagle", "ach_05" },
        { "Achieve_Bunny", "ach_06" },
        { "Achieve_Bruh", "ach_07" },
        { "Achieve_Acrophobia", "ach_08" },
        { "Achieve_TopGun", "ach_09" },
        { "Achieve_HeliVIP", "ach_10" },
        { "Achieve_Gentleman", "ach_11" },
        { "Achieve_Wrecker", "ach_12" },
        { "Achieve_Superstar", "ach_13" },
        { "Achieve_Iceman", "ach_14" },
        { "Achieve_Icarus", "ach_15" },
        { "Achieve_Hustler", "ach_16" },
        { "Achieve_SkyWalker", "ach_17" },
        { "Achieve_Pennyless", "ach_18" },
        { "Achieve_Billionaire", "ach_19" },
        { "Achieve_Rapunzel", "ach_20" }
    };
    
    /// <summary>
    /// 보상 수령
    /// </summary>
    public bool ClaimReward(string achievementId)
    {

        var achievement = GetAchievement(achievementId);
        
        // 보상 지급
        bool success = GiveReward(achievement);
        if (success)
        {
            // 보상 수령 완료 표시
            PlayerPrefs.SetInt($"Achievement_{achievementId}_Reward", 1);
            PlayerPrefs.Save();
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
            case RewardType.PlayerSkin:
                {
                    int index = achievement.rewardIndex;
                    Customize.Instance.UnlockPlayerSkinByIndex(index);
                    return true;
                }
            
            case RewardType.BeanieSkin:
                {
                    int index = achievement.rewardIndex;
                    Customize.Instance.UnlockBeanieSkinByIndex(index);
                    return true;
                }

            case RewardType.BagSkin:
                {
                    int index = achievement.rewardIndex;
                    Customize.Instance.UnlockBagSkinByIndex(index);
                    return true;
                }

            case RewardType.HelicopterSkin:
                {
                    int index = achievement.rewardIndex;
                    Customize.Instance.UnlockHelicopterSkinByIndex(index);
                    return true;
                }
                
            case RewardType.HookSkin:
                {
                    int index = achievement.rewardIndex;
                    Customize.Instance.UnlockHookSkinByIndex(index);
                    return true;
                }
                
            default:
                return false;
        }
    }

    /// <summary>
    /// 업적 완료 여부 확인
    /// </summary>
    public bool IsAchievementUnlocked(string achievementId)
    {
        return PlayerPrefs.GetInt($"Achievement_{achievementId}", 0) == 1;
    }

    #region Progress Update API

    /// <summary>
    /// 진척도 절댓값으로 설정
    /// </summary>
    public bool UpdateProgress(string achievementId, int value)
    {
        return UpdateAchievementProgress(achievementId, value);
    }

    /// <summary>
    /// 진척도를 상대값으로 증가
    /// </summary>
    public bool IncrementProgress(string achievementId, int incrementValue = 1)
    {
        return IncrementAchievementProgress(achievementId, incrementValue);
    }

    #endregion
}
