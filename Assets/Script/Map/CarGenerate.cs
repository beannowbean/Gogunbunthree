using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Tilemaps;

public class CarGenerate : MonoBehaviour    // 장애물 타일 생성 스크립트
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
    public int normalSpeed = 30;    // 노멀 모드 차 속도
    public int hardSpeed = 40;  // 하드 모드 차 속도
    void Start()
    {
        tileGenerate = GameObject.FindGameObjectWithTag("TileGenerator").GetComponent<TileGenerate>();

        // 타일 길이 계산
        BoxCollider tileBox = tiles[0].gameObject.GetComponent<BoxCollider>();
        TileLength = tileBox.size.z * tileBox.transform.localScale.z;

        // 마지막 장애물 변수 초기화
        lastObstacle = -1;

        MakeStartCar();
    }

    void Update() 
    {
        coin = ScoreManager.Instance.coinCount;
    }

    // 게임 시작시 초기 장애물
    private void MakeStartCar()
    {
        for (int i = 2; i < tiles.Length; i++)
        {
            GameObject[] obstacles = GetDifficultyArray();
            int nextObstacle;
            // 튜토리얼시 튜토리얼 장애물
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
        if (ObjectPooler.Instance == null)
        {
            Debug.LogWarning("ObjectPooler 생성 대기");
            return;
        }
        Transform obstacle = oldTile.transform.GetChild(0);

        GameObject[] obstacles = GetDifficultyArray();

        // obstacle 랜덤 생성
        int nextObstacle;

        if(!UIController.tutorialSkip)
        {
            // 튜토리얼시 튜토리얼 장애물
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
            // 일정 확률로 star 아이템 장애물 배열 사용 (배열의 마지막 인덱스 장애물은 무조건 star 사용한 장애물)
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
        ObjectPooler.Instance.ReturnPool(obstacle.gameObject);
        obstacle.SetParent(null);
        // Destroy(obstacle.gameObject);
        lastObstacle = nextObstacle;

        // 새로운 obstacle 생성
        // Instantiate(obstacles[nextObstacle], pos, Quaternion.identity, parent);
        ObjectPooler.Instance.GetPool(obstacles[nextObstacle], pos, Quaternion.identity, parent);
    }

    // 난이도별 장애물 반환
    private GameObject[] GetDifficultyArray()
    {
        if(!UIController.tutorialSkip) return tutorialObstacles;    // 튜토리얼시 튜토리얼 장애물
        if(coin >= hardCoin) {  // 하드 모드 (속도 40, 코인 70 이상 -> CarTileDesigner에서 변경)
            tileGenerate.carSpeed = hardSpeed;
            return hardObstacles;
        }
        else if(coin >= normalCoin) {   // 노멀 모드 (속도 30, 코인 30 이상)
            tileGenerate.carSpeed = normalSpeed;
            return normalObstacles;
        }
        else return easyObstacles;  // 이지 모드 (속도 20)
    }
}
