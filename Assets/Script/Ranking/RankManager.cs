using UnityEngine;
using LootLocker.Requests;
using System.Collections;
using System.Linq;

// 랭킹 정보를 담을 간단한 구조체 (UI 전달용)
[System.Serializable]
public struct RankData
{
    public int rank;    // 등수
    public string name; // 닉네임
    public int score;   // 점수
}

/// <summary>
/// 랭킹 및 업적 관리를 담당하는 매니저
/// </summary>
public class RankManager : MonoBehaviour
{
    public static RankManager Instance { get; private set; }    // 싱글톤 인스턴스
    public bool IsLoggedIn { get; private set; }    // 로그인 상태 확인용

    [Header("리더보드 Keys")]
    public string globalRankingKey = "global_ranking"; // 메인 랭킹용
    public string allPlayersKey = "all_players";    // 전체 유저 수 계산용

    // 내부 변수
    string currentLocalPlayerId = "";   // 현재 로컬 플레이어 ID

    private void Awake()
    {
        // 싱글톤 패턴 구현
        if (Instance == null)
        {
            Instance = this;
            transform.SetParent(null);
            DontDestroyOnLoad(gameObject);
        }
        else if (Instance != this)
        {
            Destroy(gameObject);
            return;
        }
    }

    private void Start()
    {
        StartCoroutine(LoginRoutine());
    }

    // 게스트 로그인 및 초기 설정
    private IEnumerator LoginRoutine()
    {
        bool connected = false;
        LootLockerSDKManager.StartGuestSession((response) =>
        {
            if (response.success)
            {
                currentLocalPlayerId = response.player_id.ToString();
                Debug.Log($"LootLocker 로그인 성공 ID: {currentLocalPlayerId}");
                IsLoggedIn = true;
                
                // 닉네임이 없으면 랜덤 생성 (최초 1회)
                if (string.IsNullOrEmpty(response.player_name))
                {
                    string randomName = "GogunUser_" + Random.Range(1000, 9999);
                    ChangeNickname(randomName);
                }

                // 전체 플레이어 카운트용 리더보드에 1점 전송 (통계용)
                SubmitScoreToKey(allPlayersKey, 1);
            }
            else
            {
                Debug.LogError("로그인 실패: " + response.errorData.message);
            }
            connected = true;
        });
        yield return new WaitUntil(() => connected);
    }

    // 점수 전송 함수 (메인 랭킹용)
    public void SubmitBestScore(int score)
    {
        SubmitScoreToKey(globalRankingKey, score);
    }

    // 업적 달성 전송 함수 (업적 리더보드에 1점 전송)
    public void CompleteAchievement(string achKey)
    {
        SubmitScoreToKey(achKey, 1);
    }

    // 키를 사용한 점수 전송 함수
    private void SubmitScoreToKey(string key, int score)
    {
        if (!IsLoggedIn) return;

        LootLockerSDKManager.SubmitScore("", score, key, (response) =>
        {
            if (response.success){}
            else Debug.LogError($"[{key}] 점수 업로드 실패: " + response.errorData.message);
        });
    }

    // 닉네임 변경 함수
    public void ChangeNickname(string newName)
    {
        LootLockerSDKManager.SetPlayerName(newName, (response) =>
        {
            if (response.success) Debug.Log("닉네임 변경 완료: " + newName);
            else Debug.LogError("닉네임 변경 실패: " + response.errorData.message);
        });
    }

    // 업적 달성률(%) 계산 함수
    public void GetAchievementRate(string achKey, System.Action<float> onRateCalculated)
    {
        // 전체 플레이어 수 가져오기
        LootLockerSDKManager.GetScoreList(allPlayersKey, 1, 0, (totalRes) =>
        {
            if (totalRes.success)
            {
                int totalCount = totalRes.pagination.total; // 전체 플레이어 수 변수

                // 특정 업적 달성자 수 가져오기
                LootLockerSDKManager.GetScoreList(achKey, 1, 0, (achRes) =>
                {
                    if (achRes.success)
                    {
                        int achCount = achRes.pagination.total; // 업적 달성자 수 변수
                        float rate = (totalCount > 0) ? ((float)achCount / totalCount) * 100f : 0f;
                        onRateCalculated?.Invoke(rate);
                    }
                });
            }
        });
    }

