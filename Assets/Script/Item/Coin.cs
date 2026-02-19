using UnityEngine;

public class Coin : MonoBehaviour
{
    private Vector3 initialPos; // 초기 위치 저장 변수

    void Awake()
    {
        // 태어날 때 원래 위치 기억
        initialPos = transform.localPosition;
    }

    void OnEnable()
    {
        // 다시 활성화될 때(풀링 재사용 시) 원래 위치로 복귀
        transform.localPosition = initialPos;
    }

}