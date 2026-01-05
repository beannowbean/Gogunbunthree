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

    // 헬리콥터 관련 변수
    public GameObject helicopterPrefab;
    public float heliSpawnChance = 0.05f;
    private GameObject currentHelicopter;
    public bool isEnding = false;

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

    void Awake()
    {
        if (UIController.tutorialSkip == true) isControl = true;
        else isControl = false;
    }
    void Start()
    {
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
            return;       // [중요] 아래쪽 이동/입력 코드는 실행하지 않음 (return)
        }

        transform.Translate(Vector3.forward * moveSpeed * Time.deltaTime, Space.World);

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
        if (Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.Space)) Jump();

        // 훅 발사 조건: 마우스 클릭 시 + 공중이거나 땅이어도 상관없다면 조건 제거 가능
        // 여기서는 기존 로직 유지하되, 땅에서도 쏠 수 있게 하려면 !isGrounded 제거하면 됨
        if (Input.GetMouseButtonDown(0) && !isGrounded) hookShoot();

        if (Input.GetMouseButtonUp(0) && isHooked == false) ReleaseHook();
        if (Input.GetKeyDown(KeyCode.DownArrow) && !isGrounded) QuickDive();

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
                if (swipeDistanceY > 0) Jump();
                else if (swipeDistanceY < 0) QuickDive();
            }
        }
        else
        {
            if (!isGrounded) hookShoot();
            else ReleaseHook();
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
        // [수정됨] 훅으로 이동 중에는 중력을 꺼서 부드럽게 날아가게 함
        rb.useGravity = false;
        // 기존 속도 제거 (관성 방지)
        rb.velocity = Vector3.zero;

        if (isEnding) StartCoroutine(Ending());
        else
        {
            // 바닥에 닿아 있어도 강제로 hook 위치로 당김
            transform.position = Vector3.MoveTowards(transform.position, currentHook.transform.position, hookPullSpeed * Time.deltaTime);

            float distance = Vector3.Distance(transform.position, currentHook.transform.position);
            if (distance <= 2) ReleaseHook();
        }
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

                // [수정됨] ★핵심★: 바닥에 닿았다고 해서 무조건 훅을 끊지 않습니다.
                // 훅이 걸려있지 않을 때만 훅 상태를 false로 만듭니다.
                // 즉, 갈고리에 매달려 바닥을 질질 끌려가는 상황에서도 훅이 유지됩니다.
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
            TrySpawnHelicopter();
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

    void TrySpawnHelicopter()
    {
        if (currentHelicopter != null) return;
        float randomValue = Random.value;
        if (randomValue <= heliSpawnChance)
        {
            currentHelicopter = Instantiate(helicopterPrefab);
            SFXManager.Instance.Play("Helicopter");
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
        StopCoroutine("InvincibilityRoutine");
        StartCoroutine(InvincibilityRoutine(duration));
    }

    IEnumerator InvincibilityRoutine(float duration)
    {
        isInvincible = true;
        Debug.Log("무적 상태 시작!");

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

        float blinkDuration = 3.0f;
        float safeTime = duration - blinkDuration;

        if (safeTime > 0) yield return new WaitForSeconds(safeTime);
        else blinkDuration = duration;

        float blinkTimer = 0f;
        bool showingInvincibleMat = true;

        while (blinkTimer < blinkDuration)
        {
            yield return new WaitForSeconds(0.15f);
            blinkTimer += 0.15f;
            showingInvincibleMat = !showingInvincibleMat;

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
        }

        isInvincible = false;
        Debug.Log("무적 상태 종료.");

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
    }

    IEnumerator Ending()
    {
        ScoreManager.Instance.isCleared = true;
        rb.useGravity = false;
        Helicopter helicopter = currentHook.transform.parent.GetComponent<Helicopter>();
        helicopter.StopChasing();
        transform.position = Vector3.MoveTowards(transform.position, currentHook.transform.position, hookPullSpeed * 0.1f * Time.deltaTime);
        yield return new WaitForSeconds(5);
        SFXManager.Instance.Play("Clear");
        SFXManager.Instance.Stop("Helicopter");
        UIController.Instance.EndGame();
        currentHelicopter.SetActive(false);
    }

    IEnumerator GameOver()
    {
        isGameOver = true;
        SFXManager.Instance.Play("Crashed");
        yield return new WaitForSecondsRealtime(3.0f);
        SFXManager.Instance.Stop("Helicopter");
        Time.timeScale = 0f;
        UIController.Instance.EndGame();
        if (currentHelicopter != null)
        {
            currentHelicopter.SetActive(false);
        }
    }

    IEnumerator CarSound()
    {
        yield return new WaitForSeconds(10.0f);
        while (!isGameOver)
        {
            if (Random.value <= 0.4f)
            {
                SFXManager.Instance.Play("Honk");
                yield return new WaitForSeconds(2f);
            }
            else
            {
                yield return new WaitForSeconds(1f);
            }
        }
    }
}