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
    public int carKnockCount = 0;       // 무적 상태에서 날린 차 수

    private int currentScore = 0;       // 최종 계산된 점수
    private int bestScore = 0;          // 최고 점수
    
    private bool IsNewRecord = false;   // 뉴 레코드 여부
    private bool isGameOver = false;    // 게임 오버 여부

    public TextMeshProUGUI inGameScoreText; // 인게임 점수 표시용
    public TextMeshProUGUI bonusScoreText;  // 보너스 점수 표시용 (점수 위에 표시)


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
        
        UpdateScoreDisplay();
        
        // TEST: 최고 점수 초기화
        //ResetBestScore();
    }

    // Update is called once per frame
    void Update()
    {
        if (isGameOver || !UIController.tutorialSkip) return;

        // 1. 시간 흐름 측정
        survivalTime += Time.deltaTime;
        
        // 2. 점수 계산 공식: 시간 * 속도 + 코인 * 100 + 차 날리기 * 50
        float rawScore = (survivalTime * currentCarSpeed) + (coinCount * 100) + (carKnockCount * 50);
        currentScore = Mathf.FloorToInt(rawScore);

        // 점수 표시 업데이트
        UpdateScoreDisplay();
    }

    void UpdateScoreDisplay()
    {
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
        
        // 코인 먹을 때마다 보너스 표시 (중단하지 않음)
        StartCoroutine(ShowBonus("+100"));
    }

    // 무적 상태에서 차 날리기 점수 추가
    public void AddCarKnockScore()
    {
        carKnockCount++;
        StartCoroutine(ShowBonus("+50"));
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
        carKnockCount = 0;
        currentScore = 0;
        survivalTime = 0f;
        IsNewRecord = false;
        isGameOver = false;
    }

    public void StopScoring()
    {
        isGameOver = true;
    }

    IEnumerator ShowBonus(string bonusAmount)
    {
        // bonusScoreText가 설정되어 있으면 애니메이션 효과와 함께 표시
        if (bonusScoreText != null)
        {
            // 원본을 복제하여 새로운 텍스트 생성 (동시에 여러 개 표시 가능)
            GameObject bonusObj = Instantiate(bonusScoreText.gameObject, bonusScoreText.transform.parent);
            TextMeshProUGUI bonusText = bonusObj.GetComponent<TextMeshProUGUI>();
            bonusText.text = bonusAmount;
            bonusObj.SetActive(true);
            
            // 초기 위치와 알파값 설정
            bonusText.transform.localPosition = bonusScoreText.transform.localPosition;
            Color startColor = bonusText.color;
            startColor.a = 1f;
            bonusText.color = startColor;
            
            Vector3 startPos = bonusText.transform.localPosition;
            
            // 애니메이션 실행 (위로 올라가면서 페이드 아웃)
            float duration = 0.8f;
            float elapsed = 0f;
            
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / duration;
                
                // 위로 이동
                Vector3 newPos = startPos;
                newPos.y = startPos.y + (t * 100f); // 100 픽셀 위로 이동
                bonusText.transform.localPosition = newPos;
                
                // 페이드 아웃
                Color newColor = startColor;
                newColor.a = 1f - t;
                bonusText.color = newColor;
                
                yield return null;
            }
            
            // 애니메이션이 끝나면 오브젝트 삭제
            Destroy(bonusObj);
        }
        else
        {
            // bonusScoreText가 없으면 아무것도 하지 않음
            yield return null;
        }
    }

    public bool GetIsNewRecord()
    {
        return IsNewRecord;
    }
}
