using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Tilemaps;

public class TileGenerate : MonoBehaviour   // 바닥 도로 타일 생성 스크립트
{
    public GameObject[] tiles;  // 바닥 도로 배열
    public float tileSpeed; // 타일 다가오는 속도
    float TileLength;   // 타일 길이
    public float carSpeed;  //  차 다가오는 속도
    private Player player;
    private float posY;
    void Start()
    {
        // 타일 길이 계산
        BoxCollider tileBox = tiles[0].gameObject.GetComponent<BoxCollider>();
        TileLength = tileBox.size.z * tileBox.transform.localScale.z;
        player = GameObject.FindGameObjectWithTag("Player").GetComponent<Player>();
        posY = tiles[0].transform.position.y;
    }

    void Update()
    {
        if(player.isGameOver == true)
        {
            tileSpeed = 0;
        }
        if(ScoreManager.Instance != null)
        {
            ScoreManager.Instance.UpdateCarSpeed(carSpeed);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
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

        // y값 살짝 조정
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
