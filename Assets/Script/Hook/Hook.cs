using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Hook : MonoBehaviour
{
    public Player player;
    public string streetLightTag;
    private Rigidbody rb;

    // 추가 중력의 세기
    public float extraGravity = 20.0f;

    // 스킨변경을 위한 static 선언
    public static Material currentSkin;

    void Start()
    {
        // 스킨 변경을 위한 코드
        if (currentSkin != null)
        {
            ApplySkin(GetComponent<Renderer>());
            Transform fireTransform = transform.Find("Fire");
            ApplySkin(fireTransform.GetComponent<Renderer>());
        }

        // Player를 찾을 때 태그가 없거나 비활성화 상태면 에러가 날 수 있으므로 예외처리 추천
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            player = playerObj.GetComponent<Player>();
        }

        rb = GetComponent<Rigidbody>();
        rb.useGravity = true;
    }

    void FixedUpdate()
    {
        if (rb != null && rb.useGravity && !rb.isKinematic)
        {
            rb.AddForce(Vector3.down * extraGravity, ForceMode.Acceleration);
        }
    }

    void ApplySkin(Renderer r)
    {
        if (r != null)
        {
            int count = r.materials.Length;
            Material[] newMats = new Material[count];
            for (int i = 0; i < count; i++)
            {
                newMats[i] = currentSkin;
            }
            r.materials = newMats;
        }
    }

    private void OnTriggerEnter(Collider other)
    {

        if (rb.isKinematic) return;

        if (other.CompareTag(streetLightTag) || other.CompareTag("Helicopter"))
        {
            if (!rb.isKinematic)
            {
                rb.velocity = Vector3.zero;       // 1. 멈춤
                rb.angularVelocity = Vector3.zero;
                rb.useGravity = false;            // 2. 중력 끔
                rb.isKinematic = true;            // 3. 고정 (이 줄 이후로는 velocity 수정 불가)
            }

            // 위치 보정 (박스 콜라이더의 중심으로)
            BoxCollider box = other.gameObject.GetComponent<BoxCollider>();
            if (box != null)
            {
                // 충돌체의 중심점(World Position)으로 이동
                transform.position = box.transform.TransformPoint(box.center);
            }

            transform.SetParent(other.transform);

            if (player != null)
            {
                player.isHooked = true;

                if (other.CompareTag("Helicopter"))
                {
                    player.isHelicopter = true;

                    // 헬리콥터에 갈고리가 걸리자마자 상승을 위해 Helicopter.cs에 갈고리가 걸렸다는 정보를 넘기는 메소드
                    Helicopter heliCopterScript = other.GetComponentInParent<Helicopter>();
                    heliCopterScript.PlayerHooked();
                }
            }
        }
        else if (other.CompareTag("Tile"))
        {
            if (player != null)
            {
                player.isHooked = false;
            }
            Destroy(gameObject);
        }
    }
}