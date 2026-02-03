using UnityEngine;

public class RankTester : MonoBehaviour
{
    void Update()
    {
        // [1번 키] 최고 점수 전송 테스트
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            int randomScore = Random.Range(10, 1000);
            Debug.Log($"[테스트] 점수 전송 시도: {randomScore}");
            RankManager.Instance.SubmitBestScore(randomScore);
        }

        // [2번 키] 닉네임 변경 테스트
        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            string newName = "User_" + Random.Range(1, 100);
            Debug.Log($"[테스트] 닉네임 변경 시도: {newName}");
            RankManager.Instance.ChangeNickname(newName);
        }

        // [3번 키] 업적 달성 보고 테스트 (ACH_01)
        if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            Debug.Log("[테스트] ACH_01 업적 달성 전송");
            RankManager.Instance.CompleteAchievement("ach_01");
        }

        // [4번 키] 업적 달성률 계산 테스트
        if (Input.GetKeyDown(KeyCode.Alpha4))
        {
            Debug.Log("[테스트] ACH_01 달성률 계산 요청...");
            RankManager.Instance.GetAchievementRate("ach_01", (rate) => {
                Debug.Log($"[결과] ACH_01을 깬 유저는 전체의 {rate:F1}% 입니다.");
            });
        }

        // [5번 키] 상위 20명 데이터 출력 테스트
        if (Input.GetKeyDown(KeyCode.Alpha5))
        {
            Debug.Log("[테스트] 상위 20명 로드 중...");
            RankManager.Instance.GetTopRanking((data) => {
                foreach (var r in data)
                {
                    Debug.Log($"[{r.rank}위] {r.name} : {r.score}점");
                }
            });
        }

        // [6번 키] 내 주변 랭킹 출력 테스트
        if (Input.GetKeyDown(KeyCode.Alpha6))
        {
            Debug.Log("[테스트] 내 주변 랭킹 로드 중...");
            RankManager.Instance.GetMyAroundRanking((data) => {
                foreach (var r in data)
                {
                    string deco = (r.name.Contains("GogunUser")) ? "<-- 나" : ""; // 내가 포함되었는지 확인
                    Debug.Log($"[{r.rank}위] {r.name} : {r.score}점 {deco}");
                }
            });
        }        

        // [0번 키] 가상 유저 20명 생성
        // if (Input.GetKeyDown(KeyCode.Alpha0))
        // {
        //     Debug.Log("[테스트] 가상 유저 20명 생성 시작...");
        //     RankManager.Instance.CreateFakeRankings();
        // }
    }
}