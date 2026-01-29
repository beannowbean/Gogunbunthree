using UnityEngine;

public class MagneticItem : MonoBehaviour
{
    [Header("자석 설정")]
    public float pullSpeed = 70.0f;
    public float magnetRange = 7.0f;
    public float collectDistance = 0.5f; 

    private bool isFollowing = false; 
    private Vector3 virtualWorldPos;    // 순간이동에 영향 안받는 가상 좌표

    // 오브젝트 풀링 재사용 초기화 용도
    void OnEnable()
    {
        isFollowing = false;
    }

    void Update()
    {
        if (Player.Instance == null) return;

        // 게임오버면 아이템 끄기
        if (Player.Instance.isGameOver)
        {
            if(CompareTag("Coin")){}    // 코인이면 비활성화 X
            else gameObject.SetActive(false);
            return;
        }

        // 플레이어의 자석이 켜져 있는지 확인
        if (Player.Instance.isMagnetActive)
        {
            Vector3 targetPos = Player.Instance.transform.position + Vector3.up * 1.0f;

            // 따라가는 중이면 가상좌표 기준으로 변경
            float distance = isFollowing ? Vector3.Distance(virtualWorldPos, targetPos) : Vector3.Distance(transform.position, targetPos);

            // [수정] 헬리콥터 타고 올라갈 때 5보다 높아지면 그만 따라가기
            if (Mathf.Abs(transform.position.y - targetPos.y) > 5.0f)
            {
                isFollowing = false; 
                return;
            }

            // 범위 안에 있거나, 이미 끌려오고 있는 중이라면
            if (distance < magnetRange || isFollowing)
            {
                // 따라가기 시작하는 순간, 현재 실제 위치 가상 좌표에 보관
                if (!isFollowing)
                {
                    isFollowing = true;
                    virtualWorldPos = transform.position;
                }
                // 거리가 가까우면 강제로 위치를 일치시킴
                if (distance <= collectDistance)
                {
                    transform.position = targetPos;
                    return; 
                }

                // 매 프레임 위치 계산
                virtualWorldPos = Vector3.MoveTowards(virtualWorldPos, targetPos, pullSpeed * Time.deltaTime);

                // 플레이어 쪽으로 이동
                transform.position = virtualWorldPos;
            }
        }
        else
        {
            // 자석 효과가 끝나면 중지
            isFollowing = false;
        }
    }
}