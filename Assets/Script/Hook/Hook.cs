using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Hook : MonoBehaviour
{
    public Player player;
    public string streetLightTag;
    private Rigidbody rb;

    // [추가됨] 추가 중력의 세기 (이 값을 높일수록 더 빨리 떨어짐)
    public float extraGravity = 20.0f;

    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player").GetComponent<Player>();
        rb = GetComponent<Rigidbody>();
        rb.useGravity = true;
    }

    // [추가됨] 물리 연산은 FixedUpdate에서 하는 것이 좋습니다.
    void FixedUpdate()
    {
        // 후크가 날아가는 중(중력을 쓰고 있는 중)이라면
        if (rb != null && rb.useGravity)
        {
            // 아래쪽 방향(Vector3.down)으로 추가 힘을 가함
            // ForceMode.Acceleration: 질량 무시하고 가속도만 줌 (가장 깔끔함)
            rb.AddForce(Vector3.down * extraGravity, ForceMode.Acceleration);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag(streetLightTag) || other.CompareTag("Helicopter"))
        {
            rb.velocity = Vector3.zero;
            rb.useGravity = false;
            rb.isKinematic = true;

            BoxCollider box = other.gameObject.GetComponent<BoxCollider>();
            if (box != null)
            {
                transform.position = box.transform.TransformPoint(box.center);
            }

            transform.SetParent(other.transform);

            player.isHooked = true;

            if (other.CompareTag("Helicopter"))
            {
                player.isEnding = true;
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