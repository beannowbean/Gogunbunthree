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
    public float diveSpeed = 10.0f;

    // 터치 시작 위치, 종료 위치, 스와이프 속도 감지
    private Vector2 touchStartPos;
    private Vector2 touchEndPos;
    private float minSwipeDistance = 50.0f;

    // 점프
    public float jumpForce = 6.0f;
    private bool isGrounded = true;

    // 바닥 tag 입력
    public string groundTag = "Tile";
    public string vanRoofTag;

    // 바닥 체크 raycast
    public float groundCheckDistance = 1.1f;
    public LayerMask groundLayer;
    public float rayOffsetY = 0.5f;
    public float rayLength = 0.501f;

    // Hook 관련 설정
    public GameObject hookPrefab;
    public Transform hookStartingPoint;
    public float hookSpeed = 10.0f;
    public bool isHooked = false;
    private GameObject currentHook;
    public LineRenderer lineRenderer;
    public int hookAngle = 30;
    public float hookPullSpeed = 30.0f;
    public bool isHelicopter = false;
    public float helicopterDistance = 4.0f;
    private bool isHelicopterInvincible = false; // 헬리콥터에서 내릴 때 무적 확인 변수

    // GameOver 관련 변수
    public bool isGameOver = false;

    // Item (무적) 관련 변수
    public bool isInvincible = false;
    public float carImpactForce = 20.0f;
    public Material invincibleMaterial;

    // coin effect 관련 변수
    public GameObject coinEffectPrefab;

    // [수정됨] 하나가 아니라 '모든' 렌더러와 재질을 저장하기 위한 배열 선언
    private Renderer[] allRenderers;
    private Material[] originalMaterials;


    // 컴포넌트 선언
    private Rigidbody rb;
    private Animator anim;

    // 튜토리얼 확인용 변수
    public bool isJump = false;
    public bool isMove = false;
    public bool isHook = false;
    public bool isControl = false;

    // 아이템 로직 관련
    private Coroutine currentInvincibilityCoroutine;

    void Awake()
    {
        if (UIController.tutorialSkip == true) isControl = true;
        else isControl = false;
    }
    void Start()
    {
        groundLayer = LayerMask.GetMask("Ground");
        currentLane = 2;
        rb = GetComponent<Rigidbody>();
        anim = GetComponent<Animator>();
        lineRenderer.enabled = false;
        anim.SetBool("isGrounded", true);

        allRenderers = GetComponentsInChildren<Renderer>();
        originalMaterials = new Material[allRenderers.Length];

        for (int i = 0; i < allRenderers.Length; i++)
        {
            originalMaterials[i] = allRenderers[i].material;
        }
        StartCoroutine(CarSound());
    }

    void Update()
    {
        if (isGameOver) return;

        CheckGround();
        DrawRope();

        // 갈고리에 걸린 상태라면?
        if (isHooked && currentHook != null)
        {
            MoveToHook(); // 갈고리 쪽으로 끌려가는 함수 실행

            if (isHelicopter)
            {

            }
            else
            {
                return;
            }
        }
        if (!isHelicopter)
        {
            transform.Translate(Vector3.forward * moveSpeed * Time.deltaTime, Space.World);

        }
        if (isControl == true) CheckInput();

        float targetX = (currentLane - 2) * laneDistance;
        Vector3 newPosition = new Vector3(targetX, transform.position.y, transform.position.z);
        Vector3 moveVector = Vector3.Lerp(transform.position, newPosition, crossSpeed * Time.deltaTime);
        transform.position = new Vector3(moveVector.x, transform.position.y, transform.position.z);
    }

    void CheckInput()
    {
        if (Input.GetKeyDown(KeyCode.LeftArrow)) ChangeLane(-1);
        if (Input.GetKeyDown(KeyCode.RightArrow)) ChangeLane(1);
        if (!isHelicopter)
        {
            if (Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.Space)) Jump();

            // 훅 발사 조건: 마우스 클릭 시 + 공중이거나 땅이어도 상관없다면 조건 제거 가능
            // 여기서는 기존 로직 유지하되, 땅에서도 쏠 수 있게 하려면 !isGrounded 제거하면 됨
            if (Input.GetMouseButtonDown(0) && !isGrounded) hookShoot();

            if (Input.GetMouseButtonUp(0) && isHooked == false) ReleaseHook();
            if (Input.GetKeyDown(KeyCode.DownArrow) && !isGrounded) QuickDive();
        }
        
        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);
            if (touch.phase == TouchPhase.Began)
            {
                touchStartPos = touch.position;
            }
            else if (touch.phase == TouchPhase.Ended)
            {
                touchEndPos = touch.position;
                if (!isGrounded) hookShoot();
                CalculateSwipeOrTap();
            }
        }
    }

    void CalculateSwipeOrTap()
    {
        float swipeDistanceX = touchEndPos.x - touchStartPos.x;
        float swipeDistanceY = touchEndPos.y - touchStartPos.y;

        bool isSwipe = Mathf.Abs(swipeDistanceX) > minSwipeDistance || Mathf.Abs(swipeDistanceY) > minSwipeDistance;

        if (isSwipe)
        {
            if (Mathf.Abs(swipeDistanceX) > Mathf.Abs(swipeDistanceY))
            {
                if (swipeDistanceX > 0) ChangeLane(1);
                else ChangeLane(-1);
            }
            else
            {
                if (!isHelicopter)
                {
                    if (swipeDistanceY > 0) Jump();
                    else if (swipeDistanceY < 0) QuickDive();
                }
            }
        }
        else
        {
            if (!isHelicopter)
            {
                if (!isGrounded) hookShoot();
                else ReleaseHook();
            }
        }
    }

    public void ChangeLane(int direction)
    {
        int targetLane = currentLane + direction;
        if (targetLane >= 1 && targetLane <= 3) currentLane = targetLane;
        SFXManager.Instance.Play("Move");
        isMove = true;
    }

    public void Jump()
    {
        if (isGrounded)
        {
            rb.AddForce(Vector3.up * jumpForce, ForceMode.VelocityChange);
            isGrounded = false;
            anim.SetBool("isGrounded", false);
            SFXManager.Instance.Play("Jump");
            isJump = true;
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

    public void hookShoot()
    {
        if (currentHook != null) return;
        Quaternion projectileRotation = Quaternion.Euler(-90f - hookAngle, transform.eulerAngles.y, 0f);
        currentHook = Instantiate(hookPrefab, hookStartingPoint.position, projectileRotation);
        Rigidbody hrb = currentHook.GetComponent<Rigidbody>();
        Vector3 shootDirection = Quaternion.Euler(-hookAngle, 0f, 0f) * transform.forward;
        hrb.velocity = shootDirection * hookSpeed;
        Hook hookScript = currentHook.GetComponent<Hook>();
        hookScript.player = this;
        SFXManager.Instance.Play("Hook");
        isHook = true;
    }

    void ReleaseHook()
    {
        // 훅 해제 시 중력을 다시 켜줍니다.
        rb.useGravity = true;

        if (currentHook != null)
        {
            Destroy(currentHook);
            currentHook = null;
        }

        isHooked = false;
        lineRenderer.enabled = false;
    }

    void MoveToHook()
    {
        rb.useGravity = false;
        rb.velocity = Vector3.zero;

        // 플레이어가 헬리콥터에서 내려올때, 좌우 방향키를 누르면 헬리콥터만 아래로 내려가는 현상 수정.
        // 무조건 헬리콥터의 Y값에서 일정 거리를 뺀 위치에서 있도록 수.
        if (isHelicopter)
        {
            Vector3 targetPos = new Vector3(
                transform.position.x,                                  // X값은 그대로
                currentHook.transform.position.y - helicopterDistance, // Y값은 헬리콥터보다 일정거리 아래
                currentHook.transform.position.z                       // Z값은 헬리콥터 따라가
            );

            // 위치로 이동
            transform.position = Vector3.MoveTowards(transform.position, targetPos, hookPullSpeed * Time.deltaTime);
        }
        else
        {
            transform.position = Vector3.MoveTowards(transform.position, currentHook.transform.position, hookPullSpeed * Time.deltaTime);

            // 갈고리의 X위치를 기반으로 currentLane을 제일 가까운 곳으로 강제 변환.
            // 갈고리에서 떨어질 때 갈고리 걸기 전 Lane으로 자동 이동되어 부자연스러운 현상을 수정.
            UpdateLanePosition(currentHook.transform.position.x);

            float distance = Vector3.Distance(transform.position, currentHook.transform.position);

            if (distance <= 2.0f)
            {
                ReleaseHook();
            }
        }
    }

    // 갈고리를 가로등에 걸었을 때, 차선을 가로등이 있는 위치로 오게 하는 함수
    void UpdateLanePosition(float xPos)
    {
        if (xPos < -1.0f) currentLane = 1;
        else if (xPos > 1.0f) currentLane = 3;
        else currentLane = 2;
    }

    public void ReleaseHelicopter()
    {
        isHelicopter = false;
        ReleaseHook();

        // 헬리콥터에서 내린 후 3초간 무적 코루틴 
        StartCoroutine(GetOffHelicopter());
    }

    // 헬리콥터에서 내린 후 3초간 깜빡이며 무적상태인 코루틴

    IEnumerator GetOffHelicopter()
    {
        isHelicopterInvincible = true;  // 무적 켜기

        float duration = 3.0f;          // 지속 시	
        float blinkSpeed = 0.2f;        // 깜빡이는 속도

        // 3초간 플레이어 모든 렌더러 깜빡이도록
        for (float t = 0; t < duration; t += blinkSpeed)
        {
            if (allRenderers != null)
            {
                foreach (Renderer r in allRenderers)
                {
                    if (r != null) r.enabled = !r.enabled;
                }
            }
            yield return new WaitForSeconds(blinkSpeed);
        }

        // 끝난 후 모습 복구
        if (allRenderers != null)
        {
            foreach (Renderer r in allRenderers)
            {
                if (r != null) r.enabled = true;
            }
        }

        isHelicopterInvincible = false; // 무적 끄기
    }

    void QuickDive()
    {
        rb.velocity = new Vector3(rb.velocity.x, -diveSpeed, rb.velocity.z);
    }

    void CheckGround()
    {
        Vector3 rayStart = transform.position + Vector3.up * rayOffsetY;
        RaycastHit hit;

        Debug.DrawRay(rayStart, Vector3.down * rayLength, Color.red);

        // 레이어 마스크 체크
        if (Physics.Raycast(rayStart, Vector3.down, out hit, rayLength, groundLayer))
        {
            // 방금 쏜 갈고리는 바닥으로 인식하지 않음
            if (currentHook != null && hit.collider.gameObject == currentHook) return;

            if (hit.collider.CompareTag(groundTag) || hit.collider.CompareTag(vanRoofTag))
            {
                isGrounded = true;
                anim.SetBool("isGrounded", true);

                if (!isHooked)
                {
                    isHooked = false;
                }
            }
        }
        else
        {
            isGrounded = false;
            anim.SetBool("isGrounded", false);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Coin"))
        {
            if (isGameOver) return;
            SFXManager.Instance.Play("Coin", 0.98f, 1.02f);
            GameObject effect = Instantiate(coinEffectPrefab, other.transform.position, Quaternion.identity);
            Destroy(effect, 1.0f);
            Destroy(other.gameObject);
            ScoreManager.Instance.AddCoin(1);
        }
        else if (other.CompareTag("Car"))
        {
            if (isInvincible)
            {
                Rigidbody carRb = other.gameObject.GetComponent<Rigidbody>();
                if (carRb != null)
                {
                    carRb.isKinematic = false;
                    carRb.useGravity = true;
                    Vector3 flyDirection = transform.forward + Vector3.up * 1.5f;
                    float finalForce = carImpactForce * 1.5f;
                    carRb.AddForce(flyDirection * finalForce, ForceMode.VelocityChange);
                    carRb.AddTorque(Random.insideUnitSphere * finalForce, ForceMode.VelocityChange);
                }
                Destroy(other.gameObject, 2.0f);
            }
            // 헬리콥터에서 내릴 때 차와 부딪히는 경우
            else if (isHelicopterInvincible)
            {
                return; // 아무 일도 일어나지 않게 리턴
            }

            else
            {
                if (isGameOver) return;
                ReleaseHook();
                ScoreManager.Instance.isCleared = false;
                anim.SetTrigger("isCrashed");
                rb.constraints = RigidbodyConstraints.None;
                StartCoroutine(GameOver());
            }
        }
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Vector3 rayStart = transform.position + Vector3.up * rayOffsetY;
        Vector3 rayEnd = rayStart + (Vector3.down * rayLength);
        Gizmos.DrawLine(rayStart, rayEnd);
        Gizmos.DrawWireSphere(rayStart, 0.05f);
        Gizmos.DrawWireSphere(rayEnd, 0.05f);
    }

    public void ActivateInvincibility(float duration)
    {
        // 1. 이미 실행 중인 무적 코루틴이 있다면 확실하게 멈춤
        if (currentInvincibilityCoroutine != null)
        {
            StopCoroutine(currentInvincibilityCoroutine);
            SFXManager.Instance.Stop("ItemTimeSound");  // 사운드 겹침 방지
        }

        // 2. 새로운 코루틴을 시작하면서, 그 정보를 변수에 저장함
        currentInvincibilityCoroutine = StartCoroutine(InvincibilityRoutine(duration));
    }

    IEnumerator InvincibilityRoutine(float duration)
    {
        isInvincible = true;
        Debug.Log("무적 상태 시작!");

        // 1. 시작: 모든 파츠를 무적 재질로 변경
        if (allRenderers != null && invincibleMaterial != null)
        {
            foreach (Renderer rend in allRenderers)
            {
                if (rend != null)
                {
                    rend.material = invincibleMaterial;
                    rend.enabled = true;
                }
            }
        }

        // --- 시간 계산 ---
        float blinkTotalTime = 3.0f;
        float safeTime = duration - blinkTotalTime;

        // 3초가 남을 때까지 대기
        if (safeTime > 0)
        {
            yield return new WaitForSeconds(safeTime);
        }

        // 3초 남음: 효과음 재생
        SFXManager.Instance.Play("ItemTimeSound");

        // --- 깜빡이는 간격 설정 (횟수 2배) ---
        List<float> blinkDelays = new List<float>();

        // 1단계: 0~2초 구간 (0.25초 간격 8번)
        for (int i = 0; i < 8; i++) blinkDelays.Add(0.25f);
        // 2단계: 2~3초 구간 (0.125초 간격 8번)
        for (int i = 0; i < 8; i++) blinkDelays.Add(0.125f);

        bool showingInvincibleMat = true; // 현재 보여주는 재질 상태

        // [수정] 순서 변경: 재질 먼저 바꾸고 -> 그 다음에 대기
        foreach (float delay in blinkDelays)
        {
            // 1. 재질 반전 (즉시 실행)
            showingInvincibleMat = !showingInvincibleMat;

            // 2. 렌더러에 적용
            if (allRenderers != null && originalMaterials != null)
            {
                for (int i = 0; i < allRenderers.Length; i++)
                {
                    Renderer rend = allRenderers[i];
                    if (rend == null) continue;

                    if (showingInvincibleMat)
                    {
                        if (invincibleMaterial != null) rend.material = invincibleMaterial;
                    }
                    else
                    {
                        if (i < originalMaterials.Length && originalMaterials[i] != null)
                        {
                            rend.material = originalMaterials[i];
                        }
                    }
                }
            }

            // 3. 정해진 시간만큼 대기 (이게 뒤로 가야 0초에 바로 바뀜)
            yield return new WaitForSeconds(delay);
        }

        // --- 종료 처리 ---
        isInvincible = false;
        Debug.Log("무적 상태 종료.");

        // 원래 재질로 복구
        if (allRenderers != null && originalMaterials != null)
        {
            for (int i = 0; i < allRenderers.Length; i++)
            {
                if (allRenderers[i] != null && i < originalMaterials.Length && originalMaterials[i] != null)
                {
                    allRenderers[i].material = originalMaterials[i];
                    allRenderers[i].enabled = true;
                }
            }
        }
        currentInvincibilityCoroutine = null;
    }

    IEnumerator GameOver()
    {
        isGameOver = true;
        SFXManager.Instance.Play("Crashed");

        // 메인메뉴, 인게임 Scene preload
        //if (UIController.Instance != null)
        //{
        //    UIController.Instance.StartPreloadScenes();
        //}

        yield return new WaitForSecondsRealtime(3.0f);
        SFXManager.Instance.Stop("Helicopter");
        Time.timeScale = 0f;
        UIController.Instance.EndGame();
    }

    IEnumerator CarSound()
    {
        yield return new WaitForSeconds(10.0f);
        while (!isGameOver)
        {
            if (Random.value <= 0.2f)
            {
                SFXManager.Instance.Play("Honk");
                yield return new WaitForSeconds(2f);
            }
            else
            {
                yield return new WaitForSeconds(2f);
            }
        }
    }
}