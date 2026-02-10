using UnityEngine;
using LootLocker.Requests;
using System.Collections;
using System.Linq;  // RankData 변환용 {.ToArray()}

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
    public bool debugOfflineMode = false; // 디버그용 오프라인 모드 토글
    public string currentNickname   // 현재 닉네임
    {
        get => PlayerPrefs.GetString("LocalPlayerName", "Guest"); // 없으면 Guest 반환
        private set => PlayerPrefs.SetString("LocalPlayerName", value);
    }

    [Header("리더보드 Keys")]
    public string globalRankingKey = "global_ranking"; // 메인 랭킹용
    public string allPlayersKey = "all_players";    // 전체 유저 수 계산용

    // 내부 변수
    string currentLocalPlayerId = "";   // 현재 로컬 플레이어 ID
    bool isRequesting = false;          // 중복 요청 방지용
    const int MAX_SCORE_LIMIT = 1000000; // 최대 점수 제한
    const string PENDING_SCORE_KEY = "PendingBestScore"; // 대기 중인 점수 저장용 키
    const string PENDING_ACHIEVEMENTS_KEY = "PendingAchievements"; // 대기 중인 업적 저장용 키

    private void Awake()
    {
        LootLocker.LootLockerConfig.current.apiKey = "dev_69d7a89d4b1441cb9e8499b8f20adc3a";    // 임시용 삭제 예정
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
                IsLoggedIn = true;

                // 닉네임이 없으면 랜덤 생성 (최초 1회)
                if (string.IsNullOrEmpty(response.player_name))
                {
                    SetInitialNickname();
                }
                else
                {
                    currentNickname = response.player_name;
                }   

                SyncPendingData(); // 대기 중인 데이터 동기화 시도
                SubmitScoreToKey(allPlayersKey, 1); // 전체 플레이어 카운트용 리더보드에 1점 전송 (통계용)
                AchievementManager.Instance.InitAchievementCache(() => {
                    connected = true;
                });
            }
            else
            {
                Debug.LogError("로그인 실패: " + response.errorData.message);
                IsLoggedIn = false;
                connected = true;
            }
        });
        yield return new WaitUntil(() => connected);
        StartCoroutine(NetworkMonitorRoutine());
    }

    // 최초 닉네임 설정 함수
    private void SetInitialNickname()
    {
        string suffix = currentLocalPlayerId.Length > 3 ? currentLocalPlayerId.Substring(currentLocalPlayerId.Length - 3) : currentLocalPlayerId;
        string randomName = "GogunUser_" + suffix + Random.Range(10, 99);
        ChangeNickname(randomName, (success, error) => {
            if (success)
            {
                currentNickname = randomName;
            }
            else{SetInitialNickname();} // 실패 시 재귀 호출
        });
    }

    // 네트워크 상태 모니터링 코루틴
    private IEnumerator NetworkMonitorRoutine()
    {
        while (true)
        {
            // 이미 로그인된 상태라면 대기
            if (IsLoggedIn)
            {
                yield return new WaitForSeconds(5f);
                continue;
            }

            if (Application.internetReachability != NetworkReachability.NotReachable || debugOfflineMode)
            {
                yield return StartCoroutine(LoginRoutine());
            }

            yield return new WaitForSeconds(5f);
        }
    }

    // 점수 전송 함수 (메인 랭킹용)
    public void SubmitBestScore(int score)
    {
        if (score > MAX_SCORE_LIMIT)
        {
            Debug.LogError($"점수 전송 실패: 점수 {score}가 허용 범위를 벗어났습니다.");
            return;
        }

        // 오프라인 상태이거나 로그인되지 않은 경우 점수를 로컬에 저장
        if (!IsLoggedIn || Application.internetReachability == NetworkReachability.NotReachable || debugOfflineMode)
        {
            int currentBestScore = PlayerPrefs.GetInt("BestScore", 0);
            int currentPendingScore = PlayerPrefs.GetInt(PENDING_SCORE_KEY, 0);
            if (score > currentPendingScore && score > currentBestScore)
            {
                PlayerPrefs.SetInt(PENDING_SCORE_KEY, score);
            }
            return;
        }
        SubmitScoreToKey(globalRankingKey, score, (success) =>
        {
            if (!success) PlayerPrefs.SetInt(PENDING_SCORE_KEY, score); // 실패 시 로컬에 저장
        });
    }

    // 업적 달성 전송 함수 (업적 리더보드에 1점 전송)
    public void CompleteAchievement(string achKey)
    {
        if(!IsLoggedIn || Application.internetReachability == NetworkReachability.NotReachable || debugOfflineMode)
        {
            string currentPendingAchs = PlayerPrefs.GetString(PENDING_ACHIEVEMENTS_KEY, "");
            if (!currentPendingAchs.Contains(achKey))   // 중복 저장 방지
            {
                string updated = string.IsNullOrEmpty(currentPendingAchs) ? achKey : currentPendingAchs + "," + achKey;
                PlayerPrefs.SetString(PENDING_ACHIEVEMENTS_KEY, updated);
            }
            return;
        }
        SubmitScoreToKey(achKey, 1);
    }

    // 키를 사용한 점수 전송 함수
    private void SubmitScoreToKey(string key, int score, System.Action<bool> onComplete = null)
    {
        if (!IsLoggedIn) {
            StartCoroutine(LoginRoutine());
            onComplete?.Invoke(false);
            return;
        }

        LootLockerSDKManager.SubmitScore("", score, key, (response) =>
        {
            if (response.success)
            {
                onComplete?.Invoke(true);
            }
            else
            {
                Debug.LogError($"[{key}] 점수 업로드 실패: " + response.errorData.message);
                onComplete?.Invoke(false);
            }
        });
    }

    // 대기 중인 점수 및 업적 동기화 함수
    private void SyncPendingData()
    {
        if (debugOfflineMode) return;

        // 대기 중인 점수 동기화
        int pendingScore = PlayerPrefs.GetInt(PENDING_SCORE_KEY, 0);
        if (pendingScore > 0)
        {
            SubmitScoreToKey(globalRankingKey, pendingScore, (success) =>
            {
                if (success)
                {
                    PlayerPrefs.DeleteKey(PENDING_SCORE_KEY);
                }
            });
        }

        // 대기 중인 업적 동기화
        string pendingAchs = PlayerPrefs.GetString(PENDING_ACHIEVEMENTS_KEY, "");
        if (!string.IsNullOrEmpty(pendingAchs))
        {
            PlayerPrefs.DeleteKey(PENDING_ACHIEVEMENTS_KEY);

            string[] achKeys = pendingAchs.Split(',');
            foreach (string achKey in achKeys)
            {
                if(string.IsNullOrEmpty(achKey)) continue;

                SubmitScoreToKey(achKey, 1, (success) =>{
                    if(!success)
                    {
                        // 실패 시 다시 로컬에 저장
                        string currentPendingAchs = PlayerPrefs.GetString(PENDING_ACHIEVEMENTS_KEY, "");
                        string updated = string.IsNullOrEmpty(currentPendingAchs) ? achKey : currentPendingAchs + "," + achKey;
                        PlayerPrefs.SetString(PENDING_ACHIEVEMENTS_KEY, updated);
                    }
                });
            }
        }
    }

    // 닉네임 가져오기 함수
    public void GetNickname(System.Action<string> onGet)
    {
        LootLockerSDKManager.GetPlayerName((response) =>
        {
            if (response.success) onGet?.Invoke(response.name);
        });
    }

    // 닉네임 변경 함수
    public void ChangeNickname(string newName, System.Action<bool, string> onComplete)
    {
        if(isRequesting) return;    // 중복 요청 방지
        isRequesting = true;

        LootLockerSDKManager.SetPlayerName(newName, (response) =>
        {
            isRequesting = false;
            if (response.success)
            {
                currentNickname = newName;
                onComplete?.Invoke(true, "");
            }
            else
            {
                // 중복된 이름인 경우 메시지 처리
                string errorReason = response.errorData.message;
                if (errorReason.Contains("unique")) errorReason = "This nickname is already taken";
                onComplete?.Invoke(false, errorReason);
            }
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
        if(isRequesting) return;    // 중복 요청 방지
        isRequesting = true;

        // globalRankingKey에서 상위 20명(0번 index부터 20개) 가져옴
        LootLockerSDKManager.GetScoreList(globalRankingKey, 20, 0, (response) =>
        {
            isRequesting = false;
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
        if(isRequesting) return;    // 중복 요청 방지
        isRequesting = true;

        // 0은 본인 기준 위아래 10명을 자동으로 가져옴 (서버 기본 값)
        LootLockerSDKManager.GetMemberRank(globalRankingKey, currentLocalPlayerId, (response) =>
        {
            isRequesting = false;
            if (response.success)
            {
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