using UnityEngine;

/// <summary>
/// 랭킹 및 업적 시스템 테스트
/// </summary>
public class RankTester : MonoBehaviour
{
    void Update()
    {
        // [1번 키] 최고 점수 전송 테스트
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            int randomScore = Random.Range(10, 1000);
            RankManager.Instance.SubmitBestScore(randomScore);
        }

        // [2번 키] 닉네임 변경 테스트
        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            string newName = "GogunUser_" + Random.Range(1, 100);
            RankManager.Instance.ChangeNickname(newName, (success, error) => {});
        }

        // [3번 키] 업적 달성 보고 테스트 (ACH_01)
        if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            RankManager.Instance.CompleteAchievement("ach_01");
        }

        // [4번 키] 업적 달성률 계산 테스트
        if (Input.GetKeyDown(KeyCode.Alpha4))
        {
            RankManager.Instance.GetAchievementRate("ach_01", (rate) => {
                Debug.Log($"[결과] ACH_01을 깬 유저는 전체의 {rate:F1}% 입니다.");
            });
        }

        // [5번 키] 상위 20명 데이터 출력 테스트
        if (Input.GetKeyDown(KeyCode.Alpha5))
        {
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
            RankManager.Instance.GetMyAroundRanking((data) => {
                foreach (var r in data)
                {
                    string deco = (r.name.Contains("GogunUser")) ? "<-- 나" : ""; // 내가 포함되었는지 확인
                    Debug.Log($"[{r.rank}위] {r.name} : {r.score}점 {deco}");
                }
            });
        }        

        // [7번 키] 업적 달성 등수 테스트
        if (Input.GetKeyDown(KeyCode.Alpha7))
        {
            string testAch = "ach_01";
            
            RankManager.Instance.GetAchievementOrder(testAch, (order) => {
                if (order > 0)
                    Debug.Log($"[결과] 당신은 이 업적을 <b>{order}번째</b>로 달성했습니다!");
                else
                    Debug.Log("[결과] 아직 이 업적을 달성하지 않았습니다.");
            });
        }

        // [8번 키] 오프라인 테스트
        if (Input.GetKeyDown(KeyCode.Alpha8))
        {
            RankManager.Instance.debugOfflineMode = !RankManager.Instance.debugOfflineMode;
            Debug.Log($"오프라인 모드 상태: {RankManager.Instance.debugOfflineMode}");
        }

        // [9번 키] 랭크 씬 테스트
        if (Input.GetKeyDown(KeyCode.Alpha9))
        {
            UnityEngine.SceneManagement.SceneManager.LoadScene("Ranking");
        }

        // [0번 키] 가상 유저 20명 생성
        // if (Input.GetKeyDown(KeyCode.Alpha0))
        // {
        //     RankManager.Instance.CreateFakeRankings();
        // }
    }
}