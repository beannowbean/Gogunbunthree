using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CarLaneMove : MonoBehaviour    // 움직이는 차 차선 변경 스크립트
{
    public bool isLeft = true;      // true = 왼쪽, false = 오른쪽
    public float moveSpeed = 10f;   // 차선 변경 속도

    public GameObject arrowObject;  // 깜빡이는 화살표(자식 오브젝트)
    
    private Vector3 targetPos;
    private bool isMoving = false;
    private Coroutine blinkRoutine;
    public float blinkTime = 0.25f;

    void Start()
    {
        // 시작 시 깜빡이 설정 시작
        StartBlinkIndicator();
    }

    void Update()
    {
        if (isMoving == true)
        {
            // 목표 위치로 이동
            float newX = Mathf.MoveTowards(transform.localPosition.x, targetPos.x, moveSpeed * Time.deltaTime);
            transform.localPosition = new Vector3(newX, transform.localPosition.y, transform.localPosition.z);
            
            // 도착 판정
            if (Mathf.Abs(newX - targetPos.x) < 0.001f)
            {
                transform.localPosition = targetPos;
                isMoving = false;
            }
        }
    }

    // 깜빡이 시작
    void StartBlinkIndicator()
    {
        // 방향에 따라 화살표 회전 설정
        if (isLeft)
            arrowObject.transform.localRotation = Quaternion.Euler(0, -180f, 0);
        else
            arrowObject.transform.localRotation = Quaternion.Euler(0, 0, 0);

        // 이미 깜빡임이 돌고 있으면 중지
        if (blinkRoutine != null)
            StopCoroutine(blinkRoutine);

        blinkRoutine = StartCoroutine(BlinkArrow());
    }

    // 화살표 깜빡임 루틴
    IEnumerator BlinkArrow()
    {
        while (true)
        {
            arrowObject.SetActive(true);
            yield return new WaitForSeconds(blinkTime);

            arrowObject.SetActive(false);
            yield return new WaitForSeconds(blinkTime);
        }
    }

    // 트리거에 들어가는 순간(레인 이동)
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "CarMove" && isMoving == false)
        {
            // 깜빡이 종료
            if (blinkRoutine != null)
                StopCoroutine(blinkRoutine);

            arrowObject.SetActive(false);

            // 부모 포함한 세계 x값
            float worldX = transform.position.x;

            // -2.4, 0, 2.4 중 가장 가까운 값으로 반올림
            float roundedX = Mathf.Round(worldX / 2.4f) * 2.4f;

            // 이동 방향 적용
            roundedX += isLeft ? 2.4f : -2.4f;

            // 레인 범위 제한
            roundedX = Mathf.Clamp(roundedX, -2.4f, 2.4f);

            // 목표 localPosition 계산
            float targetLocalX = roundedX - transform.parent.position.x;
            targetPos = new Vector3(targetLocalX, transform.localPosition.y, transform.localPosition.z);
            isMoving = true;
        }
    }
}