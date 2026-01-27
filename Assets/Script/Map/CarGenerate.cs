using UnityEngine;

/// <summary>
/// 차 타일 생성 스크립트
/// </summary>
public class CarGenerate : MonoBehaviour    // 플레이어 뒤 박스 콜라이더 이용해 지나간 타일 뒤로 이동, 새로운 차 생성
{
    public GameObject[] tiles;  // 기준 바닥 배열

    [Header("장애물 배열")]
    public GameObject[] level_1Obstacles;  // 레벨1 배열
    public GameObject[] level_2Obstacles; // 레벨2 배열
    public GameObject[] level_3Obstacles; // 레벨3 배열
    public GameObject[] level_4Obstacles; // 레벨4 배열
    public GameObject[] level_5Obstacles; // 레벨5 배열
    public GameObject[] tutorialObstacles;  // 튜토리얼 배열

    [Header("아이템 배열")]
    public GameObject[] starObstacles;  // 별 아이템 배열
    public GameObject[] heliObstacles;  // 헬기 아이템 배열
    public GameObject[] magnetObstacles; // 자석 아이템 배열
    public float itemRate = 0.3f;   // 아이템 등장 확률
    public float itemCooltime = 30f; // 아이템 등장 쿨타임
    public float starHeliRate = 0.7f; // 별 확률 (나머지는 헬기)
    public float magnetRate = 0.3f; // 자석 확률
    public float magnetCooltime = 45f; // 자석 쿨타임
    float itemTimer; // 아이템 타이머
    float magnetTimer; // 자석 타이머
    
    [Header("난이도 설정")]
    public int score = 0;   // 점수
    public int level_2Score = 5000; // 레벨2 점수
    public int level_3Score = 10000; // 레벨3 점수
    public int level_4Score = 15000; // 레벨4 점수
    public int level_5Score = 20000; // 레벨5 점수
    public int level_1Speed = 20;   // 레벨1 차 속도
    public int level_2Speed = 25;   // 레벨2 차 속도
    public int level_3Speed = 30;   // 레벨3 차 속도
    public int level_4Speed = 35;   // 레벨4 차 속도
    public int level_5Speed = 40;   // 레벨 5 차 속도

    // 난이도 설정 변수
    enum Difficulty{Level1, Level2, Level3, Level4, Level5}
    Difficulty currentDifficulty = Difficulty.Level1;   // 현재 난이도 (초기 레벨1)
    
    // 내부 변수
    TileGenerate tileGenerate;  // 타일 생성 스크립트 참조
    DayNightCycle dayNightCycle;    // 밤낮 전환 스크립트 참조
    float TileLength;   // 타일 길이
    int tutorialIndex = 0;  // 튜토리얼 확인용 인덱스
    int lastObstacle = -1;  // 마지막 장애물 인덱스
    GameObject[] currentObstacles = new GameObject[8];  // 현재 차 프리펩 갯수 확인
    bool isLastHardObstacle = false;    // 마지막 장애물 어려운지 확인
    bool isLastCoin = false;    // 마지막 장애물 코인인지 확인

    void Start()
    {
        // 참조 설정
        tileGenerate = GameObject.FindGameObjectWithTag("TileGenerator").GetComponent<TileGenerate>();
        dayNightCycle = GameObject.FindGameObjectWithTag("Light").GetComponent<DayNightCycle>();

        // 타일 길이 계산
        BoxCollider tileBox = tiles[0].gameObject.GetComponent<BoxCollider>();
        TileLength = tileBox.size.z * tileBox.transform.localScale.z;

        // 아이템 타이머 초기화
        itemTimer = Time.time;
        magnetTimer = Time.time;

        // 게임 시작 시 보이는 차 생성
        MakeStartCar();
    }

    void Update() 
    {
        // 점수 가져와 난이도 변경 감지
        score = ScoreManager.Instance.GetCurrentScore();
        DifficultyDetection();
    }
    
    private void OnTriggerEnter(Collider other)
    {
        // Tile Designer에 Obstacle이 닿으면 (Obstacle이 플레이어 지나가면)
        if(other.gameObject.tag == "Obstacle")
        {
            MoveOldTile(other);    // 지나간 타일 제일 뒤로 이동
            MakeCar(other);        // 뒤로 간 타일 위 새로운 차 생성
        }
    }

