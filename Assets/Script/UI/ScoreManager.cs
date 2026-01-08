using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

/*
 *  Coin 관련 Script에서, 코인 획득 시 아래의 함수 호출 필요
 *  ScoreManager.Instance.AddCoin(1)
 *  
 *  helicopter 관련 script에서, isCleared 변수 설정 필요
 *  변수 형식: boolean
 *  ScoreManager.Instance.isCleared;
 */

public class ScoreManager : MonoBehaviour
{

    public static ScoreManager Instance;

    public int coinCount = 0;           // 획득한 코인 수

    private int currentScore = 0;       // 최종 계산된 점수
    private int bestScore = 0;          // 최고 점수
    
    private bool IsNewRecord = false;   // 뉴 레코드 여부
    public bool isCleared = true;       // 클리어 여부

    public TextMeshProUGUI inGameScoreText; // 인게임 점수 표시용

    /* 획득한 코인 수로만 점수 계산
    public float currentCarSpeed = 0f;  // 현재 자동차 속도 (외부에서 받아옴)
    private float survivalTime = 0f;    // 게임 생존 시간 (초 단위)
    */

    void Awake()
    {
        Instance = this;
    }

    // Start is called before the first frame update
    void Start()
    {
        bestScore = PlayerPrefs.GetInt("BestScore", 0);
        
        // TEST: 최고 점수 초기화
        //ResetBestScore();
    }

    // Update is called once per frame
    void Update()
    {
        /* 코인 획득으로만 점수 계산
        // 1. 시간 흐름 측정
        survivalTime += Time.deltaTime;
        // 2. 점수 계산 공식: (Coin + 시간 * 차 시속)
        // 계산 결과는 소수점이 나올 수 있으므로 Mathf.FloorToInt로 정수(int) 변환합니다.
        float rawScore = coinCount + (survivalTime * currentCarSpeed);
        */

        currentScore = coinCount;

        // 점수 표시 업데이트
        if(currentScore < bestScore)
            inGameScoreText.text = $"SCORE:\t{currentScore}\nBEST:\t{bestScore - currentScore}";
        else if (currentScore == bestScore)
            inGameScoreText.text = $"SCORE:\t{currentScore}";
        else
            inGameScoreText.text = $"SCORE:\t{currentScore}\n<color=#FF5A11>NEW RECORD!</color>";
    }


    // 최종 점수 return
    public int GetCurrentScore()
    {
        return currentScore;
    }

    public int GetFinalScore()
    {
        if (!isCleared)
        {
            currentScore -= 200;
            if (currentScore < 0) currentScore = 0;
        }

        if (currentScore > bestScore)
        {
            bestScore = currentScore;

            // 기기에 저장 (Key 이름을 "BestScore"로 지정)
            PlayerPrefs.SetInt("BestScore", bestScore);
            IsNewRecord = true;
        }

        return currentScore;
    }

    // 코인 획득량
    // amount: 획득한 코인 수
    public void AddCoin(int amount)
    {
        coinCount += amount;
    }

    // TEST: 최고 점수 초기화
    public void ResetBestScore()
    {
        bestScore = 0;
        PlayerPrefs.DeleteKey("BestScore");
    }

    public void ResetScore()
    {
        coinCount = 0;
        currentScore = 0;
        IsNewRecord = false;
        isCleared = true;
    }

    public bool GetIsNewRecord()
    {
        return IsNewRecord;
    }
}
