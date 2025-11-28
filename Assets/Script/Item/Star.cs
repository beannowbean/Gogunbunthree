using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Star : MonoBehaviour
{
    // 회전 속도 (Z축 기준)
    public float rotateSpeed = 100f;

    // 무적 지속 시간
    public float invincibilityDuration = 5.0f;

    void Update()
    {
        // 1. Z축을 기준으로 뱅글뱅글 회전
        transform.Rotate(0, 0, rotateSpeed * Time.deltaTime);
    }

    private void OnTriggerEnter(Collider other)
    {
        // 2. 플레이어와 닿으면
        if (other.CompareTag("Player"))
        {
            // 플레이어 스크립트를 가져와서 무적 함수 실행
            Player playerScript = other.GetComponent<Player>();
            if (playerScript != null)
            {
                playerScript.ActivateInvincibility(invincibilityDuration);
            }

            // 별 아이템 삭제
            Destroy(gameObject);
        }
    }
}