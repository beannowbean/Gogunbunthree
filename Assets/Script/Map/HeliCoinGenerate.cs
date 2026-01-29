using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// 헬기 아이템 맵 공중 코인 패턴 생성 스크립트
/// </summary>
public class HeliCoinGenerate : MonoBehaviour
{
    public GameObject heliCoinTile; // 코인 타일 배열 부모
    public GameObject[] tiles;  // 기준 바닥 배열
    public GameObject[] heliCoinObstacles4;  // 4 코인 패턴 배열
    public GameObject[] heliCoinObstacles3;  // 3 코인 패턴 배열
    
    // 내부 변수
    GameObject player;  // 플레이어 참조
    float TileLength;   // 타일 길이
    float speed;    // 헬기 맵 속도
    float duration; // 헬기 맵 유지 시간
    int tileCount;  // 배치할 총 타일 갯수
    int tileElapsed; // 배치한 총 타일 갯수

    int lastFrameCount = -1;    // 지난 프레임
    HashSet<int>processedObjects = new HashSet<int>();  // 처리한 오브젝트 리스트

    void Awake()
    {
        // 타일 길이 계산
        BoxCollider tileBox = tiles[0].gameObject.GetComponent<BoxCollider>();
        TileLength = tileBox.size.z * tileBox.transform.localScale.z;

        // 시작 시 코인 타일 비활성화
        heliCoinTile.SetActive(false);
    }

    void Update()
    {
        // 참조 설정
        if (player == null)
        {
            player = GameObject.FindGameObjectWithTag("Player");
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        // Tile Designer에 Obstacle이 닿으면 (Obstacle이 플레이어 지나가면)
        if(other.gameObject.tag == "HeliCoinTile")
        {
            // 한 프레임에 같은 타일이 두 번 트리거되는 것을 방지 (무한 루프 차단)
            if (Time.frameCount != lastFrameCount)
            {
                lastFrameCount = Time.frameCount;
                processedObjects.Clear();
            }

            int instanceId = other.gameObject.GetInstanceID();
            if (processedObjects.Contains(instanceId)) return; // 이미 이번 프레임에 옮긴 타일이면 무시
            
            processedObjects.Add(instanceId);

            MoveOldTile(other);    // 지나간 타일 제일 뒤로 이동

            if(tileElapsed < tileCount)
            {
                MakeCoin(other);    // 뒤로 간 타일 위 새로운 코인 생성
                tileElapsed++;
            }
            else
            {
                clearTile(other.gameObject);
            }
        }
    }

    // 첫 코인 패턴 생성
    private void MakeStartCoin()
    {
        tileElapsed = 0;

        for (int i = 0; i < tiles.Length; i++)
        {
            clearTile(tiles[i]);    // 기존 코인 제거

            if(tileElapsed < tileCount)
            {
                GameObject[] obstacles = GetDifficultyArray();
                int nextObstacle = ChooseObstacle(obstacles);

                // 오브젝트 풀링 이용해 코인 가져오기
                ObjectPooler.Instance.GetPool(obstacles[nextObstacle], tiles[i].transform.position, Quaternion.identity, tiles[i].transform);

                tileElapsed++;
            }
        }
    }

    // 지나간 타일 제일 멀리 이동
    private void MoveOldTile(Collider oldTile)
    {
        // 배치 된 타일 중 가장 먼 타일 탐색
        float maxZ = -10000;
        for(int i = 0; i < tiles.Length; i++)
        {
            if(tiles[i] != null && maxZ < tiles[i].transform.position.z)
            {
                maxZ = tiles[i].transform.position.z;
            }
        }

        // 가장 먼 타일 기준 새로운 타일 배치 (코인 배열은 눈에 보이는 콜라이더가 아니므로 오차 X)
        oldTile.transform.position = new Vector3(oldTile.transform.position.x, oldTile.transform.position.y, maxZ + TileLength);
    }

    //  타일 위에 코인 생성
    private void MakeCoin(Collider oldTile)
    {
        // 오브젝트 풀러 대기 확인
        if (ObjectPooler.Instance == null) return;

        clearTile(oldTile.gameObject);

        GameObject[] obstacles = GetDifficultyArray();
        int nextObstacle = ChooseObstacle(obstacles);

        // 새로운 코인 생성
        ObjectPooler.Instance.GetPool(obstacles[nextObstacle], oldTile.transform.position, Quaternion.identity, oldTile.transform);
    }

    // 코인 패턴 랜덤 생성
    private int ChooseObstacle(GameObject[] obstacles)
    {
        return Random.Range(0, obstacles.Length);
    }

    // 난이도별 장애물 반환
    private GameObject[] GetDifficultyArray()
    {
        if(speed > 30) {  // 속도 30 이상 -> 3개 간격 코인
            return heliCoinObstacles3;
        }
        else {  // 속도 30 미만 -> 4개 간격 코인
            return heliCoinObstacles4;  
        }
    }

    // 시작 코인 패턴 위치 조정
    public void StartCoinMap(float mapSpeed, float time, float mapDuration)
    {
        StopAllCoroutines();
        // 헬기 관련 변수 저장
        speed = mapSpeed;
        duration = mapDuration - 1.0f;

        if (TileLength <= 0)
        {
            return;
        }

        // 배치할 타일 갯수 계산
        float fullTileLength = speed * duration;
        tileCount = Mathf.FloorToInt(fullTileLength / TileLength);
        tileElapsed = 0;

        // 배치할 타일 위치 계산
        float distance = speed * time;
        float startZ = player.transform.position.z + distance;
        float halfLength = TileLength / 2f;
        float firstTileZ = startZ + halfLength;

        // 타일 위치 조정
        for(int i = 0; i < tiles.Length; i++)
        {
            float worldZ = firstTileZ + (i * TileLength);
            tiles[i].transform.localPosition = new Vector3(tiles[i].transform.localPosition.x, tiles[i].transform.localPosition.y, worldZ);
        }

        MakeStartCoin();

        // 타일 활성화
        heliCoinTile.SetActive(true);

        StartCoroutine(EndCoin(duration + 4.0f));
    }

    // 타일 초기화
    void clearTile(GameObject tile)
    {
        if(tile == null) return;

        // 타일 자식들 가져와서 풀 반환
        int childCount= tile.transform.childCount;
        for(int i = childCount - 1; i >= 0; i--)
        {
            Transform obstacle = tile.transform.GetChild(0);
            if(obstacle != null)
            {
                obstacle.SetParent(null);
                ObjectPooler.Instance.ReturnPool(obstacle.gameObject);
            }
        }
    }

    // 코인 맵 끄기
    IEnumerator EndCoin(float time)
    {
        yield return new WaitForSeconds(time);
        heliCoinTile.SetActive(false);
    }
}
