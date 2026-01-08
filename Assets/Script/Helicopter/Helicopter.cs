/*
    헬리콥터가 높이 8에서 4로 내려오게 설정
    5초간 기다린 후, 높이 15로 올라가서 10초간 기다림.
    그 후 다시 높이 4로 내려간 후,
    마지막으로 높이 8로 올라가서 destroy.
*/


using UnityEngine;
using System.Collections;

public class Helicopter : MonoBehaviour
{
    private Transform playerTransform; // 플레이어 위치 참조

    private float targetHeight = 4.0f; // 내려올 높이
    private float startSkyHeight = 8.0f;   // 시작 높이
    private float coinMapHeight = 15.0f;    // Coin Map 높이
    private float coinMapDuration = 10.0f;  // Coin Map 지속시간 
    private float zOffset = 6.0f;


    private float moveSpeed = 5f;
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
        currentHeight = startSkyHeight;
        transform.position = new Vector3(0, currentHeight, playerTransform.position.z + zOffset);

        // 3. 움직임 코루틴 즉시 시작
        StartCoroutine(MoveRoutine());
    }

    void Update()
    {
        transform.position = new Vector3(transform.position.x, currentHeight, transform.position.z);
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
        while (Mathf.Abs(currentHeight - coinMapHeight) > 0.1f)
        {
            currentHeight = Mathf.MoveTowards(currentHeight, coinMapHeight, moveSpeed * Time.deltaTime);
            yield return null;
        }

        // [4단계] CoinMap 시간 대기
        yield return new WaitForSeconds(coinMapDuration);

        // [5단계] CoinMap에서 지상으로 하강
        while (Mathf.Abs(currentHeight - targetHeight) > 0.1f)
        {
            currentHeight = Mathf.MoveTowards(currentHeight, targetHeight, moveSpeed * Time.deltaTime);
            yield return null;
        }
        currentHeight = targetHeight;
        if (playerTransform != null)
        {
            Player playerScript = playerTransform.GetComponent<Player>();
            if (playerScript != null && playerScript.isHelicopter)
            {
                playerScript.ReleaseHelicopter(); // 강제 해제 함수 호출
            }
        }

        // [6단계] 헬리콥터 상승
        while (Mathf.Abs(currentHeight - startSkyHeight) > 0.1f)
        {
            currentHeight = Mathf.MoveTowards(currentHeight, startSkyHeight, moveSpeed * Time.deltaTime);
            yield return null;
        }

        // [7단계] 임무 완료 후 오브젝트 파괴
        SFXManager.Instance.Stop("Helicopter");
        Destroy(gameObject);
    } 
}