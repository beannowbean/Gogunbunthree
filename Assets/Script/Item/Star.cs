using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Star : MonoBehaviour
{
    // 회전 속도 (Z축 기준)
    public float rotateSpeed = 300f;

    // 무적 지속 시간
    public float invincibilityDuration = 5.0f;

    // ★ [추가 1] 인스펙터에서 아이콘 이미지를 넣을 변수
    public Sprite iconSprite;

    private bool isCollected = false;   // 중복 획득 방지 변수

    public GameObject starEffectPrefab; //이펙

    void Update()
    {
        // 1. Z축을 기준으로 뱅글뱅글 회전
        transform.Rotate(0, 0, rotateSpeed * Time.deltaTime);
    }

    private void OnTriggerEnter(Collider other)
    {
        if(isCollected) return; // 이미 획득된 상태면 무시

        // 2. 플레이어와 닿으면
        if (other.CompareTag("Player"))
        {
            SFXManager.Instance.Play("Star");
            isCollected = true; // 중복 획득 방지

            // 플레이어 스크립트를 가져와서 무적 함수 실행
            Player playerScript = other.GetComponent<Player>();

            if (playerScript != null)
            {
                // 실제 플레이어 무적 로직 실행
                playerScript.ActivateInvincibility(invincibilityDuration);

                // ★ [추가 2] UI 매니저에게 "UI 슬롯에 아이콘 띄워줘!" 라고 요청
                // 지속시간은 위에서 설정한 invincibilityDuration을 그대로 사용
                ItemUIController.Instance.ActivateNextAvailableItem(invincibilityDuration, iconSprite);
            }
            GameObject effect = Instantiate(starEffectPrefab, transform.position, Quaternion.identity);
            Destroy(effect, 1.0f); // 1초 뒤 삭제

            // 별 아이템 삭제
            Destroy(gameObject);
        }
    }
}