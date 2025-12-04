using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Tilemaps;

public class CarGenerate : MonoBehaviour
{
    public GameObject[] tiles;  // 장애물 타일 배열
    public GameObject[] easyObstacles;  // 장애물 배열
    public GameObject[] normalObstacles;
    public GameObject[] hardObstacles;
    public GameObject[] tutorialObstacles;  // 튜토리얼용 배열
    public int normalCoin = 10; // 난이도별 코인 요구치
    public int hardCoin = 20;
    public float itemRate = 0.1f;   // 아이템 등장 확률
    float TileLength;   // 타일 길이
    TileGenerate tileGenerate;
    int tileCount = 0;
    public int coin = 0;    // 코인 갯수
    int tutorialIndex = 0;
    public int normalSpeed = 30;
    public int hardSpeed = 40;
    void Start()
    {
        tileGenerate = GameObject.FindGameObjectWithTag("TileGenerator").GetComponent<TileGenerate>();

        // 타일 길이 계산
        BoxCollider tileBox = tiles[0].gameObject.GetComponent<BoxCollider>();
        TileLength = tileBox.size.z * tileBox.transform.localScale.z;

        lastObstacle = -1;

        MakeStartCar();
    }

    void Update() 
    {
        coin = ScoreManager.Instance.coinCount;
    }

    private void MakeStartCar()
    {
        for (int i = 2; i < tiles.Length; i++)
        {
            GameObject[] obstacles = GetDifficultyArray();
            int nextObstacle;
            if(!UIController.tutorialSkip)
            {
                if(tutorialIndex < tutorialObstacles.Length)
                {
                    nextObstacle = tutorialIndex;
                    tutorialIndex++;
                }
                else
                {
                    UIController.tutorialSkip = true;
                    // 튜토리얼 장애물이 부족하면 일반 장애물 사용
                    obstacles = GetDifficultyArray();
                    nextObstacle = Random.Range(0,obstacles.Length - 1);
                }
            }
            else
            {
                do
                {
                    nextObstacle = Random.Range(0, obstacles.Length - 1);
                }
                while(nextObstacle == lastObstacle);
            }
            lastObstacle = nextObstacle;
            Instantiate(obstacles[nextObstacle], tiles[i].transform.position, Quaternion.identity, tiles[i].transform);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        // Tile Designer에 Obstacle이 닿으면 (Obstacle이 플레이어 지나가면)
        if(other.gameObject.tag == "Obstacle")
        {
            MoveOldTile(other);
            MakeCar(other);
            tileCount++;
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
            maxZ + TileLength);
    }

    //  장애물 랜덤으로 타일에 생성
    int lastObstacle;
    private void MakeCar(Collider oldTile)
    {
        Transform obstacle = oldTile.transform.GetChild(0);

        GameObject[] obstacles = GetDifficultyArray();

        // obstacle 랜덤 생성
        int nextObstacle;

        if(!UIController.tutorialSkip)
        {
            if(tutorialIndex < tutorialObstacles.Length)
            {
                nextObstacle = tutorialIndex;
                tutorialIndex++;
            }
            else
            {
                UIController.tutorialSkip = true;
                // 튜토리얼 장애물이 부족하면 일반 장애물 사용
                obstacles = GetDifficultyArray();
                nextObstacle = Random.Range(0, obstacles.Length - 1);
            }
        }
        else
        {
            bool itemTile = (tileCount >= 10) && (Random.value < itemRate);
            if(itemTile == true)
            {
                nextObstacle = obstacles.Length - 1;
                tileCount = 0;
            }
            else
            {
                do
                {
                    nextObstacle = Random.Range(0, obstacles.Length - 1);
                }
                while(nextObstacle == lastObstacle);
            }
        }

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

    private GameObject[] GetDifficultyArray()
    {
        if(!UIController.tutorialSkip) return tutorialObstacles;
        if(coin >= hardCoin) {
            tileGenerate.carSpeed = hardSpeed;
            return hardObstacles;
        }
        else if(coin >= normalCoin) {
            tileGenerate.carSpeed = normalSpeed;
            return normalObstacles;
        }
        else return easyObstacles;
    }
}
