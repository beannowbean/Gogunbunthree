using UnityEngine;

/// <summary>
/// 바닥 도로 타일 생성 스크립트
/// </summary>
public class TileGenerate : MonoBehaviour
{
    public GameObject[] tiles;  // 기준 바닥 배열
    public float tileSpeed; // 타일 다가오는 속도
    public float carSpeed;  // 차 다가오는 속도

    // 내부 변수
    Player player;  // 플레이어 참조
    float TileLength;   // 타일 길이
    float posY; // 타일 y좌표 기준값

    void Start()
    {
        // 참조 설정
        player = GameObject.FindGameObjectWithTag("Player").GetComponent<Player>();

        // 타일 길이 계산
        BoxCollider tileBox = tiles[0].gameObject.GetComponent<BoxCollider>();
        TileLength = tileBox.size.z * tileBox.transform.localScale.z;

        // y좌표 기준값 설정
        posY = tiles[0].transform.position.y;
    }

    void Update()
    {
        // 게임 오버 시 타일 멈춤
        if(player.isGameOver == true)
        {
            tileSpeed = 0;
        }

        // 타일 속도 업데이트
        if(ScoreManager.Instance != null)
        {
            ScoreManager.Instance.UpdateCarSpeed(carSpeed);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        // 게임 오버 시 타일 이동 멈춤
        if(player.isGameOver == true) return;

        // Tile Designer에 Tile이 닿으면 (Tile이 플레이어 지나가면)
        if(other.gameObject.tag == "Tile")
        {
            MoveOldTile(other);
        }
    }

    // 지나간 타일 제일 멀리 이동
    private void MoveOldTile(Collider oldTile)
    {
        // 가장 먼 타일 탐색
        float maxZ = -10000;
        for(int i = 0; i < tiles.Length; i++)
        {
            if(maxZ < tiles[i].transform.position.z)
            {
                maxZ = tiles[i].transform.position.z;
            }
        }

        float currentY = oldTile.transform.position.y;
        float targetY;

        // y값 살짝 조정 (완전히 같으면 오차 생길 수 있음)
        if(Mathf.Abs(currentY - posY) < 0.0005f)
        {
            targetY = posY + 0.001f;
        }
        else
        {
            targetY = posY;
        }

        // 가장 먼 타일 기준 새로운 타일 배치
        oldTile.transform.position = new Vector3(oldTile.transform.position.x, targetY, 
            maxZ + TileLength - 0.1f);
    }
}
