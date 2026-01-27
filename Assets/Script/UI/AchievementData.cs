using UnityEngine;

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
    
    [Header("Reward Settings")]
    public RewardType rewardType;
    // 인스펙터에서 직접 선택 가능한 리스트 인덱스 (Customize의 리스트 인덱스)
    public int rewardIndex = -1; // -1 means not set
    
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

public enum RewardType
{
    None,          // 보상 없음
    PlayerSkin,    // 플레이어 스킨
    RopeSkin,
    HookSkin,      // 로프 스킨
    CustomItem,    // 커스텀 아이템
    BagSkin,       // 가방 스킨
    BeanieSkin,    // 비니 스킨
    HelicopterSkin // 헬리콥터 스킨
}
