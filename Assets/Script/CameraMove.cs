using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraMove : MonoBehaviour
{
    public Transform target;          // 따라갈 플레이어

    public Vector3 offset = new Vector3(0, 3, -5);   // 기본 오프셋

    public float followSpeed = 5f;    // 위치 따라가기 속도 

    public GameObject[] designer;
    private Vector3[] designerOffsets;

    void Start()
    {
        // 디자이너들의 초기 상대 오프셋 저장
        designerOffsets = new Vector3[designer.Length];
        for (int i = 0; i < designer.Length; i++)
        {
            designerOffsets[i] = designer[i].transform.position - transform.position;
        }
    }

    void Update()
    {
        // 목표 위치 = 플레이어 위치 + 오프셋
        Vector3 targetPos = target.position + offset;

        // 부드럽게 이동
        transform.position = Vector3.Lerp(transform.position, targetPos, Time.deltaTime * followSpeed);

        // 디자이너 오브젝트 이동
        for (int i = 0; i < designer.Length; i++)
        {
            Vector3 desiredPos = transform.position + designerOffsets[i];
            desiredPos.y = 0;

            designer[i].transform.position = Vector3.Lerp(designer[i].transform.position, desiredPos, Time.deltaTime * followSpeed);
        }
    }
}