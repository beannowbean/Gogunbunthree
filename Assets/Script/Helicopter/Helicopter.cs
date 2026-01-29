/*
    헬리콥터가 높이 8에서 4로 내려오게 설정
    5초간 기다린 후, 높이 15로 올라가서 10초간 기다림.
    그 후 다시 높이 4로 내려간 후,
    마지막으로 높이 8로 올라가서 destroy.
*/


using UnityEngine;
using System.Collections;
using System.Collections.Generic;   // List 라이브러리 사용
using UnityEngine.UI;   // Image 클래스 사용

public class Helicopter : MonoBehaviour
{
    // 스킨 적용을 위한 변수
    [Header("Skin Settings")]
    public Renderer[] partsRenderers;   // helicopter의 blade와 body를 대입
    public static Texture currentSkin;
    
    private Transform playerTransform; // 플레이어 위치 참조
    private GameObject heliTouchIcon;   // InGameUI의 헬기 터치 도움 UI
    private float targetHeight = 4.0f; // 내려올 높이
    private float startSkyHeight = 8.0f;   // 시작 높이
    private float coinMapHeight = 15.0f;    // Coin Map 높이
    private float coinMapDuration = 10.0f;  // Coin Map 지속시간 
    private float zOffset = 6.0f;


    private float moveSpeed = 5f;
    private float stayDuration = 5.0f;

    private float currentHeight;

    // 아이템 로직을 위해 갈고리가 걸려있는지 확인하는 변수
    private bool isHookedHelicopter = false;

    private Collider heliCollider;

    void Start()
    {
        heliCollider = GetComponent<Collider>();
        // 게임 시작 시 선택된 스킨 적용
        if (currentSkin != null)
        {
            ApplySkin(currentSkin);
        }

        // 1. 플레이어 찾기 (태그 이용)
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj == null)
        {
            Destroy(gameObject); // 플레이어가 없으면 존재 가치가 없으므로 바로 파괴
            return;
        }
        playerTransform = playerObj.transform;

        // 헬기 터치 도움 UI 찾기 (태그 이용)
        heliTouchIcon = GameObject.FindGameObjectWithTag("TouchIcon");
        
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

    // Hook.cs에서 이 함수를 호출하여 훅이 걸렸다고 전달.
    public void PlayerHooked()
    {
        isHookedHelicopter = true;
        CoinMapGenerate();
        HeliTouchIconDeactive();
        ScoreManager.Instance.heliSuccessCount++;
    }

    // 스킨을 변경하는 함수
    void ApplySkin(Texture texture)
    {
        if (partsRenderers != null)
        {
            foreach (Renderer r in partsRenderers)
            {
                if (r != null)
                {
                    // 텍스처 교체
                    r.material.mainTexture = texture;
                }
            }
        }
    }

    /// <summary>
    /// Apply the static Helicopter.currentSkin to this instance.
    /// </summary>
    public void ApplyCurrentSkinToThis()
    {
        if (currentSkin != null) ApplySkin(currentSkin);
    }

    /// <summary>
    /// Apply the static Helicopter.currentSkin to all Helicopter instances.
    /// </summary>
    public static void ApplyCurrentSkinToAll()
    {
        Helicopter[] helis = FindObjectsOfType<Helicopter>();
        foreach (var h in helis)
        {
            h.ApplyCurrentSkinToThis();
        }
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
        // 5초 대기 상태에서 헬리콥터에 갈고리가 걸리면 바로 상승하도록
        // 시간이 5초 미만이고 훅을 걸었다면 다음 단계로.
        float timer = 0f;
        while (timer < stayDuration && isHookedHelicopter == false)
        {
            HeliTouchIconActive();
            timer += Time.deltaTime;
            yield return null;
        }



        // [3단계] 상승 (Target -> Sky)
        if (!isHookedHelicopter)
        {
            // [8. Acrophobia] 플레어이가 살아있는데 안 탔다면 업적 달성
            if (Player.Instance != null && !Player.Instance.isGameOver)
            {
                if (PlayerAchivementList.Instance != null)
                {
                    PlayerAchivementList.Instance.Acrophobia();
                }
            }

            heliCollider.enabled = false;
            HeliTouchIconDeactive();
        }
        
        while (Mathf.Abs(currentHeight - coinMapHeight) > 0.1f)
        {
            currentHeight = Mathf.MoveTowards(currentHeight, coinMapHeight, moveSpeed * Time.deltaTime);
            yield return null;
        }

        // 플레이어가 헬리콥터에 갈고리를 걸지 않았다면 헬리콥터 삭제
        if (isHookedHelicopter == false)
        {
            SFXManager.Instance.Stop("Helicopter"); // 오디오 멈춤.
            Destroy(gameObject);
            yield break;    // 코루틴 강제 종료
        }

        // [4단계] CoinMap 시간 대기
        yield return new WaitForSeconds(coinMapDuration);

        // [5단계] CoinMap에서 지상으로 하강
        while (Mathf.Abs(currentHeight - (targetHeight + 1)) > 0.1f)  // 차에 부딪히면 죽는 현상 수정 (좀 더 높은 데서 떨어짐)
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

    // 헬기 코인 맵 생성
    void CoinMapGenerate()
    {
        // 코인 도달 시간 계산
        float reachTime = (coinMapHeight - targetHeight) / moveSpeed;

        TileGenerate tileGenerate = GameObject.FindGameObjectWithTag("TileGenerator").GetComponent<TileGenerate>(); // carSpeed 참조용
        HeliCoinGenerate heliCoinGenerate = GameObject.FindGameObjectWithTag("HeliCoinGenerator").GetComponent<HeliCoinGenerate>(); // startCoinMap 호출용

        // 코인 맵 생성
        heliCoinGenerate.StartCoinMap(tileGenerate.carSpeed, reachTime, coinMapDuration);
    }

    // 헬기 터치 보조 UI 생성
    void HeliTouchIconActive()
    {
        // 헬기 성공 횟수 2번 이상이면 더 이상 X
        if(ScoreManager.Instance.heliSuccessCount >= 2) return;
        
        // Image 컴포넌트 가져와 깜빡이기
        Image heliTouchImage = heliTouchIcon.GetComponent<Image>();
        Color c = heliTouchImage.color;
        c.a = Mathf.PingPong(Time.time * 2.5f, 1.0f);
        heliTouchImage.color = c;
    }

    // 헬기 터치 보조 UI 종료
    void HeliTouchIconDeactive()
    {
        // Image 컴포넌트 가져와 투명도 복구
        Image heliTouchImage = heliTouchIcon.GetComponent<Image>();
        Color c = heliTouchImage.color;
        c.a = 0.0f;
        heliTouchImage.color = c;
    }
}
