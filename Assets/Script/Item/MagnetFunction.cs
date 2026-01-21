using UnityEngine;

public class MagneticItem : MonoBehaviour
{
    [Header("자석 설정")]
    public float pullSpeed = 25.0f;
    public float magnetRange = 7.0f;
    public float collectDistance = 0.5f; 

    private bool isFollowing = false; 

    void Update()
    {
        if (Player.Instance == null) return;

        // 게임오버면 아이템 끄기
        if (Player.Instance.isGameOver)
        {
            gameObject.SetActive(false);
            return;
        }

        // 플레이어의 자석이 켜져 있는지 확인
        if (Player.Instance.isMagnetActive)
        {
            Vector3 targetPos = Player.Instance.transform.position + Vector3.up * 1.0f;

            float distance = Vector3.Distance(transform.position, targetPos);

            // 범위 안에 있거나, 이미 끌려오고 있는 중이라면
            if (distance < magnetRange || isFollowing)
            {
                isFollowing = true; 

                // 거리가 가까우면 강제로 위치를 일치시킴
                if (distance <= collectDistance)
                {
                    transform.position = targetPos;
                    return; 
                }

                // 플레이어 쪽으로 이동
                transform.position = Vector3.MoveTowards(transform.position, targetPos, pullSpeed * Time.deltaTime);
            }
        }
        else
        {
            // 자석 효과가 끝나면 중지
            isFollowing = false;
        }
    }
}