    // 시작할 때 차 생성
    private void MakeStartCar()
    {
        for (int i = 2; i < tiles.Length; i++)  // 처음 두 타일은 차 없음
        {
            // 난이도별 차 배열 가져오기
            GameObject[] obstacles = GetDifficultyArray();
            int nextObstacle;

            // 튜토리얼시 튜토리얼 차
            if(!UIController.tutorialSkip)
            {
                // 튜토리얼이 끝나지 않았으면 튜토리얼 차 사용
                if(tutorialIndex < tutorialObstacles.Length)
                {
                    nextObstacle = tutorialIndex;
                    tutorialIndex++;
                }
                // 튜토리얼이 끝났는데 차가 부족하면 일반 차 사용
                else
                {
                    obstacles = level_1Obstacles;
                    nextObstacle = ChooseObstacle(obstacles, true);
                }
            }
            // 일반 차 생성
            else
            {
                nextObstacle = ChooseObstacle(obstacles, true);
            }

            lastObstacle = nextObstacle;
            currentObstacles[i] = obstacles[nextObstacle]; // 현재 차 프리펩 갯수 확인 배열에 저장

            // 오브젝트 풀링 이용해 차 가져오기
            ObjectPooler.Instance.GetPool(obstacles[nextObstacle], tiles[i].transform.position, Quaternion.identity, tiles[i].transform);
        }
    }

    // 지나간 타일 제일 멀리 이동
    private void MoveOldTile(Collider oldTile)
    {
        // 배치 된 타일 중 가장 먼 타일 탐색
        float maxZ = -10000;
        for(int i = 0; i < tiles.Length; i++)
        {
            if(maxZ < tiles[i].transform.position.z)
            {
                maxZ = tiles[i].transform.position.z;
            }
        }

        // 가장 먼 타일 기준 새로운 타일 배치 (차 배열은 눈에 보이는 콜라이더가 아니므로 오차 X)
        oldTile.transform.position = new Vector3(oldTile.transform.position.x, oldTile.transform.position.y, 
            maxZ + TileLength);
    }

    //  타일 위에 차 생성
    private void MakeCar(Collider oldTile)
    {
        // 오브젝트 풀러 대기 확인
        if (ObjectPooler.Instance == null) return;

        // 들어온 타일 인덱스 확인 (currentObstacles 수정용)
        int tileIndex = -1;
        for (int i = 0; i < tiles.Length; i++)
        {
            if (tiles[i] == oldTile.gameObject)
            {
                tileIndex = i;
                break;
            }
        }

        // 타일에 0번째 자식으로 있는 차 가져오기 (첫 자식은 무조건 차 프리펩)
        Transform obstacle = oldTile.transform.GetChild(0);

        GameObject[] obstacles; // 난이도별 배열 저장용

        int nextObstacle;   // 다음 생성할 장애물 인덱스

        // 튜토리얼시 튜토리얼 차
        if(!UIController.tutorialSkip)
        {
            // 튜토리얼이 끝나지 않았으면 튜토리얼 차 사용
            obstacles = tutorialObstacles;
            if(tutorialIndex < tutorialObstacles.Length)
            {
                nextObstacle = tutorialIndex;
                tutorialIndex++;
            }
            // 튜토리얼이 끝났는데 차가 부족하면 일반 차 사용
            else
            {
                obstacles = level_1Obstacles;
                nextObstacle = ChooseObstacle(obstacles, true);
            }
        }
        // 일반 차 생성
        else
        {
            // 일정 확률로 별/헬기 아이템 배열 사용
            bool itemTile = (Time.time >= itemTimer + itemCooltime) && (Random.value < itemRate);   // itemCooltime 지나고, itemRate 확률 시
            bool magnetTile = (Time.time >= magnetTimer + magnetCooltime) && (Random.value < magnetRate); // magnetCooltime 지나고, magnetRate 확률 시
            if(itemTile == true)
            {
                // 아이템 배열 중 별 or 헬기 선택
                obstacles = (Random.value < starHeliRate) ? starObstacles : heliObstacles;

                // 위에서 정해진 별 or 헬기 배열에서 하나 선택
                nextObstacle = ChooseObstacle(obstacles, false);

                // itemTimer 초기화
                itemTimer = Time.time;
            }
            else if(magnetTile == true)
            {
                // 자석 배열 사용
                obstacles = magnetObstacles;

                // 자석 배열에서 하나 선택
                nextObstacle = ChooseObstacle(obstacles, false);

                // magnetTimer 초기화
                magnetTimer = Time.time;
            }
            // 아이템 아닐 시 일반 차 생성
            else
            {
                obstacles = GetDifficultyArray();
                nextObstacle = ChooseObstacle(obstacles, true);
            }
        }

        // 기존 차의 위치, 부모 정보 기억 후 반납
        Vector3 pos = obstacle.position;
        Transform parent = oldTile.transform;

        ObjectPooler.Instance.ReturnPool(obstacle.gameObject);
        obstacle.SetParent(null);

        // 현재 차 프리펩 갯수 정보 업데이트
        lastObstacle = nextObstacle;
        currentObstacles[tileIndex] = obstacles[nextObstacle];

        // 새로운 차 생성
        ObjectPooler.Instance.GetPool(obstacles[nextObstacle], pos, Quaternion.identity, parent);
    }