    // 업적 달성 등수 계산 함수
    public void GetAchievementOrder(string achKey, System.Action<int> onOrderFound)
    {
        // 내 ID를 사용하여 해당 업적 리더보드에서의 내 등수를 조회
        LootLockerSDKManager.GetMemberRank(achKey, currentLocalPlayerId, (response) =>
        {
            if (response.success)
            {
                int myOrder = response.rank;
                onOrderFound?.Invoke(myOrder);
            }
            else
            {
                Debug.LogError($"업적 순서 조회 실패: {response.errorData.message}");
            }
        });
    }

    // 상위 랭크 가져오기
    public void GetTopRanking(System.Action<RankData[]> onLoaded)
    {
        // globalRankingKey에서 상위 20명(0번 index부터 20개) 가져옴
        LootLockerSDKManager.GetScoreList(globalRankingKey, 20, 0, (response) =>
        {
            if (response.success)
            {
                // 서버 데이터를 RankData 구조체로 변환
                RankData[] rankResults = response.items.Select(item => new RankData
                {
                    rank = item.rank,
                    name = string.IsNullOrEmpty(item.player.name) ? item.player.id.ToString() : item.player.name,
                    score = item.score
                }).ToArray();

                onLoaded?.Invoke(rankResults);
            }
            else
            {
                Debug.LogError("상위 랭킹 로드 실패: " + response.errorData.message);
            }
        });
    }

    // 내 주변 등수 가져오기 (내 점수 기준 위아래)
    public void GetMyAroundRanking(System.Action<RankData[]> onLoaded)
    {
        // 0은 본인 기준 위아래 10명을 자동으로 가져옴 (서버 기본 값)
        LootLockerSDKManager.GetMemberRank(globalRankingKey, currentLocalPlayerId, (response) =>
        {
            if (response.success)
            {
                // 성공 시 내 등수 출력
                Debug.Log($"서버가 인식하는 내 등수: {response.rank}위");
                int myRank = response.rank;
                int startAfter = Mathf.Max(0, myRank - 6); // 내 등수 5명 위부터 가져옴

                // 내 등수 근처 데이터들을 변환
                LootLockerSDKManager.GetScoreList(globalRankingKey, 11, startAfter, (listResponse) =>
                {
                    if (listResponse.success)
                    {
                        // 서버 데이터를 RankData 구조체로 변환
                        RankData[] rankResults = listResponse.items.Select(item => new RankData
                        {
                            rank = item.rank,
                            name = string.IsNullOrEmpty(item.player.name) ? item.player.id.ToString() : item.player.name,
                            score = item.score
                        }).ToArray();

                        onLoaded?.Invoke(rankResults);
                    }
                });
            }
            else
            {
                Debug.LogError($"내 주변 랭킹 로드 실패 (ID: {currentLocalPlayerId}): " + response.errorData.message);
                GetTopRanking(onLoaded); // 실패 시 상위 랭킹으로 대체
            }
        });
    }

    public void CreateFakeRankings()
    {
        StartCoroutine(FakeRankingRoutine());
    }

    IEnumerator FakeRankingRoutine()
    {
        for (int i = 1; i <= 20; i++)
        {
            // 로컬에 저장된 이전 세션 식별자를 삭제하여 새 유저로 인식하게 함
            PlayerPrefs.DeleteKey("LootLockerGuestPlayerIdentifier");
            
            LootLockerSDKManager.EndSession((endRes) => { });

            // 잠시 대기 (SDK가 내부적으로 정리할 시간)
            yield return new WaitForSeconds(0.2f);

            bool done = false;
            string fakeName = "Tester_" + i;
            int fakeScore = Random.Range(100, 5000);

            // 새 유저로 로그인 시도
            LootLockerSDKManager.StartGuestSession((response) =>
            {
                if (response.success)
                {
                    // 닉네임 설정 및 점수 전송
                    LootLockerSDKManager.SetPlayerName(fakeName, (res) => {
                        RankManager.Instance.SubmitBestScore(fakeScore);
                        Debug.Log($"{fakeName} 생성 및 {fakeScore}점 전송 완료");
                        done = true;
                    });
                }
            });

            yield return new WaitUntil(() => done);
            yield return new WaitForSeconds(0.5f); // 서버 과부하 방지
        }
        Debug.Log("가상 유저 20명 생성 완료");
    }
}