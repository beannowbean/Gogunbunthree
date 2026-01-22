using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAchivementList : MonoBehaviour
{
    public static PlayerAchivementList Instance;

    void Awake()
    {
        if (Instance == null) Instance = this;
    }

    // Start is called before the first frame update
    void Start()
    {
        // [Newbie1] 게임 시작 시 체크
        Newbie();
    }

    // Update is called once per frame
    void Update()
    {

    }

    // 업적을 달성했는지 처리하는 함수
    private void UnlockAchievement(string key, string title)
    {
        // 이미 달성 했다면(저장된 값이 있음 = 1) 리턴
        if (PlayerPrefs.GetInt(key, 0) == 1) return;

        // 달성했다면 1을 저장
        PlayerPrefs.SetInt(key, 1);
        PlayerPrefs.Save(); // 저장 확정

        // 달성했는지 로그 확인
        Debug.Log($"[업적 달성] {title}!");

        // UI 코드 여기 추가
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