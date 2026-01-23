using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAchivementList : MonoBehaviour
{
    public static PlayerAchivementList Instance;

    private List<AchievementDefinition> achievementDefinitions = new List<AchievementDefinition>();

    void Awake()
    {
        if (Instance == null) Instance = this;
        
        // 업적 정의 초기화
        InitializeAchievements();
    }

    // Start is called before the first frame update
    void Start()
    {
        // 업적 데이터를 AchievementManager에 등록
        RegisterAchievementsToManager();
        
        // Newbie는 Start 버튼 클릭 시 달성되므로 여기서 호출하지 않음
    }

    // Update is called once per frame
    void Update()
    {

    }
    
    /// <summary>
    /// 업적 정의 초기화
    /// </summary>
    private void InitializeAchievements()
    {
        // 난이도 1짜리 업적
        achievementDefinitions.Add(new AchievementDefinition
        {
            id = "Achieve_Newbie",                                  // PlayerPrefs 키
            title = "Newbie",                                       // 업적 이름
            description = "",                                       // 업적 설명
            icon = null,                                            // 업적 보상 아이콘: 보상 이미지
            conditionType = AchievementConditionType.Custom,        // 조건타입: 점수, 코인 등등
            targetValue = 1,                                        // 달성조건: e.g. 갯수
            rewardType = RewardType.PlayerSkin,                     // 보상타입: 스킨, 코인 등등
            rewardIndex = 0                                         // e.g. Customize.playerSkins[0]을 보상으로 설정
        });

        achievementDefinitions.Add(new AchievementDefinition
        {
            id = "Achieve_Dumb",
            title = "Dumb",
            description = "",
            icon = null,
            conditionType = AchievementConditionType.Custom,
            targetValue = 1,
            rewardType = RewardType.None
        });

        // 난이도 2짜리 업적
        achievementDefinitions.Add(new AchievementDefinition
        {
            id = "Achieve_HitAndRun",
            title = "Hit And Run",
            description = "",
            icon = null,
            conditionType = AchievementConditionType.Custom,
            targetValue = 1,
            rewardType = RewardType.None
        });

        achievementDefinitions.Add(new AchievementDefinition
        {
            id = "Achieve_TreasureHunter",
            title = "Treasure Hunter",
            description = "",
            icon = null,
            conditionType = AchievementConditionType.Custom,
            targetValue = 1,
            rewardType = RewardType.None
        });

        achievementDefinitions.Add(new AchievementDefinition
        {
            id = "Achieve_Eagle",
            title = "Eagle",
            description = "",
            icon = null,
            conditionType = AchievementConditionType.Custom,
            targetValue = 1,
            rewardType = RewardType.None
        });

        // 난이도 3짜리 업적
        achievementDefinitions.Add(new AchievementDefinition
        {
            id = "Achieve_Bunny",
            title = "Bunny",
            description = "",
            icon = null,
            conditionType = AchievementConditionType.Custom,
            targetValue = 1,
            rewardType = RewardType.None
        });

        achievementDefinitions.Add(new AchievementDefinition
        {
            id = "Achieve_Bruh",
            title = "Bruh",
            description = "",
            icon = null,
            conditionType = AchievementConditionType.Custom,
            targetValue = 1,
            rewardType = RewardType.None
        });

        achievementDefinitions.Add(new AchievementDefinition
        {
            id = "Achieve_Acrophobia",
            title = "Acrophobia",
            description = "",
            icon = null,
            conditionType = AchievementConditionType.Custom,
            targetValue = 1,
            rewardType = RewardType.None
        });

        // 난이도 4짜리 업적
        achievementDefinitions.Add(new AchievementDefinition
        {
            id = "Achieve_TopGun",
            title = "Top Gun",
            description = "",
            icon = null,
            conditionType = AchievementConditionType.Custom,
            targetValue = 1,
            rewardType = RewardType.None
        });

        achievementDefinitions.Add(new AchievementDefinition
        {
            id = "Achieve_HeliVIP",
            title = "Heli VIP",
            description = "",
            icon = null,
            conditionType = AchievementConditionType.Custom,
            targetValue = 1,
            rewardType = RewardType.None
        });

        achievementDefinitions.Add(new AchievementDefinition
        {
            id = "Achieve_Gentleman",
            title = "Gentleman",
            description = "",
            icon = null,
            conditionType = AchievementConditionType.Custom,
            targetValue = 1,
            rewardType = RewardType.None
        });

        achievementDefinitions.Add(new AchievementDefinition
        {
            id = "Achieve_Wrecker",
            title = "Wrecker",
            description = "",
            icon = null,
            conditionType = AchievementConditionType.Custom,
            targetValue = 1,
            rewardType = RewardType.None
        });

        // 난이도 5짜리 업적
        achievementDefinitions.Add(new AchievementDefinition
        {
            id = "Achieve_Superstar",
            title = "Superstar",
            description = "",
            icon = null,
            conditionType = AchievementConditionType.Custom,
            targetValue = 1,
            rewardType = RewardType.None
        });

        achievementDefinitions.Add(new AchievementDefinition
        {
            id = "Achieve_Iceman",
            title = "Iceman",
            description = "",
            icon = null,
            conditionType = AchievementConditionType.Custom,
            targetValue = 1,
            rewardType = RewardType.None
        });

        achievementDefinitions.Add(new AchievementDefinition
        {
            id = "Achieve_Icarus",
            title = "Icarus",
            description = "",
            icon = null,
            conditionType = AchievementConditionType.Custom,
            targetValue = 1,
            rewardType = RewardType.None
        });

        achievementDefinitions.Add(new AchievementDefinition
        {
            id = "Achieve_Hustler",
            title = "Hustler",
            description = "",
            icon = null,
            conditionType = AchievementConditionType.Custom,
            targetValue = 1,
            rewardType = RewardType.None
        });

        // 난이도 6짜리 업적
        achievementDefinitions.Add(new AchievementDefinition
        {
            id = "Achieve_SkyWalker",
            title = "Sky Walker",
            description = "",
            icon = null,
            conditionType = AchievementConditionType.Custom,
            targetValue = 1,
            rewardType = RewardType.None
        });

        achievementDefinitions.Add(new AchievementDefinition
        {
            id = "Achieve_Pennyless",
            title = "Pennyless",
            description = "",
            icon = null,
            conditionType = AchievementConditionType.Custom,
            targetValue = 1,
            rewardType = RewardType.None
        });

        // 난이도 7짜리 업적
        achievementDefinitions.Add(new AchievementDefinition
        {
            id = "Achieve_Billionaire",
            title = "Billionaire",
            description = "",
            icon = null,
            conditionType = AchievementConditionType.Custom,
            targetValue = 1,
            rewardType = RewardType.None
        });

        achievementDefinitions.Add(new AchievementDefinition
        {
            id = "Achieve_Rapunzel",
            title = "Rapunzel",
            description = "",
            icon = null,
            conditionType = AchievementConditionType.Custom,
            targetValue = 1,
            rewardType = RewardType.None
        });
    }
    
    /// <summary>
    /// 정의된 업적들을 AchievementManager에 등록
    /// </summary>
    private void RegisterAchievementsToManager()
    {

        // 코드에서 정의된 업적들을 Manager에 등록
        foreach (var definition in achievementDefinitions)
        {
            AchievementManager.Instance.RegisterAchievement(definition.ToAchievementData());
        }
    }

    // 업적을 달성했는지 처리하는 함수
    private void UnlockAchievement(string achievementId, string title)
    {
        if (AchievementManager.Instance != null)
        {
            AchievementManager.Instance.UnlockAchievement(achievementId);
        }
        else
        {
            // Manager가 없을 경우 기존 방식 사용
            if (PlayerPrefs.GetInt(achievementId, 0) == 1) return;
            PlayerPrefs.SetInt(achievementId, 1);
            PlayerPrefs.Save();
            Debug.Log($"[업적 달성] {title}!");
        }
    } 

    /* 
     * * 난이도 1짜리 업적 목록
     */

    // IN
    public void Newbie()
    {
        // key = Achieve_Newbie, 이름 = Newbie
        UnlockAchievement("Achieve_Newbie", "Newbie");
    }

    //
    public void Dumb()
    {
        // key = Achieve_Dumb, 이름 = Dumb
        UnlockAchievement("Achieve_Dumb", "Dumb");
    }

    /* 
     * * 난이도 2짜리 업적 목록
     */

    public void HitAndRun()
    {
        // key = Achieve_HitAndRun, 이름 = HitAndRun
        UnlockAchievement("Achieve_HitAndRun", "Hit And Run");
    }

    public void TreasureHunter()
    {
        // key = Achieve_TreasureHunter, 이름 = TreasureHunter
        UnlockAchievement("Achieve_TreasureHunter", "Treasure Hunter");
    }

    public void Eagle()
    {
        // key = Achieve_Eagle, 이름 = Eagle
        UnlockAchievement("Achieve_Eagle", "Eagle");
    }

    /* 
     * * 난이도 3짜리 업적 목록
     */

    public void Bunny()
    {
        // key = Achieve_Bunny, 이름 = Bunny
        UnlockAchievement("Achieve_Bunny", "Bunny");
    }

    public void Bruh()
    {
        // key = Achieve_Bruh, 이름 = Bruh
        UnlockAchievement("Achieve_Bruh", "Bruh");
    }

    public void Acrophobia()
    {
        // key = Achieve_Acrophobia, 이름 = Acrophobia
        UnlockAchievement("Achieve_Acrophobia", "Acrophobia");
    }

    /* 
     * * 난이도 4짜리 업적 목록
     */

    public void TopGun()
    {
        // key = Achieve_TopGun, 이름 = TopGun
        UnlockAchievement("Achieve_TopGun", "Top Gun");
    }

    public void HeliVIP()
    {
        // key = Achieve_HeliVIP, 이름 = HeliVIP
        UnlockAchievement("Achieve_HeliVIP", "Heli VIP");
    }

    public void Gentleman()
    {
        // key = Achieve_Gentleman, 이름 = Gentleman
        UnlockAchievement("Achieve_Gentleman", "Gentleman");
    }

    public void Wrecker()
    {
        // key = Achieve_Wrecker, 이름 = Wrecker
        UnlockAchievement("Achieve_Wrecker", "Wrecker");
    }

    /* 
     * * 난이도 5짜리 업적 목록
     */

    public void Superstar()
    {
        // key = Achieve_Superstar, 이름 = Superstar
        UnlockAchievement("Achieve_Superstar", "Superstar");
    }

    public void Iceman()
    {
        // key = Achieve_Iceman, 이름 = Iceman
        UnlockAchievement("Achieve_Iceman", "Iceman");
    }

    public void Icarus()
    {
        // key = Achieve_Icarus, 이름 = Icarus
        UnlockAchievement("Achieve_Icarus", "Icarus");
    }

    public void Hustler()
    {
        // key = Achieve_Hustler, 이름 = Hustler
        UnlockAchievement("Achieve_Hustler", "Hustler");
    }

    /* 
     * * 난이도 6짜리 업적 목록
     */

    public void SkyWalker()
    {
        // key = Achieve_SkyWalker, 이름 = SkyWalker
        UnlockAchievement("Achieve_SkyWalker", "Sky Walker");
    }

    public void Pennyless()
    {
        // key = Achieve_Pennyless, 이름 = Pennyless
        UnlockAchievement("Achieve_Pennyless", "Pennyless");
    }

    /* 
     * * 난이도 7짜리 업적 목록
     */

    public void Billionaire()
    {
        // key = Achieve_Billionaire, 이름 = Billionaire
        UnlockAchievement("Achieve_Billionaire", "Billionaire");
    }

    public void Rapunzel()
    {
        // key = Achieve_Rapunzel, 이름 = Rapunzel
        UnlockAchievement("Achieve_Rapunzel", "Rapunzel");
    }
}

/// <summary>
/// Inspector에서 업적을 정의하기 위한 클래스
/// </summary>
[System.Serializable]
public class AchievementDefinition
{
    public string id;
    public string title;
    [TextArea(2, 4)]
    public string description;
    public Sprite icon;
    public AchievementConditionType conditionType;
    public int targetValue;
    
    [Header("Reward Settings")]
    public RewardType rewardType;
    public int rewardIndex = -1; // Customize 리스트의 인덱스 (Inspector에서 설정)
    
    /// <summary>
    /// AchievementData로 변환
    /// </summary>
    public AchievementData ToAchievementData()
    {
        return new AchievementData
        {
            id = this.id,
            title = this.title,
            description = this.description,
            icon = this.icon,
            conditionType = this.conditionType,
            targetValue = this.targetValue,
            currentValue = 0,
            rewardType = this.rewardType,
            rewardIndex = this.rewardIndex
        };
    }
}