/*
    플레이어 가방과 모자를 바꾸는 함수 (Equip을 하면 자동으로 원래 있던거 unequip함수 불러와짐)
    EquipHat(), UnequipHat()
    EquipBag(), UnequipBag()
    싱글톤 인스턴스를 사용해서 앞에 "Player.Instance." 을 붙이면 됨
*/

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    // 스킨 장착을 위해 싱글톤 도입
    public static Player Instance;

    // 스킨 장착을 위한 변수들
    [Header("Accessory Settings")]
    // 장비를 장착시키기 위한 위치 받기
    public Transform headBone;
    public Transform backBone;
    private Texture currentBeanieSkinTexture;
    private Texture currentBagSkinTexture;

    // 기본 오프셋
    public Vector3 hatPositionOffset;
    public Vector3 hatRotationOffset;
    public Vector3 bagPositionOffset;
    public Vector3 bagRotationOffset;

    // 현재 장착된 아이템 관리
    private GameObject currentBeanie;
    private GameObject currentBag;

    // 스킨 장착 관리
    [Header("Skin Settings")]
    public SkinnedMeshRenderer playerRenderer;
    public int targetMaterialIndex = 1;

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

    // [14. Iceman] 플레이어가 게임 시작 후 7초간 움직였는지 확인하는 변수 + 중복처리 방지
    private bool hasPlayerMoved = false;
    private bool isIcemanChecked = false;

    // 바닥 tag 입력
    public string groundTag = "Tile";
    public string vanRoofTag;

    // 바닥 체크 raycast
    public float groundCheckDistance = 1.1f;
    public LayerMask groundLayer;
    public float rayOffsetY = 0.5f;
    private float rayLength = 0.6f; // van위에서 점프 딜레이 해결을 위한 바닥 인식 범위 늘리/

    // Hook 관련 설정
    public GameObject hookPrefab;
    public Transform hookStartingPoint;
    public float hookSpeed = 10.0f;
    public bool isHooked = false;
    private GameObject currentHook;
    public LineRenderer lineRenderer;
    public int hookAngle = 30;
    public float hookPullSpeed = 30.0f;
    private bool isHookVisible = false;    // 훅이 안보이도록 하는 변수
    private Coroutine visualDelayCoroutine; // 훅이 안보이게 하는 코루틴 선언

    // Helicopter 관련 변수
    public bool isHelicopter = false;
    public float helicopterDistance = 4.0f;
    private int heliCoinCount = 0;  // [5. Eagle] 헬리콥터 탄 상태에서의 코인 개수 변수
    private int heliCount = 0;  // [9. TopGun] 헬리콥터를 몇 번 탔는지 확인하는 변수
    private bool isHelicopterInvincible = false; // 헬리콥터에서 내릴 때 무적 확인 변수
    private bool isDroppingHeli = false;    // 헬리콥터에서 내려오는 동안인지 확인하는 변수

    // GameOver 관련 변수
    public bool isGameOver = false;

    // Item Star(무적) 관련 변수
    public bool isInvincible = false;
    public float carImpactForce = 20.0f;
    private bool hitCar = false;   // [11. Gentleman] star 아이템을 먹은 동안 차에 부딪쳤는지 확인하는 변수
    private int carHitCount = 0;    // [12. Wrecker] star 아이템을 먹은 동안 차를 날린 개수 확인 변수
    public int starCount = 0;  // [13. Superstar] star 아이템 먹은 개수 확인 변수 (star.cs에서)
    public Material invincibleMaterial;

    // coin effect 관련 변수 
    public GameObject coinEffectPrefab;

    // Magnet 관련 변수
    public bool isMagnetActive = false;
    private int magnetCoinCount = 0;    // [4. TreasureHunter] 자석아이템 먹고 코인 갯수 세는 변수
    private Coroutine magnetCoroutine;
    public GameObject magnetEffectPrefab;       // 자석 아이템 프리펩
    private GameObject currentMagnetEffect;     // 현재 자석 코루틴 활성화 중인지 확인 변수

    // 일시정지를 확인하는 변수
    public bool isResuming = false;

    // 플레이어 렌더러와 재질을 저장하기 위한 배열 선언
    private Renderer[] allRenderers;
    private Material[][] originalMaterials;

    // 컴포넌트 선언
    private Rigidbody rb;
    private Animator anim;

    // 튜토리얼 확인용 변수
    public bool isControl = false;

    // 아이템 로직 관련
    private Coroutine currentInvincibilityCoroutine;

    // [2. Dumb] 업적 달성을 위한 변수
    private float gameStartTime;

    // [6. Bunny] 퀵다이브를 한 시간을 기록하는 리스트
    private List<float> quickDiveTime = new List<float>();

    void Awake()
    {
        // 싱글톤 초기화
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        if (UIController.tutorialSkip == true) isControl = true;
        else isControl = false;
    }
    void Start()
    {
        starCount = 0;  // [13. Superstar] 별을 먹은 개수 초기화
        heliCount = 0;  // [9. TopGun] 헬리콥터에 탄 횟수 초기화

        // [14. Iceman] 변수 초기화
        hasPlayerMoved = false;
        isIcemanChecked = false;

        groundLayer = LayerMask.GetMask("Ground");
        currentLane = 2;
        rb = GetComponent<Rigidbody>();
        anim = GetComponent<Animator>();
        lineRenderer.enabled = false;
        anim.SetBool("isGrounded", true);

        allRenderers = GetComponentsInChildren<Renderer>();
        originalMaterials = new Material[allRenderers.Length][];

        for (int i = 0; i < allRenderers.Length; i++)
        {
            originalMaterials[i] = allRenderers[i].materials;
        }

        // [2. Dumb] 게임 시작 시점의 시간 저장
        gameStartTime = Time.time;

        StartCoroutine(CarSound());
    }

    void Update()
    {
        // 일시정지와 일시정지 재개중인 3초간 아무 실행을 안하도록
        if (isGameOver || Time.timeScale == 0.0f || isResuming) return;

        // [14. Iceman] 아직 업적 달성이 안되어 있고 플레이어가 게임오버가 안되었다면 업적 확인
        if (!isIcemanChecked && !isGameOver)
        {
            // 플레이어가 한 번이라도 움직였으면 -> 업적 실패 (체크 종료)
            if (hasPlayerMoved)
            {
                isIcemanChecked = true;
            }
            // 아직 안 움직였는데 7초가 지났다면? -> 업적 성공
            else if (Time.time - gameStartTime >= 7.0f)
            {
                if (PlayerAchivementList.Instance != null)
                {
                    PlayerAchivementList.Instance.Iceman();
                }
                isIcemanChecked = true; // 체크 완료
            }
        }

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

    // 스와이프 중복 인식을 막기 위한 변수
    private bool hasSwiped = false;

    // 모바일 시 갈고리와 스와이프로직 충돌 수정
    void CheckInput()
    {
        if (Time.timeScale == 0f || isResuming) return;

        // [14. Iceman] 움직였다고 체크 안될 때만 검사
        if (!hasPlayerMoved)
        {
            // 키보드나 마우스, 터치 입력이 하나라도 들어오면
            if (Input.anyKeyDown || Input.touchCount > 0)
            {
                hasPlayerMoved = true; // "움직였음" 표시
            }
        }

        // 키보드 입력 유지
        if (Input.GetKeyDown(KeyCode.LeftArrow)) ChangeLane(-1);
        if (Input.GetKeyDown(KeyCode.RightArrow)) ChangeLane(1);

        if (!isHelicopter)
        {
            if (Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.Space)) Jump();
            if (Input.GetKeyDown(KeyCode.DownArrow) && !isGrounded) QuickDive();

            if (Input.GetMouseButtonDown(0) && !isGrounded) hookShoot();
            if (Input.GetMouseButtonUp(0)) ReleaseHook();
        }

        // 모바일 입력 수정
        // 터치 후 갈고리는 나가지만 일정거리 움직였다면 바로 끊고 스와이프
        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);

            switch (touch.phase)
            {
                case TouchPhase.Began:
                    touchStartPos = touch.position;
                    hasSwiped = false; // 스와이프 상태 초기화

                    // 터치하자마자 공중이라면 갈고리 발사
                    if (!isHelicopter && !isGrounded)
                    {
                        hookShoot();
                    }
                    break;

                case TouchPhase.Moved:
                    // 이미 스와이프 되었다면 리턴
                    if (hasSwiped) return;

                    Vector2 currentSwipe = touch.position - touchStartPos;

                    // 터치 후 일정거리 움직였다면 스와이프로 판단
                    if (currentSwipe.magnitude > minSwipeDistance)
                    {
                        // 갈고리 취소
                        ReleaseHook();

                        // 스와이프 실행
                        if (Mathf.Abs(currentSwipe.x) > Mathf.Abs(currentSwipe.y))
                        {
                            if (currentSwipe.x > 0) ChangeLane(1);
                            else ChangeLane(-1);
                        }
                        else
                        {
                            if (!isHelicopter)
                            {
                                if (currentSwipe.y > 0) Jump();
                                else QuickDive();
                            }
                        }

                        // 중복 실행 버그 방지
                        hasSwiped = true;
                    }
                    break;

                case TouchPhase.Ended:
                case TouchPhase.Canceled:
                    // 손을 떼면 무조건 갈고리 해제
                    ReleaseHook();
                    hasSwiped = false;
                    break;
            }
        }
    }

    public void ChangeLane(int direction)
    {
        int targetLane = currentLane + direction;
        if (targetLane >= 1 && targetLane <= 3) currentLane = targetLane;
        SFXManager.Instance.Play("Move");
    }

    public void Jump()
    {
        if (isGrounded)
        {
            rb.AddForce(Vector3.up * jumpForce, ForceMode.VelocityChange);
            isGrounded = false;
            anim.SetBool("isGrounded", false);
            SFXManager.Instance.Play("Jump");
        }
    }

    void DrawRope()
    {
        // 0.1초가 지난 후에만 그리도록 
        if (!isHookVisible)
        {
            lineRenderer.enabled = false;
            return;
        }

        if (currentHook != null)
        {
            if (!lineRenderer.enabled) lineRenderer.enabled = true;

            // 헬기 랜딩에서 지점을 없앴기에 다시 재선
            if (lineRenderer.positionCount != 2)
            {
                lineRenderer.positionCount = 2;
            }
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
        if (isDroppingHeli) return;  // 헬기에서 내려오는 동안에는 갈고리 발사 금지 

        if (currentHook != null) return;
        Quaternion projectileRotation = Quaternion.Euler(-90f - hookAngle, transform.eulerAngles.y, 0f);
        currentHook = Instantiate(hookPrefab, hookStartingPoint.position, projectileRotation);
        Rigidbody hrb = currentHook.GetComponent<Rigidbody>();
        Vector3 shootDirection = Quaternion.Euler(-hookAngle, 0f, 0f) * transform.forward;
        hrb.velocity = shootDirection * hookSpeed;
        Hook hookScript = currentHook.GetComponent<Hook>();
        hookScript.player = this;

        // 갈고리 안보이도록 설정
        isHookVisible = false;
        Renderer hookRenderer = currentHook.GetComponent<Renderer>();
        if (hookRenderer != null) hookRenderer.enabled = false;
        lineRenderer.enabled = false;
        if (visualDelayCoroutine != null) StopCoroutine(visualDelayCoroutine);
        visualDelayCoroutine = StartCoroutine(ShowHookDelay(0.1f));
    }

    // 0.1초간 훅이 안보이는 코루틴
    IEnumerator ShowHookDelay(float delay)
    {
        // 0.1초 대기
        yield return new WaitForSeconds(delay);

        // 0.1초 뒤에도 갈고리가 있다면 
        if (currentHook != null)
        {
            isHookVisible = true; // 줄을 그리도록 변수 선언

            // 갈고리 다시 보이게 켜기 
            Renderer hookRenderer = currentHook.GetComponent<Renderer>();
            if (hookRenderer != null) hookRenderer.enabled = true;

            SFXManager.Instance.Play("Hook");
        }
    }

    void ReleaseHook()
    {
        // 안보이는 코루틴 종료
        if (visualDelayCoroutine != null) StopCoroutine(visualDelayCoroutine);

        isHookVisible = false; // 안보이도록

        isHooked = false;
        rb.useGravity = true;
        lineRenderer.enabled = false;

        if (currentHook != null)
        {
            Destroy(currentHook);
            currentHook = null;
        }
    }

    void MoveToHook()
    {
        rb.useGravity = false;
        rb.velocity = Vector3.zero;

        // 플레이어가 헬리콥터에 갈고리를 걸어 이동할 때, 뚝뚝 끊기며 이동하는 현상 수정
        // transform으로 이동하던 로직을 Lerp를 사용한 벡터로 이동하도록.
        if (isHelicopter)
        {
            // [10. HeliVIP] 아이템을 먹은 상태로 헬리콥터에 탑승했을 시 업적 달성
            if (isMagnetActive || isInvincible)
            {
                if (PlayerAchivementList.Instance != null)
                {
                    PlayerAchivementList.Instance.HeliVIP();
                }
            }

            Vector3 targetPos = new Vector3(
                transform.position.x,                                  // X값은 그대로
                currentHook.transform.position.y - helicopterDistance, // Y값은 헬리콥터보다 일정거리 아래
                currentHook.transform.position.z                       // Z값은 헬리콥터 따라가
            );

            // MoveTowards에서 Lerp로 이동하도록.
            // 15.0f 의 속도로 지정.
            transform.position = Vector3.Lerp(transform.position, targetPos, Time.deltaTime * 15.0f);
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
        // [5. Eagle] 헬리콥터에서 내리면 카운트 초기화
        heliCoinCount = 0;

        isHelicopter = false;

        // 플레이어가 헬리콥터에서 내린 후 갈고리와 로프가 헬리콥터에서 안 사라지고 남아있는 현상 수정을 위한 release hook 로직 직접 적용
        isHooked = false;

        rb.velocity = Vector3.zero;
        rb.useGravity = true;

        if (currentHook != null)
        {
            Destroy(currentHook);
            currentHook = null;
        }

        if (lineRenderer != null)
        {
            lineRenderer.enabled = false;
            lineRenderer.positionCount = 0;
        }

        // [9. TopGun] 헬리콥터에 탈때마다 카운, 3번 이상이면 업적 달성
        heliCount++;
        if (heliCount >= 3)
        {
            if (PlayerAchivementList.Instance != null)
            {
                PlayerAchivementList.Instance.TopGun();
            }
        }

        // 헬기에서 내려오는 중임을 선언
        isDroppingHeli = true;

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

    public void QuickDive() // 참조 가능하게 수정
    {
        SFXManager.Instance.Play("Move");   // 퀵다이브 시 효과음 재생
        rb.velocity = new Vector3(rb.velocity.x, -diveSpeed, rb.velocity.z);

        // [6. Bunny] 5초 안에 3번 퀵다이브를 하였는지 체크
        float currentTime = Time.time;
        quickDiveTime.Add(currentTime);

        // [6. Bunny] 5초가 지나면 리스트에서 삭제
        for (int i = quickDiveTime.Count - 1; i >= 0; i--)
        {
            if (quickDiveTime[i] < currentTime - 5.0f)
            {
                quickDiveTime.RemoveAt(i);
            }
        }

        // [6. Bunny] 리스트에 3개 이상 존재한다면 업적 달성
        if (quickDiveTime.Count >= 3)
        {
            if (PlayerAchivementList.Instance != null)
            {
                PlayerAchivementList.Instance.Bunny();
            }

            // 중복 해제
            quickDiveTime.Clear(); 
        }

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
                // 헬리콥터에서 내려서 바닥에 닿았는지 확인
                if (isDroppingHeli) isDroppingHeli = false;
            }
        }
        else
        {
            isGrounded = false;
            anim.SetBool("isGrounded", false);
        }
    }

    // 자석 아이템 먹었을 때 호출되는 함수
    public void ActivateMagnet(float duration)
    {
        if (magnetCoroutine != null) StopCoroutine(magnetCoroutine);
        if (currentMagnetEffect != null) Destroy(currentMagnetEffect);  // 아이템 겹침 현상 방지
        magnetCoinCount = 0;    // [4. TreasureHunter] 자석 아이템 먹을 때마다 초기화
        magnetCoroutine = StartCoroutine(MagnetRoutine(duration));
    }

    // 자석 활성화 코루틴
    IEnumerator MagnetRoutine(float duration)
    {
        isMagnetActive = true;

        // 이펙트 생성 (플레이어를 부모로 하여 따라다니도록)
        currentMagnetEffect = Instantiate(magnetEffectPrefab, transform.position, Quaternion.identity, transform);

        // 지속 시간만큼 대기
        yield return new WaitForSeconds(duration);

        // 이펙트 삭제
        Destroy(currentMagnetEffect);

        isMagnetActive = false;
    }

    // 모자 프리펩을 장착하는 함수
    public void EquipBeanie(GameObject beaniePrefab)
    {
        // 기존 모자를 삭제
        UnequipBeanie();

        // 새 모자 생성 및 장착
        if (beaniePrefab != null && headBone != null)
        {
            currentBeanie = Instantiate(beaniePrefab);
            currentBeanie.transform.SetParent(headBone); // 머리 뼈에 부착

            // 위치 회전값 초기화 및 오프셋 적용
            currentBeanie.transform.localPosition = hatPositionOffset;
            currentBeanie.transform.localRotation = Quaternion.Euler(hatRotationOffset);
            currentBeanie.transform.localScale = Vector3.one;

            // 스킨 적용
            if (currentBeanieSkinTexture != null)
            {
                ApplyTexture(currentBeanie, currentBeanieSkinTexture);
            }
        }

    }

    // 모자 벗기 함수
    public void UnequipBeanie()
    { 
        if (currentBeanie != null)
        {
            Destroy(currentBeanie);
            currentBeanie = null;
        }
    }

    // 가방 프리펩을 장착하는 함수
    public void EquipBag(GameObject bagPrefab)
    {
        // 기존 가방 삭제 
        UnequipBag();

        if (bagPrefab != null && backBone != null)
        {
            currentBag = Instantiate(bagPrefab);
            currentBag.transform.SetParent(backBone); // 척추 뼈에 부착

            // 위치 회전값 초기화 및 오프셋 적용
            currentBag.transform.localPosition = bagPositionOffset;
            currentBag.transform.localRotation = Quaternion.Euler(bagRotationOffset);
            currentBag.transform.localScale = Vector3.one;

            // 스킨 적용
            if (currentBagSkinTexture != null)
            {
                ApplyTexture(currentBag, currentBagSkinTexture);
            }
        }
    }

    // 가방 벗기 함수
    public void UnequipBag()
    {
        if (currentBag != null)
        {
            Destroy(currentBag);
            currentBag = null;
        }
    }

    // 외부에서 texture를 받아서 albedo를 바꾸는 함수 
    public void ChangeSkinTexture(Texture newTexture)
    {
        if (playerRenderer != null && newTexture != null)
        {
            Material[] mats = playerRenderer.materials;
            playerRenderer.material.mainTexture = newTexture;

            if (mats.Length > targetMaterialIndex)
            {
                mats[targetMaterialIndex].mainTexture = newTexture;
            }
            else
            {
                Debug.LogError("오류: 머티리얼 Element 번호가 존재하지 않습니다!");
            }
            playerRenderer.materials = mats;
        }
    }

    // 비니 스킨 변경 함수
    public void ChangeBeanieSkin(Texture newTexture)
    {
        // 1. 선택된 스킨을 기억해둠 (나중에 모자 갈아껴도 유지되게)
        currentBeanieSkinTexture = newTexture;

        // 2. 현재 모자가 머리에 있다면 즉시 색깔 변경
        if (currentBeanie != null)
        {
            ApplyTexture(currentBeanie, newTexture);
        }
    }

    // 가방 스킨 변경 함수
    public void ChangeBagSkin(Texture newTexture)
    {
        currentBagSkinTexture = newTexture;

        if (currentBag != null)
        {
            ApplyTexture(currentBag, newTexture);
        }
    }

    // 가방과 비니 스킨 텍스쳐 입히는 함수
    private void ApplyTexture(GameObject obj, Texture texture)
    {
        if (obj == null || texture == null) return;

        Renderer r = obj.GetComponent<Renderer>();
        if (r == null) r = obj.GetComponentInChildren<Renderer>();

        if (r != null)
        {
            r.material.mainTexture = texture;
        }
    }

    // 갈고리 로프 머터리얼 변경 함수
    public void ChangeRopeMaterial(Material newMaterial)
    {
        if (lineRenderer != null && newMaterial != null)
        {
            lineRenderer.material = newMaterial;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Coin"))
        {
            // 게임 오버시 코인과 부딪혀도 반응없도록
            if (isGameOver)
            {
                return;
            }

            SFXManager.Instance.Play("Coin", 0.98f, 1.02f);
            GameObject effect = Instantiate(coinEffectPrefab, other.transform.position, Quaternion.identity);
            Destroy(effect, 1.0f);
            other.gameObject.SetActive(false);
            ScoreManager.Instance.AddCoin(1);

            // [4. TreasureHunter] 자석 아이템을 먹고 코인 30개 이상 먹었을 시 업적 달성
            if (isMagnetActive)
            {
                // [4. TreasureHunter] 코인 먹을 때마다 1씩 더하기
                magnetCoinCount++;

                // [4. TreasureHunter] 30개 이상 먹을 시 업적달성
                if (magnetCoinCount >= 30)
                {
                    if (PlayerAchivementList.Instance != null)
                    {
                        PlayerAchivementList.Instance.TreasureHunter();
                    }
                }
            }

            // [5. Eagle] 헬리콥터에 탄 상태로 코인 30개 이상 먹었을 시 업적 달성
            if (isHelicopter)
            {
                // [5. Eagle] 코인 먹을 때마다 1씩 더하기
                heliCoinCount++;

                // [5. Eagle] 30개 이상 먹을 시 업적달성
                if (heliCoinCount >= 30)
                {
                    if (PlayerAchivementList.Instance != null)
                    {
                        PlayerAchivementList.Instance.TreasureHunter();
                    }
                }
            }
        }
        else if (other.CompareTag("Car"))
        {
            if (isInvincible)
            {
                hitCar = true;  // [11. Gentleman] 무적인 상태로 차에 부딪쳤다면 true

                carHitCount++;  // [12. Wrecker] 차에 부딪힌 카운트 추가

                // [12. Wrecker] 차를 20대 이상 날렸다면 업적 달
                if (carHitCount >= 20)
                {
                    if (PlayerAchivementList.Instance != null)
                    {
                        PlayerAchivementList.Instance.Wrecker();
                    }
                }

                // 중복 충돌 판정 버그 수정
                Rigidbody carRb = other.gameObject.GetComponent<Rigidbody>();

                // 만약 차가 날아가고있다면 리턴
                if (carRb != null && carRb.useGravity == true)
                {
                    return;
                }

                // 별을 먹은 상태로 차를 튕겨낼 시 효과음 재생.
                SFXManager.Instance.Play("CarBounceOff");

                // 차 날리기 점수 추가
                ScoreManager.Instance.AddCarKnockScore();

                if (carRb != null)
                {
                    carRb.isKinematic = false;
                    carRb.useGravity = true;
                    Vector3 flyDirection = transform.forward + Vector3.up * 1f;
                    float finalForce = carImpactForce * 1.5f;
                    carRb.AddForce(flyDirection * finalForce, ForceMode.VelocityChange);
                    carRb.AddTorque(Random.insideUnitSphere * finalForce, ForceMode.VelocityChange);
                }
                StartCoroutine(DeActiveAfterSeconds(other.gameObject, 2.0f));

            }
            // 헬리콥터에서 내릴 때 차와 부딪히는 경우
            else if (isHelicopterInvincible)
            {
                return; // 아무 일도 일어나지 않게 리턴
            }

            else
            {
                if (isGameOver) return;

                // [3. Hit-And-Run] 치여서 죽은게 깜빡이 차라면 업적 달
                if (other.gameObject.name.Contains("MoveCar"))
                {
                    if (PlayerAchivementList.Instance != null)
                    {
                        PlayerAchivementList.Instance.HitAndRun();
                    }
                }

                ReleaseHook();
                anim.SetTrigger("isCrashed");
                rb.constraints = RigidbodyConstraints.None;
                StartCoroutine(GameOver());
            }
        }
    }

    IEnumerator DeActiveAfterSeconds(GameObject obj, float seconds)
    {
        yield return new WaitForSeconds(seconds);
        obj.SetActive(false);
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

        hitCar = false;     // [11. Gentleman] 차에 부딪쳤는지 확인하는 변수 초기화
        carHitCount = 0;    //[12. Wrecker] 차를 날린 횟수 초기화

        // 1. 시작: 모든 파츠를 무적 재질로 변경
        if (allRenderers != null && invincibleMaterial != null)
        {
            foreach (Renderer rend in allRenderers)
            {
                if (rend != null)
                {
                    int matCount = rend.materials.Length;

                    // 노란색 스킨 적용할 부분 배열 생성
                    Material[] newMats = new Material[matCount];
                    for (int j = 0; j < matCount; j++)
                    {
                        newMats[j] = invincibleMaterial;
                    }

                    // 적용
                    rend.materials = newMats;
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

        // 
        foreach (float delay in blinkDelays)
        {
            showingInvincibleMat = !showingInvincibleMat;

            if (allRenderers != null && originalMaterials != null)
            {
                for (int i = 0; i < allRenderers.Length; i++)
                {
                    Renderer rend = allRenderers[i];
                    if (rend == null) continue;

                    if (showingInvincibleMat)
                    {
                        // 노란색으로 덮어쓰기
                        if (invincibleMaterial != null)
                        {
                            Material[] newMats = new Material[rend.materials.Length];
                            for (int j = 0; j < newMats.Length; j++) newMats[j] = invincibleMaterial;
                            rend.materials = newMats;
                        }
                    }
                    else
                    {
                        // 복구하기
                        if (i < originalMaterials.Length && originalMaterials[i] != null)
                        {
                            rend.materials = originalMaterials[i];
                        }
                    }
                }
            }
            yield return new WaitForSeconds(delay);
        }

        // [11. Gentleman] 게임오버가 아니면서 차를 한 번도 안쳐서 hitCar가 false라면 업적달성
        if (!isGameOver && !hitCar)
        {
            if (PlayerAchivementList.Instance != null)
            {
                PlayerAchivementList.Instance.Gentleman();
            }
        }

        // --- 종료 처리 ---
        isInvincible = false;

        // 원래 재질로 복구
        if (allRenderers != null && originalMaterials != null)
        {
            for (int i = 0; i < allRenderers.Length; i++)
            {
                if (allRenderers[i] != null && i < originalMaterials.Length && originalMaterials[i] != null)
                {
                    allRenderers[i].materials = originalMaterials[i];
                    allRenderers[i].enabled = true;
                }
            }
        }
        currentInvincibilityCoroutine = null;
    }

    IEnumerator GameOver()
    {
        isGameOver = true;

        // [7. Bruh] 아이템 효과가 켜져있는 상태에서 죽었는지 체크 후 업적 달성
        if (isMagnetActive || isInvincible || isHelicopter)
        {
            if (PlayerAchivementList.Instance != null)
            {
                PlayerAchivementList.Instance.Bruh();
            }
        }

        // [2. Dumb] 게임시작 10초안에 죽었는지 체크
        if (Time.time - gameStartTime <= 10.0f)
        {
            // [2. Dumb] 업적 호출
            if (PlayerAchivementList.Instance != null)
            {
                PlayerAchivementList.Instance.Dumb();
            }
        }

        // 게임오버시 자석 아이템 종료
        if (isMagnetActive)
        {
            if (magnetCoroutine != null) StopCoroutine(magnetCoroutine); // 시간 체크 중단
            if (currentMagnetEffect != null) Destroy(currentMagnetEffect); // 자석 이펙트 삭제
            isMagnetActive = false; // 상태 끄기
        }

        // 게임오버시 스타 아이템 종료
        if (isInvincible)
        {
            if (currentInvincibilityCoroutine != null) StopCoroutine(currentInvincibilityCoroutine);

            if (allRenderers != null && originalMaterials != null)
            {
                for (int i = 0; i < allRenderers.Length; i++)
                {
                    if (allRenderers[i] != null && i < originalMaterials.Length && originalMaterials[i] != null)
                    {
                        allRenderers[i].materials = originalMaterials[i];
                        allRenderers[i].enabled = true;
                    }
                }
            }

        }
        ScoreManager.Instance.StopScoring();
        SFXManager.Instance.Play("Crashed");
        // 게임오버 루틴에서 일시정지시 버그 수정
        UIController.Instance.pauseButton.SetActive(false);

        yield return new WaitForSecondsRealtime(3.0f);
        SFXManager.Instance.StopAll();  // 게임 오버시에도 소리가 들리는 현상 수정
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