using UnityEngine;

// 자석아이템이 돌아가는 로직, 자석효과와 지속시.
public class Magnet : MonoBehaviour
{
    public float rotateSpeed = 100.0f;
    public float duration = 10.0f; // 자석 지속 시간

    // UI에 넣을 이미지
    public Sprite iconSprite;

    void Update()
    {
        // 아이템 뱅글뱅글 회전
        transform.Rotate(0, rotateSpeed * Time.deltaTime, 0);
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

                ItemUIController.Instance.ActivateNextAvailableItem(duration, iconSprite);

                // 자석 지속시간동안 효과음
                SFXManager.Instance.Play("MagnetSound");
            }
            Destroy(gameObject);
        }
    }
}