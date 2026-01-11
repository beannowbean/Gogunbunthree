using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

/*
 *  Coin 관련 Script에서, 코인 획득 시 아래의 함수 호출 필요
 *  ScoreManager.Instance.AddCoin(1)
 */

public class ScoreManager : MonoBehaviour
{

    public static ScoreManager Instance;

    public int coinCount = 0;           // 획득한 코인 수

    private int currentScore = 0;       // 최종 계산된 점수
    private int bestScore = 0;          // 최고 점수
    
    private bool IsNewRecord = false;   // 뉴 레코드 여부
    private bool isGameOver = false;    // 게임 오버 여부
    private bool showingBonus = false;  // 보너스 표시 중인지 여부
    private Coroutine bonusCoroutine = null; // 현재 실행 중인 보너스 코루틴

    public TextMeshProUGUI inGameScoreText; // 인게임 점수 표시용


    // 점수 계산 공식: 시간 * 속도 + 코인 * 100
    public float currentCarSpeed = 0f;  // 현재 자동차 속도 (외부에서 받아옴)
    private float survivalTime = 0f;    // 게임 생존 시간 (초 단위)

    void Awake()
    {
        Instance = this;
    }

    // Start is called before the first frame update
    void Start()
    {
        bestScore = PlayerPrefs.GetInt("BestScore", 0);
        
        if(currentScore < bestScore)
            inGameScoreText.text = $"SCORE:\t{currentScore}\nBEST:\t{bestScore - currentScore}";
        else if (currentScore == bestScore)
            inGameScoreText.text = $"SCORE:\t{currentScore}";
        
        // TEST: 최고 점수 초기화
        //ResetBestScore();
    }

    // Update is called once per frame
    void Update()
    {
        if (isGameOver || !UIController.tutorialSkip) return;

        // 1. 시간 흐름 측정
        survivalTime += Time.deltaTime;
        
        // 2. 점수 계산 공식: 시간 * 속도 + 코인 * 100
        float rawScore = (survivalTime * currentCarSpeed) + (coinCount * 100);
        currentScore = Mathf.FloorToInt(rawScore);

        // 점수 표시 업데이트 (보너스 표시 중이면 보너스도 함께 표시)
        UpdateScoreDisplay(showingBonus);
    }

    void UpdateScoreDisplay(bool showBonus)
    {
        string bonusText = showBonus ? " <color=#FF5A11>+100</color>" : "";
        
        if(currentScore < bestScore)
            inGameScoreText.text = $"SCORE:\t{currentScore} {bonusText}\nBEST:\t{bestScore - currentScore}";
        else if (currentScore == bestScore)
            inGameScoreText.text = $"SCORE:\t{currentScore} {bonusText}";
        else
            inGameScoreText.text = $"SCORE:\t{currentScore} {bonusText}\n<color=#FF5A11>NEW RECORD!</color>";
    }


    // 최종 점수 return
    public int GetCurrentScore()
    {
        return currentScore;
    }

    public int GetFinalScore()
    {
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
        
        // 기존 보너스 코루틴이 실행 중이면 중단
        if (bonusCoroutine != null)
        {
            StopCoroutine(bonusCoroutine);
        }
        
        // 새 보너스 코루틴 시작
        bonusCoroutine = StartCoroutine(ShowCoinBonus());
    }

    // 차량 속도 업데이트
    public void UpdateCarSpeed(float speed)
    {
        currentCarSpeed = speed;
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
        survivalTime = 0f;
        IsNewRecord = false;
        isGameOver = false;
    }

    public void StopScoring()
    {
        isGameOver = true;
    }

    IEnumerator ShowCoinBonus()
    {
        showingBonus = true;
        UpdateScoreDisplay(true);
        yield return new WaitForSeconds(0.3f);
        showingBonus = false;
        UpdateScoreDisplay(false);
        bonusCoroutine = null;
    }

    public bool GetIsNewRecord()
    {
        return IsNewRecord;
    }
}
