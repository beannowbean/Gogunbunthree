using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BackGroundTileGenerate : MonoBehaviour
{
    public GameObject[] tiles;  // 바닥 도로 배열
    public GameObject[] buildings;  // 빌딩 배열
    float TileLength;   // 타일 길이
    Player player;
    
    void Start()
    {
        // 타일 길이 계산
        BoxCollider tileBox = tiles[0].gameObject.GetComponent<BoxCollider>();
        TileLength = tileBox.size.z * tileBox.transform.localScale.z;
        player = GameObject.FindGameObjectWithTag("Player").GetComponent<Player>();

        lastBuilding = -1;
    }

    private void OnTriggerEnter(Collider other)
    {
        if(player.isGameOver == true) return;
        // Tile Designer에 Tile이 닿으면 (Tile이 플레이어 지나가면)
        if(other.gameObject.tag == "BackgroundTile")
        {
            MoveOldTile(other);
            MakeBuilding(other);
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

        // 가장 먼 타일 기준 새로운 타일 배치
        oldTile.transform.position = new Vector3(oldTile.transform.position.x, oldTile.transform.position.y, 
            maxZ + TileLength - 1.0f);
    }

    //  빌딩 랜덤으로 타일에 생성
    int lastBuilding;
    private void MakeBuilding(Collider oldTile)
    {
        Transform obstacle = oldTile.transform.GetChild(0);

        // obstacle 랜덤 생성
        int nextBuilding;
        do
        {
            nextBuilding = Random.Range(0, buildings.Length);
        }
        while(nextBuilding == lastBuilding);

        // Destroy전 좌표와 부모 기억
        Vector3 pos = obstacle.position;
        Transform parent = oldTile.transform;

        // 기존 자식 오브젝트 삭제
        obstacle.SetParent(null);
        Destroy(obstacle.gameObject);
        lastBuilding = nextBuilding;
        
        // 새로운 obstacle 생성
        Instantiate(buildings[nextBuilding], pos, Quaternion.identity, parent);
    }
}
