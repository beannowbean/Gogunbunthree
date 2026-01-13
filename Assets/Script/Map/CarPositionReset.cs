using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 자동차 위치 리셋 스크립트
/// </summary>
public class CarPositionReset : MonoBehaviour   // 별로 날라간 위치 잡아주는 스크립트
{
    // 내부 변수
    struct CarTransform   // 차의 초기 위치와 회전 저장하는 구조체
    {
        public Transform target;    // 차의 트랜스폼
        public Vector3 initialPos;  // 초기 위치
        public Quaternion initialRot;   // 초기 회전
    }
    List<CarTransform> carData = new List<CarTransform>();    // 차들의 초기 위치와 회전 저장하는 리스트

    void Awake()
    {
        // 처음 생성될 때 모든 자식의 상대적 위치와 회전 저장
        foreach (Transform child in transform)
        {
            CarTransform data = new CarTransform
            {
                target = child,
                initialPos = child.localPosition,
                initialRot = child.localRotation
            };
            carData.Add(data);
        }
    }

    void OnEnable()
    {
        // 모든 자식의 위치와 회전 초기화
        foreach (var data in carData)
        {
            if (data.target != null)
            {
                data.target.localPosition = data.initialPos;
                data.target.localRotation = data.initialRot;
            }
        }
    }
}
