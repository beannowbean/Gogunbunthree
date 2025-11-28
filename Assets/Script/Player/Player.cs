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

    //Hook 관련 설정
    public GameObject hookPrefab;
    public Transform hookStartingPoint;
    public float hookSpeed = 10.0f;
    public bool isHooked = false;
    private GameObject currentHook;
    public LineRenderer lineRenderer;
    public int hookAngle = 30;
    public float hookPullSpeed = 30.0f;

    //헬리콥터 관련 변수
    public GameObject helicopterPrefab;
    public float heliSpawnChance = 0.05f;
    private GameObject currentHelicopter;
    public bool isEnding = false;

    //GameOver관련변수
    private bool isGameOver = false;

    // 컴포넌트 선언
    private Rigidbody rb;
    private Animator anim;

    // Start is called before the first frame update
    void Start()
    {
        currentLane = 2;
        rb = GetComponent<Rigidbody>();
        anim = GetComponent<Animator>();
        lineRenderer.enabled = false;
        anim.SetBool("isGrounded", true);

    }

    // Update is called once per frame
    void Update()
    {
        if (isGameOver) return;
        if (isHooked && currentHook != null) MoveToHook();
        // Space.world는 캐릭터의 방향과 관계없이 +로 이동하도
        transform.Translate(Vector3.forward * moveSpeed * Time.deltaTime, Space.World);

        //차선 이동 
        CheckInput();
        float targetX = (currentLane - 2) * laneDistance;
        Vector3 newPosition = new Vector3(targetX, transform.position.y, transform.position.z);
        Vector3 moveVector = Vector3.Lerp(transform.position, newPosition, crossSpeed * Time.deltaTime);
        transform.position = new Vector3(moveVector.x, transform.position.y, transform.position.z);
        DrawRope();
    }

    void CheckInput()
    {
        // PC에서는 방향키로 스와이프 이동
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

                if (!isGrounded)
                {
                    hookShoot();
                }
            }
            else if (touch.phase == TouchPhase.Ended)
            {
                touchEndPos = touch.position;
                CalculateSwipe();

                ReleaseHook();
            }
        }

    }

    //스와이프 감지
    void CalculateSwipe()
    {
        //터치 이동 거리 계산
        float swipeDistanceX = touchEndPos.x - touchStartPos.x;
        float swipeDistanceY = touchEndPos.y - touchStartPos.y;

        //스와이프가 최소 스와이프 거리보다 작으면 반환
        if (Mathf.Abs(swipeDistanceX) < minSwipeDistance && Mathf.Abs(swipeDistanceY) < minSwipeDistance) return;

        // 스와이프 시 가로가 더 길면 가로스와이프 세로가 더 길면 세로스와이프
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

    //차선을 오른쪽 혹은 왼쪽으로 이동
    void ChangeLane(int direction)
    {
        int targetLane = currentLane + direction;

        //차선의 범위를 1~3차선으로 제한
        if (targetLane >= 1 && targetLane <= 3)
        {
            currentLane = targetLane;
        }
    }

    void Jump()
    {
        if (isGrounded)
        {
            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
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

    // 갈고리 발사 메소드
    void hookShoot()
    {
        // 갈고리 중복 발사 제어
        if (currentHook != null) return;

        // 갈고리 생성,
        Quaternion projectileRotation = Quaternion.Euler(-90f - hookAngle, transform.eulerAngles.y, 0f);
        currentHook = Instantiate(hookPrefab, hookStartingPoint.position, projectileRotation);
        Rigidbody hrb = currentHook.GetComponent<Rigidbody>();
        Vector3 shootDirection = Quaternion.Euler(-hookAngle, 0f, 0f) * transform.forward;

        hrb.velocity = shootDirection * hookSpeed;

        Hook hookScript = currentHook.GetComponent<Hook>();
        hookScript.player = this;
    }

    // 갈고리 해제 메소드
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
        if (isEnding)
        {
            StartCoroutine(Ending());
        }
        else
        {
            transform.position = Vector3.MoveTowards(transform.position, currentHook.transform.position, hookPullSpeed * Time.deltaTime);
            float distance = Vector3.Distance(transform.position, currentHook.transform.position);
            if (distance <= 2)
            {
                ReleaseHook();
            }
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag(groundTag))
        {
            isGrounded = true;
            anim.SetBool("isGrounded", true);
            isHooked = false;
        }
        else if (collision.collider.CompareTag("Car"))
        {
            anim.SetTrigger("isCrashed");
            rb.constraints = RigidbodyConstraints.None;
            StartCoroutine(GameOver());
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Coin"))
        {
            Destroy(other.gameObject);

            TrySpawnHelicopter();
        }
    }

    void TrySpawnHelicopter()
    {

        if (currentHelicopter != null)
        {
            return;
        }

        // 2. 확률 계산
        float randomValue = Random.value;
        if (randomValue <= heliSpawnChance)
        {
            // 3. 헬리콥터 생성 (위치는 헬리콥터 스크립트가 Start에서 알아서 잡음)
            currentHelicopter = Instantiate(helicopterPrefab);
        }
    }

    void QuickDive()
    {
        // 현재의 전진/좌우 속도는 유지하되(rb.velocity.x, z), 
        // 수직 속도(y)만 강제로 마이너스로 설정하여 바닥으로 꽂음
        rb.velocity = new Vector3(rb.velocity.x, -diveSpeed, rb.velocity.z);

        // (선택사항) 만약 'Drop' 같은 전용 애니메이션이 있다면 여기서 트리거 실행
        // anim.SetTrigger("Dive"); 
    }

    IEnumerator Ending()
    {
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
