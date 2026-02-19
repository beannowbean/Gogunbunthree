using UnityEngine;

/// <summary>
/// 바닥 도로 타일 이동 스크립트
/// </summary>
public class TileMove : MonoBehaviour
{
    // 내부 변수
    TileGenerate tileGenerate;
    
    void Start()
    {
        // 참조 설정
        tileGenerate = GameObject.FindGameObjectWithTag("TileGenerator").GetComponent<TileGenerate>(); // tileSpeed 참조용
    }

    void Update()
    {
        // 타일 이동
        transform.position += new Vector3(0, 0, -tileGenerate.tileSpeed * Time.deltaTime);
    }
}
