using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAchivementList : MonoBehaviour
{
    public static PlayerAchivementList Instance;

    [Header("Achievement Definitions")]
    [SerializeField] private List<AchievementDefinition> achievementDefinitions = new List<AchievementDefinition>();

    void Awake()
    {
        if (Instance == null) Instance = this;
    }

    // Start is called before the first frame update
    void Start()
    {
        // 업적 데이터를 AchievementManager에 등록
        RegisterAchievementsToManager();
        
        // [Newbie1] 게임 시작 시 체크
        Newbie();
    }

    // Update is called once per frame
    void Update()
    {

    }
    
    /// <summary>
    /// 정의된 업적들을 AchievementManager에 등록
    /// </summary>
    private void RegisterAchievementsToManager()
    {
        if (AchievementManager.Instance == null)
        {
            Debug.LogError("[PlayerAchievementList] AchievementManager가 없습니다! MainMenu 씬에 AchievementManager를 추가하세요.");
            return;
        }

        // Inspector에서 정의된 업적들을 Manager에 등록
        foreach (var definition in achievementDefinitions)
        {
            AchievementManager.Instance.RegisterAchievement(definition.ToAchievementData());
        }
        
        Debug.Log($"[PlayerAchievementList] {achievementDefinitions.Count}개 업적 등록 완료");
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
    public string rewardId;
    
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
            rewardId = this.rewardId
        };
    }
}