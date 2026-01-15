using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 헬기 아이템 맵 공중 코인 패턴 생성 스크립트
/// </summary>
public class HeliCoinGenerate : MonoBehaviour
{
    public GameObject heliCoinTile; // 코인 타일 배열 부모
    public GameObject[] tiles;  // 기준 바닥 배열
    public GameObject[] heliCoinObstacles;  // 코인 패턴 배열
    
    // 내부 변수
    GameObject player;
    float TileLength;   // 타일 길이

    void Start()
    {
        // 참조 설정
        player = GameObject.FindGameObjectWithTag("Player");

        // 타일 길이 계산
        BoxCollider tileBox = tiles[0].gameObject.GetComponent<BoxCollider>();
        TileLength = tileBox.size.z * tileBox.transform.localScale.z;

        // 코인 패턴 생성
        MakeStartCoin();
    }

    private void OnTriggerEnter(Collider other)
    {
        // Tile Designer에 Obstacle이 닿으면 (Obstacle이 플레이어 지나가면)
        if(other.gameObject.tag == "HeliCoinTile")
        {
            MoveOldTile(other);    // 지나간 타일 제일 뒤로 이동
            MakeCoin(other);        // 뒤로 간 타일 위 새로운 코인 생성
        }
    }

    // 첫 코인 패턴 생성
    private void MakeStartCoin()
    {
        for (int i = 0; i < tiles.Length; i++)
        {
            int nextObstacle = ChooseObstacle(heliCoinObstacles);

            // 오브젝트 풀링 이용해 코인 가져오기
            ObjectPooler.Instance.GetPool(heliCoinObstacles[nextObstacle], tiles[i].transform.position, Quaternion.identity, tiles[i].transform);
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

        // 가장 먼 타일 기준 새로운 타일 배치 (코인 배열은 눈에 보이는 콜라이더가 아니므로 오차 X)
        oldTile.transform.position = new Vector3(oldTile.transform.position.x, oldTile.transform.position.y, 
            maxZ + TileLength);
    }

    //  타일 위에 코인 생성
    private void MakeCoin(Collider oldTile)
    {
        // 오브젝트 풀러 대기 확인
        if (ObjectPooler.Instance == null) return;

        // 타일에 0번째 자식으로 있는 코인 가져오기 (첫 자식은 무조건 코인 프리펩)
        Transform obstacle = oldTile.transform.GetChild(0);

        int nextObstacle = ChooseObstacle(heliCoinObstacles);

        // 기존 코인의 위치, 부모 정보 기억 후 반납
        Vector3 pos = obstacle.position;
        Transform parent = oldTile.transform;

        ObjectPooler.Instance.ReturnPool(obstacle.gameObject);
        obstacle.SetParent(null);

        // 새로운 코인 생성
        ObjectPooler.Instance.GetPool(heliCoinObstacles[nextObstacle], pos, Quaternion.identity, parent);
    }

    // 코인 패턴 랜덤 생성
    private int ChooseObstacle(GameObject[] obstacles)
    {
        return Random.Range(0, obstacles.Length);
    }

    public void StartCoinMap(float speed, float time)
    {
        float distance = speed * time;
        float startZ = player.transform.position.z + distance;

        float halfLength = TileLength / 2f;
        float firstTileZ = startZ + halfLength;

        for(int i = 0; i < tiles.Length; i++)
        {
            float worldZ = firstTileZ + (i * TileLength);

            tiles[i].transform.localPosition = new Vector3(tiles[i].transform.localPosition.x, tiles[i].transform.localPosition.y, worldZ);
        }

        heliCoinTile.SetActive(true);
    }
}