    // 현재 생성된 차 프리펩 갯수 세기
    private int CountObstacles(GameObject prefab)
    {
        int count = 0;
        for(int i = 0; i < 8; i++)
        {
            if(currentObstacles[i] == prefab) count++;
        }
        return count;
    }

    // 차 랜덤 생성 (최대 2개)
    private int ChooseObstacle(GameObject[] obstacles, bool isObstacle) // isObstacle은 아이템인지 장애물인지 확인용
    {
        int nextObstacle;
        int hardDifficulty = Mathf.Max(0, obstacles.Length - 5);    // 어려운 난이도 갯수
        bool isCurrentCoin; // 현재 코인 맵인지
        int safety = 0; // 무한 루프 방지용
        do
        {
            isCurrentCoin = false;

            nextObstacle = Random.Range(0, obstacles.Length);
            safety++;
            if(safety > 100)
            {
                Debug.LogWarning("무한 루프 방지 작동");
                break;
            }
            if(isObstacle == false) // 아이템 맵일 경우 코인 맵
            {
                isCurrentCoin = true;
            }
            else    // 장애물 맵일 경우 코인 맵인지 확인
            {
                if(obstacles[nextObstacle].GetComponent<IsCoin>() != null)
                {
                    isCurrentCoin = true;
                }
            }

            if(safety < 50)
            {
                if(nextObstacle == lastObstacle    // 다음 장애물과 중복 X
                || CountObstacles(obstacles[nextObstacle]) >= 2 // 전체 맵에서 중복 맵 2개 이하
                || isLastHardObstacle && isObstacle && nextObstacle >= hardDifficulty   // 어려운 4개의 난이도 맵 중복 X
                || isLastCoin && isCurrentCoin)    // 코인 맵 중복 X
                continue;
            }
            else
            {
                if(isLastHardObstacle && isObstacle && nextObstacle >= hardDifficulty   // 어려운 4개의 난이도 맵 중복 X
                || isLastCoin && isCurrentCoin) // 코인 맵 중복 X
                continue;
            }
            break;
        } while(true);

        if(isObstacle == true)  // 장애물 배열일 경우 상태 업데이트
        {
            isLastHardObstacle = nextObstacle >= hardDifficulty;    // 어려운 난이도 확인

            if(obstacles[nextObstacle].GetComponent<IsCoin>() != null)  // 코인 장애물 확인
            {
                isLastCoin = true;
            }
            else
            {
                isLastCoin = false;
            }
        }
        else    // 아이템 배열일 경우 자동 true
        {
            isLastHardObstacle = true;
            isLastCoin = true;
        }

        return nextObstacle;
    }

    // 난이도별 장애물 반환
    private GameObject[] GetDifficultyArray()
    {
        if(!UIController.tutorialSkip) return tutorialObstacles;    // 튜토리얼시 튜토리얼 장애물
        if(score >= level_5Score) {  // 레벨5 (속도 40 -> CarTileDesigner에서 변경)
            tileGenerate.carSpeed = level_5Speed;
            return level_5Obstacles;
        }
        else if(score >= level_4Score) {  // 레벨4 (속도 35)
            tileGenerate.carSpeed = level_4Speed;
            return level_4Obstacles;
        }
        else if(score >= level_3Score) {  // 레벨3 (속도 30)
            tileGenerate.carSpeed = level_3Speed;
            return level_3Obstacles;
        }
        else if(score >= level_2Score) {   // 레벨2 (속도 25)
            tileGenerate.carSpeed = level_2Speed;
            return level_2Obstacles;
        }
        else {  // 레벨1 (속도 20)
            tileGenerate.carSpeed = level_1Speed;
            return level_1Obstacles;  
        }
    }

    // 난이도 변경 감지
    private void DifficultyDetection()
    {
        if(!UIController.tutorialSkip) return;
        if(score >= level_5Score) {  // 레벨5 (속도 40 -> CarTileDesigner에서 변경)
            if(currentDifficulty != Difficulty.Level5)
            {
                currentDifficulty = Difficulty.Level5;
                dayNightCycle.NightToDay();
            }
        }
        else if(score >= level_4Score) {   // 레벨4 (속도 30)
            if(currentDifficulty != Difficulty.Level4)
            {
                currentDifficulty = Difficulty.Level4;
                dayNightCycle.DayToNight();
            }
        }
        else if(score >= level_3Score) {   // 레벨3 (속도 25)
            if(currentDifficulty != Difficulty.Level3)
            {
                currentDifficulty = Difficulty.Level3;
                dayNightCycle.NightToDay();
            }
        }
        else if(score >= level_2Score) {   // 레벨2 (속도 20)
            if(currentDifficulty != Difficulty.Level2)
            {
                currentDifficulty = Difficulty.Level2;
                dayNightCycle.DayToNight();
            }
        }
    }
}
