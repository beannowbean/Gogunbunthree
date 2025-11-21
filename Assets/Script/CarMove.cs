using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CarMove : MonoBehaviour
{
    private TileGenerate tileGenerate;
    
    void Start()
    {
        // 타일 오브젝트 생성 시 TileGenerate 스크립트 가져오기 (carSpeed 사용)
        tileGenerate = GameObject.FindGameObjectWithTag("TileGenerator").GetComponent<TileGenerate>();   
    }

    void Update()
    {
        transform.position += new Vector3(0, 0, -tileGenerate.carSpeed * Time.deltaTime);
    }
}
