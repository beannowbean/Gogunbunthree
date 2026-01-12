using UnityEngine;

public class Coin : MonoBehaviour
{
    private Transform playerTransform;
    private Player playerScript;

    public float flySpeed = 30.0f; // 날아가는 속도
    public float magnetRange = 10.0f; // 자석 감지 범위 (무제한으로 하려면 아주 큰 수)

    void Start()
    {
        // 게임 시작 시 플레이어 찾아두기
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            playerTransform = playerObj.transform;
            playerScript = playerObj.GetComponent<Player>();
        }
    }

    void Update()
    {
        if (playerTransform == null || playerScript == null) return;

        // 플레이어의 자석이 켜져 있는지 확인
        if (playerScript.isMagnetActive)
        {
            // 거리 계산
            float distance = Vector3.Distance(transform.position, playerTransform.position);

            // 범위 안에 들어오면 플레이어 쪽으로 이동
            if (distance < magnetRange)
            {
                // MoveTowards로 플레이어 위치로 이동
                transform.position = Vector3.MoveTowards(transform.position, playerTransform.position, flySpeed * Time.deltaTime);
            }
        }
    }
}