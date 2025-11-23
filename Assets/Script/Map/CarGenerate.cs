using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class CarGenerate : MonoBehaviour
{
    public GameObject[] tiles;  // 장애물 타일 배열
    public GameObject[] obstacles;  // 장애물 배열
    float TileLength;   // 타일 길이
    TileGenerate tileGenerate;
    void Start()
    {
        tileGenerate = GameObject.FindGameObjectWithTag("TileGenerator").GetComponent<TileGenerate>();

        // 타일 길이 계산
        BoxCollider tileBox = tiles[0].gameObject.GetComponent<BoxCollider>();
        TileLength = tileBox.size.z * tileBox.transform.localScale.z;

        lastObstacle = -1;
    }

    private void OnTriggerEnter(Collider other)
    {
        // Tile Designer에 Obstacle이 닿으면 (Obstacle이 플레이어 지나가면)
        if(other.gameObject.tag == "Obstacle")
        {
            moveOldTile(other);
            makeCar(other);
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
            maxZ + TileLength);
    }

    //  장애물 랜덤으로 타일에 생성
    int lastObstacle;
    private void makeCar(Collider oldTile)
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
