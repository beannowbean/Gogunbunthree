using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Tilemaps;

public class TileGenerate : MonoBehaviour
{
    public GameObject[] tiles;  // 바닥 도로 배열
    public GameObject[] obstacles;  // 장애물 배열
    public float tileSpeed; // 타일 다가오는 속도
    float TileLength;   // 타일 길이
    public float carSpeed;  //  차 다가오는 속도
    void Start()
    {
        // 타일 길이 계산
        BoxCollider tileBox = tiles[0].gameObject.GetComponent<BoxCollider>();
        TileLength = tileBox.size.z * tileBox.transform.localScale.z;

        lastObstacle = -1;
    }

    private void OnTriggerEnter(Collider other)
    {
        // Tile Designer에 Tile이 닿으면 (Tile이 플레이어 지나가면)
        if(other.gameObject.tag == "Tile")
        {
            moveOldTile(other);
            // makeObstacle(other);
        }
    }

    // 지나간 타일 제일 멀리 이동
    private void moveOldTile(Collider oldTile)
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
            maxZ + TileLength / 2.0f);
    }

    //  장애물 랜덤으로 타일에 생성
    int lastObstacle;
    private void makeObstacle(Collider oldTile)
    {
        Transform obstacle = oldTile.transform.GetChild(0);

        // obstacle 랜덤 생성
        int nextObstacle;
        do
        {
            nextObstacle = Random.Range(0, obstacles.Length);
        }
        while(nextObstacle == lastObstacle);

        // Destroy전 좌표와 부모 기억
        Vector3 pos = obstacle.position;
        Transform parent = oldTile.transform;

        // 기존 자식 오브젝트 삭제
        obstacle.SetParent(null);
        Destroy(obstacle.gameObject);
        lastObstacle = nextObstacle;

        // 새로운 obstacle 생성
        Instantiate(obstacles[nextObstacle], pos, Quaternion.identity, parent);
    }
}
