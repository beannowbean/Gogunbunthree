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

    [Header("Score Settings")]
    private const int COIN_SCORE_VALUE = 10;  // 코인 1개당 점수 가중치
    private const int CAR_KNOCK_SCORE_VALUE = 25; // 차 날리기당 점수 가중치

    [Header("Game Stats")]
    public int coinCount = 0;           // 획득한 코인 수
    public int carKnockCount = 0;       // 무적 상태에서 날린 차 수
    public float currentCarSpeed = 0f;  // 현재 자동차 속도 (외부에서 받아옴)

    [Header("Score Data")]
    private int currentScore = 0;       // 최종 계산된 점수
    private int bestScore = 0;          // 최고 점수
    private int lastDisplayedScore = -1; // 마지막으로 표시된 점수 (최적화용)
    private float survivalTime = 0f;    // 게임 생존 시간 (초 단위)
    
    [Header("State")]
    private bool IsNewRecord = false;   // 뉴 레코드 여부
    private bool isGameOver = false;    // 게임 오버 여부

    [Header("UI References")]
    public TextMeshProUGUI inGameScoreText; // 현재 점수 표시용
    public TextMeshProUGUI inGameCoinText;  // 현재 코인 수 표시용
    public TextMeshProUGUI bestScoreText;   // 베스트 점수 표시용
    public TextMeshProUGUI bonusScoreText;  // 보너스 점수 표시용 (점수 위에 표시)

    [Header("Achievement")]
    private bool hasAchievedBillionaire = false;    // [19. Billionaire] 업적 중복 체크
    private bool hasAchievedHustler = false;        // [16. Hustler] 업적 중복 체크
    private bool hasAchievedPennyLess = false;      // [18. Pennyless] 업적 중복 체크

    [Header("Tutorial Reference")]
    public Tutorial tutorial; // 튜토리얼 스크립트 참조

    public int heliSuccessCount = 0;   // 헬기 후크 성공 횟수

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
        // 게임 오버 상태, 혹은 튜토리얼 미종료 시 점수 갱신 중지
        if (isGameOver || (!tutorial.isTutorialEnd && !UIController.isRestarting)) return;

        // 1. 시간 흐름 측정
        survivalTime += Time.deltaTime;
        
        // 2. 점수 계산: 거리 점수 + 코인 점수
        currentScore = GetDistanceScore() + GetCoinScore();

        // [16. Hustler] 점수가 15000점 이상 달성 시 업적 달성, 중복 체크
        if (!hasAchievedHustler && currentScore >= 15000)
        {
            if (PlayerAchivementList.Instance != null)
            {
                PlayerAchivementList.Instance.Hustler();
            }
            hasAchievedHustler = true; // 달성 완료 표시
        }

        // [18. Pennyless] 코인카운트가 0이면서 7000점을 넘으면 업적 달성, 중복 체크
        if (!hasAchievedPennyLess && currentScore >= 7000 && coinCount == 0)
        {
            if (PlayerAchivementList.Instance != null)
            {
                PlayerAchivementList.Instance.Pennyless();
            }
            hasAchievedPennyLess = true; // 달성 완료 표시
        }

        // 점수가 변경되었을 때만 UI 업데이트 (성능 최적화)
        if (currentScore != lastDisplayedScore)
        {
            UpdateScoreDisplay();
            lastDisplayedScore = currentScore;
        }
    }

    void UpdateScoreDisplay()
    {
        // 현재 점수 표시
        if (inGameScoreText != null)
        {
            inGameScoreText.text = $"SCORE:\t{currentScore}";;
        }
        
        // 현재 코인 수 표시
        if (inGameCoinText != null)
        {
            inGameCoinText.text = $"{coinCount}";
        }
        
        // 베스트 점수 표시
        if (bestScoreText != null)
        {
            if (currentScore < bestScore)
            {
                bestScoreText.text = $"BEST:\t{bestScore - currentScore}";
            }
            else if (currentScore == bestScore)
            {
                bestScoreText.text = "";
            }
            else
            {
                bestScoreText.text = "<color=#FF5A11>NEW RECORD!</color>";
            }
        }
    }


    // 거리 점수만 반환 (시간 * 속도 + 차 날리기 * 가중치)
    public int GetDistanceScore()
    {
        float rawDistanceScore = (survivalTime * currentCarSpeed) + (carKnockCount * CAR_KNOCK_SCORE_VALUE);
        return Mathf.FloorToInt(rawDistanceScore);
    }

    // 코인 점수만 반환 (코인 * 가중치)
    public int GetCoinScore()
    {
        return coinCount * COIN_SCORE_VALUE;
    }

    // 코인 개수 반환
    public int GetCoinCount()
    {
        return coinCount;
    }

    // 현재 점수 return
    public int GetCurrentScore()
    {
        return GetDistanceScore() + GetCoinScore();
    }

    public int GetFinalScore()
    {
        int finalScore = GetCurrentScore();
        
        if (finalScore > bestScore)
        {
            bestScore = finalScore;

            // 기기에 저장 (Key 이름을 "BestScore"로 지정)
            PlayerPrefs.SetInt("BestScore", bestScore);
            IsNewRecord = true;
        }

        return finalScore;
    }

    // 코인 획득량
    // amount: 획득한 코인 수
    public void AddCoin(int amount)
    {
        coinCount += amount;

        // [19. Billionaire] 코인 200개 이상 모을 시 업적 달성
        if (!hasAchievedBillionaire && coinCount >= 200)
        {
            if (PlayerAchivementList.Instance != null)
            {
                PlayerAchivementList.Instance.Billionaire();
            }
            hasAchievedBillionaire = true; // 중복 달성 방지
        }

        // 코인 먹을 때마다 보너스 표시 (중단하지 않음)
        StartCoroutine(ShowBonus($"+{COIN_SCORE_VALUE * amount}"));
    }

    // 무적 상태에서 차 날리기 점수 추가
    public void AddCarKnockScore()
    {
        carKnockCount++;
        StartCoroutine(ShowBonus($"+{CAR_KNOCK_SCORE_VALUE}"));
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
        lastDisplayedScore = -1;
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
            
            // 점수 텍스트의 너비를 계산하여 보너스 텍스트 위치를 오른쪽으로 이동
            if (inGameScoreText != null)
            {
                // TextMeshPro의 textInfo를 사용하여 실제 문자 위치 기반으로 너비 계산
                inGameScoreText.ForceMeshUpdate();
                TMP_TextInfo textInfo = inGameScoreText.textInfo;
                
                float scoreTextWidth = 0f;
                
                // 문자가 있을 때만 계산
                if (textInfo.characterCount > 0)
                {
                    // 마지막 문자의 오른쪽 끝 위치 찾기
                    TMP_CharacterInfo lastChar = textInfo.characterInfo[textInfo.characterCount - 1];
                    scoreTextWidth = lastChar.xAdvance;
                }
                
                // 보너스 텍스트를 점수 텍스트의 오른쪽에 배치 (왼쪽 정렬 기준)
                Vector3 adjustedPos = bonusText.transform.localPosition;
                float baseX = inGameScoreText.transform.localPosition.x;
                adjustedPos.x = baseX + scoreTextWidth + 130f; // 시작점 + 너비 + 여백
                bonusText.transform.localPosition = adjustedPos;
            }
            
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

    // 게임오버 화면에서 점수를 단계적으로 표시하는 코루틴
    // scoreTextUI: 점수를 표시할 TextMeshProUGUI (거리 점수 → 최종 점수)
    // coinCountTextUI: 코인 개수를 표시할 TextMeshProUGUI (선택사항, null이면 코인 개수 미표시)
    // onComplete: 애니메이션 완료 후 실행할 콜백
    public IEnumerator AnimateGameOverScore(TextMeshProUGUI scoreTextUI, TextMeshProUGUI coinCountTextUI = null, System.Action onComplete = null)
    {
        if (scoreTextUI == null) yield break;

        int distanceScore = GetDistanceScore();
        int coinScore = GetCoinScore();
        int totalCoinCount = GetCoinCount();
        int totalScore = GetFinalScore();

        // 1단계: 거리 점수와 코인 개수를 각각 표시
        scoreTextUI.text = distanceScore.ToString();
        
        if (coinCountTextUI != null)
        {
            coinCountTextUI.text = totalCoinCount.ToString();
        }
        
        // 0.5초 대기 (Time.timeScale 영향을 받지 않음)
        yield return new WaitForSecondsRealtime(0.5f);
        
        // 2단계: 코인 개수가 줄어들면서 점수가 증가하는 애니메이션
        float duration = 0.5f;
        float elapsed = 0f;
        
        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime; // Time.timeScale 영향을 받지 않음
            float t = elapsed / duration;
            
            // 거리 점수에서 최종 점수까지 보간
            int currentDisplayScore = Mathf.FloorToInt(Mathf.Lerp(distanceScore, totalScore, t));
            scoreTextUI.text = currentDisplayScore.ToString();
            
            // 코인 개수가 줄어드는 효과 (역방향 보간)
            if (coinCountTextUI != null)
            {
                int currentCoinCount = Mathf.CeilToInt(Mathf.Lerp(0, totalCoinCount, 1f - t));
                coinCountTextUI.text = currentCoinCount.ToString();
            }
            
            yield return null;
        }
        
        // 최종 점수로 정확히 설정
        scoreTextUI.text = totalScore.ToString();
        
        // 코인 개수를 0으로 설정
        if (coinCountTextUI != null)
        {
            coinCountTextUI.text = "0";
        }
        
        // 완료 콜백 실행
        onComplete?.Invoke();
    }
}
