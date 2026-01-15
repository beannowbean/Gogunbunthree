using UnityEngine;

/// <summary>
/// 헬기 아이템 맵 공중 코인 이동 스크립트
/// </summary>
public class HeliCoinMove : MonoBehaviour
{
    // 내부 변수
    TileGenerate tileGenerate;  // 타일 생성 스크립트 참조
    float coinSpeed;    // 코인 속도
    
    void Awake()
    {
        // 참조 설정
        tileGenerate = GameObject.FindGameObjectWithTag("TileGenerator").GetComponent<TileGenerate>(); // tileSpeed 참조용
    }

    void OnEnable() {
        // 코인 속도 -> 아이템 먹은 타이밍으로 고정
        coinSpeed = tileGenerate.carSpeed;
    }

    void Update()
    {
        // 코인 이동
        transform.position += new Vector3(0, 0, -coinSpeed * Time.deltaTime);
    }
}
