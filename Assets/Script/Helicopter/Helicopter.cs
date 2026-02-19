using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

public class Helicopter : MonoBehaviour
{
    [Header("Skin Settings")]
    public Renderer[] partsRenderers;
    public static Texture currentSkin;

    private Transform playerTransform;
    private GameObject heliTouchIcon;
    private float targetHeight = 4.0f;
    private float startSkyHeight = 15.0f;
    private float coinMapHeight = 15.0f;
    private float coinMapDuration = 10.0f;
    private float zOffset = 6.0f;

    private float moveSpeed = 5f;
    private float stayDuration = 5.0f;

    private float currentHeight;
    private bool isHookedHelicopter = false;

    private Collider heliCollider;

    void Start()
    {
        heliCollider = GetComponent<Collider>();
        if (currentSkin != null) ApplySkin(currentSkin);

        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj == null)
        {
            Destroy(gameObject);
            return;
        }
        playerTransform = playerObj.transform;

        heliTouchIcon = GameObject.FindGameObjectWithTag("TouchIcon");

        currentHeight = startSkyHeight;
        transform.position = new Vector3(0, currentHeight, playerTransform.position.z + zOffset);

        StartCoroutine(MoveRoutine());
    }

    void Update()
    {
        transform.position = new Vector3(transform.position.x, currentHeight, transform.position.z);
    }

    public void PlayerHooked()
    {
        isHookedHelicopter = true;
        CoinMapGenerate();
        HeliTouchIconDeactive();
        ScoreManager.Instance.heliSuccessCount++;
    }

    void ApplySkin(Texture texture)
    {
        if (partsRenderers != null)
        {
            foreach (Renderer r in partsRenderers)
                if (r != null) r.material.mainTexture = texture;
        }
    }

    public void ApplyCurrentSkinToThis()
    {
        if (currentSkin != null) ApplySkin(currentSkin);
    }

    public static void ApplyCurrentSkinToAll()
    {
        Helicopter[] helis = FindObjectsOfType<Helicopter>();
        foreach (var h in helis) h.ApplyCurrentSkinToThis();
    }

    IEnumerator MoveRoutine()
    {
        // [1단계] 하강
        while (Mathf.Abs(currentHeight - targetHeight) > 0.1f)
        {
            currentHeight = Mathf.MoveTowards(currentHeight, targetHeight, moveSpeed * Time.deltaTime);
            yield return null;
        }
        currentHeight = targetHeight;

        // [2단계] 대기
        float timer = 0f;
        while (timer < stayDuration && isHookedHelicopter == false)
        {
            HeliTouchIconActive();
            timer += Time.deltaTime;
            yield return null;
        }

        // [3단계] 상승
        float currentTargetHeight = coinMapHeight;

        if (isHookedHelicopter)
        {
            // 헬리콥터는 1만큼 더 올라감 (15 + 1 = 16)
            currentTargetHeight = coinMapHeight;

        }
        else
        {
            if (Player.Instance != null && !Player.Instance.isGameOver)
            {
                if (PlayerAchivementList.Instance != null) PlayerAchivementList.Instance.Acrophobia();
            }

            heliCollider.enabled = false;
            HeliTouchIconDeactive();
        }

        while (Mathf.Abs(currentHeight - currentTargetHeight) > 0.1f)
        {
            currentHeight = Mathf.MoveTowards(currentHeight, currentTargetHeight, moveSpeed * Time.deltaTime);
            yield return null;
        }

        if (isHookedHelicopter == false)
        {
            SFXManager.Instance.Stop("Helicopter");
            Destroy(gameObject);
            yield break;
        }

        // [4단계] CoinMap 시간 대기
        yield return new WaitForSeconds(coinMapDuration);

        // [5단계] 하강
        while (Mathf.Abs(currentHeight - (targetHeight + 1)) > 0.1f)
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
                playerScript.ReleaseHelicopter();
            }
        }

        // [6단계] 상승 및 종료
        while (Mathf.Abs(currentHeight - startSkyHeight) > 0.1f)
        {
            currentHeight = Mathf.MoveTowards(currentHeight, startSkyHeight, moveSpeed * Time.deltaTime);
            yield return null;
        }

        SFXManager.Instance.Stop("Helicopter");
        Destroy(gameObject);
    }

    void ExtendPlayerRope(float amount)
    {
        if (playerTransform == null) return;

        DistanceJoint2D distJoint = playerTransform.GetComponent<DistanceJoint2D>();
        if (distJoint != null && distJoint.enabled)
        {
            distJoint.autoConfigureDistance = false;
            distJoint.distance += amount;
            return;
        }

        SpringJoint2D springJoint = playerTransform.GetComponent<SpringJoint2D>();
        if (springJoint != null && springJoint.enabled)
        {
            springJoint.autoConfigureDistance = false;
            springJoint.distance += amount;
            return;
        }
    }

    void CoinMapGenerate()
    {
        // 도달 시간 계산: 헬리콥터가 올라가는 총 높이(기본+1)를 고려
        float reachTime = (coinMapHeight - targetHeight) / moveSpeed;

        TileGenerate tileGenerate = GameObject.FindGameObjectWithTag("TileGenerator").GetComponent<TileGenerate>();
        HeliCoinGenerate heliCoinGenerate = GameObject.FindGameObjectWithTag("HeliCoinGenerator").GetComponent<HeliCoinGenerate>();

        heliCoinGenerate.StartCoinMap(tileGenerate.carSpeed, reachTime, coinMapDuration);
    }

    void HeliTouchIconActive()
    {
        if (ScoreManager.Instance.heliSuccessCount >= 2) return;

        Image heliTouchImage = heliTouchIcon.GetComponent<Image>();
        Color c = heliTouchImage.color;
        c.a = Mathf.PingPong(Time.time * 2.5f, 1.0f);
        heliTouchImage.color = c;
    }

    void HeliTouchIconDeactive()
    {
        Image heliTouchImage = heliTouchIcon.GetComponent<Image>();
        Color c = heliTouchImage.color;
        c.a = 0.0f;
        heliTouchImage.color = c;
    }
}