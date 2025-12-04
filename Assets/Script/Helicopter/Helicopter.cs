using UnityEngine;
using System.Collections;

public class Helicopter : MonoBehaviour
{
    private Transform playerTransform; // 플레이어 위치 참조

    private float targetHeight = 4.0f; // 내려올 높이
    private float skyHeight = 30.0f;   // 시작 및 복귀 높이
    private float zOffset = 6.0f;


    private float moveSpeed = 2.5f;
    private float stayDuration = 5.0f;

    private float currentHeight;
    private bool isChasing = true;

    void Start()
    {
        // 1. 플레이어 찾기 (태그 이용)
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj == null)
        {
            Destroy(gameObject); // 플레이어가 없으면 존재 가치가 없으므로 바로 파괴
            return;
        }
        playerTransform = playerObj.transform;

        // 2. 초기 위치 설정 (하늘 위, 플레이어 기준 오프셋)
        currentHeight = skyHeight;
        transform.position = new Vector3(0, currentHeight, playerTransform.position.z + zOffset);

        // 3. 움직임 코루틴 즉시 시작
        StartCoroutine(MoveRoutine());
    }

    void Update()
    {
        if (playerTransform == null) return;

        // X, Z축은 플레이어를 계속 따라다님 (Y축은 코루틴이 제어)
        if (isChasing)
        {
            float targetZ = playerTransform.position.z + zOffset;
            transform.position = new Vector3(0, currentHeight, targetZ);
        }
    }

    public void StopChasing()
    {
        isChasing = false;
    }

    IEnumerator MoveRoutine()
    {
        // [1단계] 하강 (Sky -> Target)
        while (Mathf.Abs(currentHeight - targetHeight) > 0.1f)
        {
            currentHeight = Mathf.MoveTowards(currentHeight, targetHeight, moveSpeed * Time.deltaTime);
            yield return null;
        }
        currentHeight = targetHeight;

        // [2단계] 대기
        yield return new WaitForSeconds(stayDuration);

        // [3단계] 상승 (Target -> Sky)
        while (Mathf.Abs(currentHeight - skyHeight) > 0.1f)
        {
            currentHeight = Mathf.MoveTowards(currentHeight, skyHeight, moveSpeed * Time.deltaTime);
            yield return null;
        }

        // [4단계] 임무 완료 후 오브젝트 파괴
        SFXManager.Instance.Stop("Helicopter");
        Destroy(gameObject);
    }
}