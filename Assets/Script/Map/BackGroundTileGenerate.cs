using UnityEngine;

/// <summary>
/// 배경 빌딩 생성 스크립트
/// </summary>
public class BackGroundTileGenerate : MonoBehaviour // 플레이어 뒤 박스 콜라이더 이용해 지나간 타일 뒤로 이동, 새로운 빌딩 생성
{
    public GameObject[] tiles;  // 기준 바닥 배열
    public GameObject[] buildings;  // 빌딩 배열

    // 내부 변수
    Player player;  // 플레이어 참조
    float TileLength;   // 타일 길이
    int lastBuilding = -1;  // 마지막 빌딩 인덱스
    GameObject[] currentBuildings = new GameObject[8];  // 현재 빌딩 프리펩 갯수 확인
    
    void Start()
    {
        // 참조 설정
        player = GameObject.FindGameObjectWithTag("Player").GetComponent<Player>();

        // 타일 길이 계산
        BoxCollider tileBox = tiles[0].gameObject.GetComponent<BoxCollider>();
        TileLength = tileBox.size.z * tileBox.transform.localScale.z;

        // 게임 시작 시 보이는 빌딩 생성
        MakeStartBuilding();
    }

    private void OnTriggerEnter(Collider other)
    {
        // 게임 오버 시 작동 중지
        if(player.isGameOver == true) return;

        // Tile Designer에 Tile이 닿으면 (Tile이 플레이어 지나가면)
        if(other.gameObject.tag == "BackgroundTile")
        {
            MoveOldTile(other);     // 지나간 타일 제일 뒤로 이동
            MakeBuilding(other);    // 뒤로 간 타일 위 새로운 빌딩 생성
        }
    }

    // 시작할 때 빌딩 생성
    private void MakeStartBuilding()
    {
        for(int i = 0; i < tiles.Length; i++)
        {
            // 중복 제한따라 빌딩 랜덤 선택
            int nextBuilding = ChooseBuildings(buildings);

            lastBuilding = nextBuilding;
            currentBuildings[i] = buildings[nextBuilding];  // 현재 빌딩 프리펩 갯수 확인 배열에 저장

            // 오브젝트 풀링 이용해 빌딩 가져오기
            ObjectPooler.Instance.GetPool(buildings[nextBuilding], tiles[i].transform.position, Quaternion.identity, tiles[i].transform);
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

        // 가장 먼 타일 기준 새로운 타일 배치 (오차 0.1f 겹치게)
        oldTile.transform.position = new Vector3(oldTile.transform.position.x, oldTile.transform.position.y, 
            maxZ + TileLength - 0.1f);
    }

    //  타일 위에 빌딩 생성
    private void MakeBuilding(Collider oldTile)
    {   
        // 오브젝트 풀러 대기 확인
        if (ObjectPooler.Instance == null) return;

        // 들어온 타일 인덱스 확인 (currentBuildings 수정용)
        int tileIndex = -1;
        for (int i = 0; i < tiles.Length; i++)
        {
            if (tiles[i] == oldTile.gameObject)
            {
                tileIndex = i;
                break;
            }
        }

        // 타일에 0번째 자식으로 있는 빌딩 가져오기 (첫 자식은 무조건 빌딩 프리펩)
        Transform obstacle = oldTile.transform.GetChild(0);

        // 중복 규칙 따라 빌딩 랜덤 생성
        int nextBuilding;   // 다음 생성할 빌딩 인덱스
        nextBuilding = ChooseBuildings(buildings);

        // 기존 빌딩의 위치, 부모 정보 기억 후 반납
        Vector3 pos = obstacle.position;
        Transform parent = oldTile.transform;

        ObjectPooler.Instance.ReturnPool(obstacle.gameObject);
        obstacle.SetParent(null);

        // 현재 빌딩 프리펩 갯수 정보 업데이트
        lastBuilding = nextBuilding;
        currentBuildings[tileIndex] = buildings[nextBuilding];
        
        // 새로운 빌딩 생성
        ObjectPooler.Instance.GetPool(buildings[nextBuilding], pos, Quaternion.identity, parent);
    }

    // 현재 생성된 빌딩 프리펩 갯수 세기
    private int CountBuildings(GameObject prefab)
    {
        int count = 0;
        for (int i = 0; i < 8; i++)
        {
            if (currentBuildings[i] == prefab) count++;
        }
        return count;
    }

    // 빌딩 랜덤 생성 (최대 2개)
    private int ChooseBuildings(GameObject[] buildings)
    {
        int nextBuilding;
        do
        {
            nextBuilding = Random.Range(0, buildings.Length);
        } while (nextBuilding == lastBuilding || CountBuildings(buildings[nextBuilding]) >= 2);
        return nextBuilding;
    }
}
