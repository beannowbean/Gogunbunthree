using UnityEngine;

// 자석아이템이 돌아가는 로직, 자석효과와 지속시.
public class Magnet : MonoBehaviour
{
    public float rotateSpeed = 100.0f;
    public float duration = 10.0f; // 자석 지속 시간

    void Update()
    {
        // 아이템 뱅글뱅글 회전
        transform.Rotate(0, 0, rotateSpeed * Time.deltaTime);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            Player player = other.GetComponent<Player>();
            if (player != null)
            {
                // 플레이어에게 자석 효과 켜라고 명령
                player.ActivateMagnet(duration);

                // 효과음
                // SFXManager.Instance.Play("ItemGet");
            }
            Destroy(gameObject);
        }
    }
}