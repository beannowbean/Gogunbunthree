using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    // 캐릭터 이동 속도
    public float moveSpeed = 0f;

    // 1,2,3 차선 거리, 넘어가는 속도, 현재 차선확인, 목표차선위치
    public float laneDistance = 2.0f;
    public float crossSpeed = 10.0f;
    private int currentLane = 2;
    private Vector3 targetPosition;
    public float diveSpeed = 20.0f;

    // 터치 시작 위치, 종료 위치, 스와이프 속도 감지
    private Vector2 touchStartPos;
    private Vector2 touchEndPos;
    private float minSwipeDistance = 50.0f;

    // 점프
    public float jumpForce = 6.0f;
    private bool isGrounded = true;

    // 바닥 tag 입력
    public string groundTag;

    // Hook 관련 설정
    public GameObject hookPrefab;
    public Transform hookStartingPoint;
    public float hookSpeed = 10.0f;
    public bool isHooked = false;
    private GameObject currentHook;
    public LineRenderer lineRenderer;
    public int hookAngle = 30;
    public float hookPullSpeed = 30.0f;

    // 헬리콥터 관련 변수
    public GameObject helicopterPrefab;
    public float heliSpawnChance = 0.05f;
    private GameObject currentHelicopter;
    public bool isEnding = false;

    // GameOver 관련 변수
    private bool isGameOver = false;

    // Item (무적) 관련 변수
    public bool isInvincible = false;
    public float carImpactForce = 20.0f;
    public Material invincibleMaterial;

    // [수정됨] 하나가 아니라 '모든' 렌더러와 재질을 저장하기 위한 배열 선언
    private Renderer[] allRenderers;
    private Material[] originalMaterials;

    // 컴포넌트 선언
    private Rigidbody rb;
    private Animator anim;

    void Start()
    {
        currentLane = 2;
        rb = GetComponent<Rigidbody>();
        anim = GetComponent<Animator>();
        lineRenderer.enabled = false;
        anim.SetBool("isGrounded", true);

        // [수정됨] 자식 오브젝트들에 있는 '모든(All)' 렌더러를 찾아옵니다.
        allRenderers = GetComponentsInChildren<Renderer>();

        // 원래 재질들을 저장할 배열 크기를 맞춥니다.
        originalMaterials = new Material[allRenderers.Length];

        // 반복문을 돌며 각 파츠의 원래 재질을 저장해둡니다.
        for (int i = 0; i < allRenderers.Length; i++)
        {
            originalMaterials[i] = allRenderers[i].material;
        }
    }

    void Update()
    {
        if (isGameOver) return;
        if (isHooked && currentHook != null) MoveToHook();

        transform.Translate(Vector3.forward * moveSpeed * Time.deltaTime, Space.World);

        CheckInput();
        float targetX = (currentLane - 2) * laneDistance;
        Vector3 newPosition = new Vector3(targetX, transform.position.y, transform.position.z);
        Vector3 moveVector = Vector3.Lerp(transform.position, newPosition, crossSpeed * Time.deltaTime);
        transform.position = new Vector3(moveVector.x, transform.position.y, transform.position.z);
        DrawRope();
    }

    // ... (CheckInput, CalculateSwipe, ChangeLane, Jump, DrawRope, hookShoot, ReleaseHook, MoveToHook 함수들은 기존과 동일) ...

    // 공간 절약을 위해 중복되는 함수 내용은 생략했습니다. 기존 코드를 그대로 쓰시면 됩니다.
    // CheckInput ~ MoveToHook 까지는 수정할 필요 없습니다.

    void CheckInput()
    {
        if (Input.GetKeyDown(KeyCode.LeftArrow)) ChangeLane(-1);
        if (Input.GetKeyDown(KeyCode.RightArrow)) ChangeLane(1);
        if (Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.Space)) Jump();
        if (Input.GetMouseButtonDown(0) && !isGrounded) hookShoot();
        if (Input.GetMouseButtonUp(0) && isHooked == false) ReleaseHook();
        if (Input.GetKeyDown(KeyCode.DownArrow) && !isGrounded) QuickDive();

        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);
            if (touch.phase == TouchPhase.Began)
            {
                touchStartPos = touch.position;
                if (!isGrounded) hookShoot();
            }
            else if (touch.phase == TouchPhase.Ended)
            {
                touchEndPos = touch.position;
                CalculateSwipe();
                ReleaseHook();
            }
        }
    }

    void CalculateSwipe()
    {
        float swipeDistanceX = touchEndPos.x - touchStartPos.x;
        float swipeDistanceY = touchEndPos.y - touchStartPos.y;
        if (Mathf.Abs(swipeDistanceX) < minSwipeDistance && Mathf.Abs(swipeDistanceY) < minSwipeDistance) return;
        if (Mathf.Abs(swipeDistanceX) > Mathf.Abs(swipeDistanceY))
        {
            if (swipeDistanceX > 0) ChangeLane(1);
            else ChangeLane(-1);
        }
        else
        {
            if (swipeDistanceY > 0) Jump();
        }
    }

    void ChangeLane(int direction)
    {
        int targetLane = currentLane + direction;
        if (targetLane >= 1 && targetLane <= 3) currentLane = targetLane;
    }

    void Jump()
    {
        if (isGrounded)
        {
            rb.AddForce(Vector3.up * jumpForce, ForceMode.VelocityChange);
            isGrounded = false;
            anim.SetBool("isGrounded", false);
        }
    }

    void DrawRope()
    {
        if (currentHook != null)
        {
            if (!lineRenderer.enabled) lineRenderer.enabled = true;
            lineRenderer.SetPosition(0, hookStartingPoint.position);
            lineRenderer.SetPosition(1, currentHook.transform.position);
        }
        else
        {
            if (lineRenderer.enabled) lineRenderer.enabled = false;
        }
    }

    void hookShoot()
    {
        if (currentHook != null) return;
        Quaternion projectileRotation = Quaternion.Euler(-90f - hookAngle, transform.eulerAngles.y, 0f);
        currentHook = Instantiate(hookPrefab, hookStartingPoint.position, projectileRotation);
        Rigidbody hrb = currentHook.GetComponent<Rigidbody>();
        Vector3 shootDirection = Quaternion.Euler(-hookAngle, 0f, 0f) * transform.forward;
        hrb.velocity = shootDirection * hookSpeed;
        Hook hookScript = currentHook.GetComponent<Hook>();
        hookScript.player = this;
    }

    void ReleaseHook()
    {
        if (currentHook != null && isHooked == true)
        {
            Destroy(currentHook);
            isHooked = false;
            currentHook = null;
            lineRenderer.enabled = false;
        }
        else if (isHooked == false)
        {
            Destroy(currentHook);
            isHooked = false;
            currentHook = null;
            lineRenderer.enabled = false;
        }
    }

    void MoveToHook()
    {
        if (isEnding) StartCoroutine(Ending());
        else
        {
            transform.position = Vector3.MoveTowards(transform.position, currentHook.transform.position, hookPullSpeed * Time.deltaTime);
            float distance = Vector3.Distance(transform.position, currentHook.transform.position);
            if (distance <= 2) ReleaseHook();
        }
    }

    void QuickDive()
    {
        rb.velocity = new Vector3(rb.velocity.x, -diveSpeed, rb.velocity.z);
    }

    // ... (여기까지 기존 함수들) ...

    // [수정] 물리적 충돌 (벽, 바닥 등) - 자동차 코드는 여기서 뺍니다.
    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag(groundTag))
        {
            isGrounded = true;
            anim.SetBool("isGrounded", true);
            isHooked = false;
        }
        else if (collision.collider.CompareTag("Tile"))
        {
            gameObject.SetActive(false);
        }
        // 자동차 코드는 삭제됨
    }

    // [수정] 통과형 충돌 (코인, 별, 자동차)
    private void OnTriggerEnter(Collider other)
    {
        // 1. 코인
        if (other.CompareTag("Coin"))
        {
            Destroy(other.gameObject);
            TrySpawnHelicopter();
            ScoreManager.Instance.AddCoin(1);
        }
        // 3. [이동됨] 자동차 (Car)
        else if (other.CompareTag("Car"))
        {
            if (isInvincible)
            {
                // --- 자동차 날리기 로직 ---
                Rigidbody carRb = other.gameObject.GetComponent<Rigidbody>();
                // 혹시 부모에 Rigidbody가 있는 구조라면 아래 주석 해제
                // if (carRb == null) carRb = other.gameObject.GetComponentInParent<Rigidbody>();

                if (carRb != null)
                {
                    // 날리기 위해 잠시 물리 엔진 켜기
                    carRb.isKinematic = false;
                    carRb.useGravity = true;

                    // 플레이어 진행 방향 + 위쪽으로 날리기
                    Vector3 flyDirection = transform.forward + Vector3.up * 1.5f;

                    // 질량 무시하고 즉시 속도 적용 (VelocityChange)
                    float finalForce = carImpactForce * 1.5f;
                    carRb.AddForce(flyDirection * finalForce, ForceMode.VelocityChange);

                    // 회전 주기
                    carRb.AddTorque(Random.insideUnitSphere * finalForce, ForceMode.VelocityChange);
                }

                Destroy(other.gameObject, 2.0f); // 2초 뒤 삭제
            }
            else
            {
                // --- 게임 오버 로직 ---
                if (isGameOver) return;
                ScoreManager.Instance.isCleared = false;
                UIController.Instance.EndGame();
                anim.SetTrigger("isCrashed");
                rb.constraints = RigidbodyConstraints.None;
                StartCoroutine(GameOver());
            }
        }
    }

    void TrySpawnHelicopter()
    {
        if (currentHelicopter != null) return;
        float randomValue = Random.value;
        if (randomValue <= heliSpawnChance)
        {
            currentHelicopter = Instantiate(helicopterPrefab);
        }
    }

    public void ActivateInvincibility(float duration)
    {
        // 이미 무적 상태라면 코루틴이 중복 실행되지 않도록 관리하거나, 
        // 시간을 연장하는 방식이 좋지만 여기선 단순하게 새로 시작합니다.
        StopCoroutine("InvincibilityRoutine"); // 기존 코루틴이 있다면 멈춤
        StartCoroutine(InvincibilityRoutine(duration));
    }

    // [수정됨] 모든 파츠의 색을 바꾸는 무적 코루틴
    IEnumerator InvincibilityRoutine(float duration)
    {
        isInvincible = true;
        Debug.Log("무적 상태 시작!");

        // 1. 모든 파츠를 무적 재질로 교체
        if (allRenderers != null && invincibleMaterial != null)
        {
            foreach (Renderer rend in allRenderers)
            {
                rend.material = invincibleMaterial;
            }
        }

        yield return new WaitForSeconds(duration);

        isInvincible = false;
        Debug.Log("무적 상태 종료.");

        // 2. 모든 파츠를 원래 재질로 복구
        if (allRenderers != null && originalMaterials != null)
        {
            for (int i = 0; i < allRenderers.Length; i++)
            {
                // 저장해뒀던 원래 재질로 다시 입힘
                if (allRenderers[i] != null)
                {
                    allRenderers[i].material = originalMaterials[i];
                }
            }
        }
    }

    IEnumerator Ending()
    {
        ScoreManager.Instance.isCleared = true;
        rb.useGravity = false;
        Helicopter helicopter = currentHook.transform.parent.GetComponent<Helicopter>();
        helicopter.StopChasing();
        transform.position = Vector3.MoveTowards(transform.position, currentHook.transform.position, hookPullSpeed * 0.1f * Time.deltaTime);
        yield return new WaitForSeconds(5);
    }

    IEnumerator GameOver()
    {
        isGameOver = true;
        //Time.timeScale = 0.1f;
        yield return new WaitForSecondsRealtime(3.0f);
        Time.timeScale = 0f;
    }
}