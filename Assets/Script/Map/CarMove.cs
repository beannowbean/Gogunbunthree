using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

public class CarMove : MonoBehaviour
{
    private TileGenerate tileGenerate;
    private Player player;
    private bool freezeStreetLight = false;
    public float changeTime = 5.0f;
    private float currentCarSpeed;
    
    void Start()
    {
        // 타일 오브젝트 생성 시 TileGenerate 스크립트 가져오기 (carSpeed 사용)
        tileGenerate = GameObject.FindGameObjectWithTag("TileGenerator").GetComponent<TileGenerate>();   

        // 플레이어 스크립트 가져오기 (게임오버 확인용)
        player = GameObject.FindGameObjectWithTag("Player").GetComponent<Player>();

        // 초기 자동차 속도
        currentCarSpeed = tileGenerate.carSpeed;
    }

    void Update()
    {
        // 자동차 타일 이동
        currentCarSpeed = Mathf.Lerp(currentCarSpeed, tileGenerate.carSpeed, Time.deltaTime / changeTime);
        transform.position += new Vector3(0, 0, -currentCarSpeed * Time.deltaTime);

        // 게임오버 시 가로등 고정
        if(player.isGameOver == true && freezeStreetLight == false)
        {
            GameObject[] streetLights = GameObject.FindGameObjectsWithTag("StreetLight");
            foreach(GameObject streetLight in streetLights)
            {
                Vector3 curPos = streetLight.transform.position;
                streetLight.transform.SetParent(null);
                streetLight.transform.position = curPos;
            }
            freezeStreetLight = true;
        }
    }
}
