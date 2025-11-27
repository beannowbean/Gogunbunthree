using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class ScoreManager : MonoBehaviour
{

    public static ScoreManager Instance;

    public int coinCount = 0;           // 획득한 코인 수

    /* 획득한 코인 수로만 점수 계산
    public float currentCarSpeed = 0f;  // 현재 자동차 속도 (외부에서 받아옴)
    private float survivalTime = 0f;    // 게임 생존 시간 (초 단위)
    */

    private int currentScore = 0;       // 최종 계산된 점수
    private int bestScore = 0;          // 최고 점수
    
    private bool IsNewRecord = false;

    public TextMeshProUGUI inGameScoreText; // 인게임 점수 표시용

    // Start is called before the first frame update
    void Start()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        bestScore = PlayerPrefs.GetInt("BestScore", 0);
        // ResetBestScore();
    }

    // Update is called once per frame
    void Update()
    {
        // 1. 시간 흐름 측정
        // survivalTime += Time.deltaTime;

        // 2. 점수 계산 공식: (Coin + 시간 * 차 시속)
        // 계산 결과는 소수점이 나올 수 있으므로 Mathf.FloorToInt로 정수(int) 변환합니다.
        // float rawScore = coinCount + (survivalTime * currentCarSpeed);


        currentScore = coinCount;

        // 최고 점수 넘으면 저장
        if (currentScore > bestScore)
        {
            bestScore = currentScore;
            // 기기에 바로 저장 (Key 이름을 "BestScore"로 지정)
            PlayerPrefs.SetInt("BestScore", bestScore);
            IsNewRecord = true;
        }

        // 3. 인게임 UI에 실시간 표시
        if (inGameScoreText != null)
        {
            int displayBest = Mathf.Max(bestScore, currentScore);

            inGameScoreText.text = $"SCORE:\t{currentScore}\nBEST:\t{displayBest}";
        }
    }


    /// 최종 점수 return
    public int GetCurrentScore()
    {
        return currentScore;
    }

    // 최고 점수 return
    public int GetBestScore()
    {
        return bestScore;
    }

    // 코인 획득량
    // amount: 획득한 코인 수
    public void AddCoin(int amount)
    {
        coinCount += amount;
    }

    public void ResetBestScore()
    {
        bestScore = 0;
        PlayerPrefs.DeleteKey("BestScore");
    }

    public void ResetScore()
    {
        coinCount = 0;
        currentScore = 0;
    }

    public bool GetIsNewRecord()
    {
        return IsNewRecord;
    }
}
