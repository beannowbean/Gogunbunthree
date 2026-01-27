using System.Collections;
using UnityEngine;

/// <summary>
/// 튜토리얼 스크립트
/// </summary>
public class Tutorial : MonoBehaviour
{
    public GameObject[] tutorialObject; // 튜토리얼 관련 UI 오브젝트
    public bool isPaused = false;   // 튜토리얼 일시정지 상태
    public bool isTutorialEnd = false; // 튜토리얼 종료 여부 (외부에서 튜토리얼 끝났는지 확인하려면 이 변수 사용)

    // 내부 변수
    Player player;  // 플레이어 참조
    Vector2 touchStartPos;  // 터치 시작 위치
    Vector2 touchEndPos;    // 터치 종료 위치
    float minSwipeDistance = 50.0f; // 스와이프 속도 감지
    int tutorialStage = 0;  // 튜토리얼 단계
    bool inputUsed = false; // 입력 사용 여부

    void Start() 
    {
        // 모든 튜토리얼 오브젝트 비활성화
        for(int i = 0; i < tutorialObject.Length; i++)
        {
            tutorialObject[i].SetActive(false);
        }

        // 튜토리얼 배치가 끝나면 tutorialSkip이 true가 되므로 바로 종료되는 것 방지
        if(isPaused == false) return;

        // 튜토리얼 스킵 시 플레이어 조작 가능
        if(UIController.tutorialSkip == true) {
            player.isControl = true;
            isTutorialEnd = true;
        }
        else
        {
            player.isControl = false;
        }
    }

    void Update()
    {
        if (player == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
            {
                player = playerObj.GetComponent<Player>();
            }
            else
            {
                return; 
            }
        }

        // 입력 대기 중이면 무시
        if (inputUsed == true) return;

        // 모바일 스와이프 확인
        bool swipeUp = false;
        bool swipeDown = false;
        bool swipeLeft = false;
        bool swipeRight = false;
        bool isTap = false;

        // 모바일 스와이프 조작
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

                float swipeDistanceX = touchEndPos.x - touchStartPos.x;
                float swipeDistanceY = touchEndPos.y - touchStartPos.y;

                bool isSwipe = Mathf.Abs(swipeDistanceX) > minSwipeDistance || Mathf.Abs(swipeDistanceY) > minSwipeDistance;

                if (isSwipe)
                {
                    if(Mathf.Abs(swipeDistanceX) > Mathf.Abs(swipeDistanceY))
                    {
                        if(swipeDistanceX > 0) swipeRight = true;
                        else swipeLeft = true;
                    }
                    else
                    {
                        if(swipeDistanceY > 0) swipeUp = true;
                        else swipeDown = true;
                    }
                }
                else isTap = true;
            }
        }

        // 튜토리얼 단계별 입력 대기, 키 입력시 튜토리얼 진행
        if(tutorialStage == 1)  // 점프 튜토리얼
        {
            if (Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.Space) || swipeUp)
            {
                tutorialObject[tutorialStage - 1].SetActive(false);
                StartCoroutine(ResumeTutorial());
                player.Jump();
                StartCoroutine(TutorialDive());
            }
        }

        if(tutorialStage == 2)  // 퀵 다이브 튜토리얼
        {
            if (Input.GetKeyDown(KeyCode.DownArrow) || swipeDown)
            {
                tutorialObject[tutorialStage - 1].SetActive(false);
                StartCoroutine(ResumeTutorial());
                player.QuickDive();
            }
        }

        if(tutorialStage == 3)  // 좌우 이동 튜토리얼
        {
            if (Input.GetKeyDown(KeyCode.LeftArrow) || swipeLeft) {
                tutorialObject[tutorialStage - 1].SetActive(false);
                StartCoroutine(ResumeTutorial());
                player.ChangeLane(-1);
            }
            else if (Input.GetKeyDown(KeyCode.RightArrow) || swipeRight) {
                tutorialObject[tutorialStage - 1].SetActive(false);
                StartCoroutine(ResumeTutorial());
                player.ChangeLane(1);
            }
        }

        if(tutorialStage == 4)  // 다시 점프 튜토리얼
        {
            if (Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.Space) || swipeUp)
            {
                tutorialObject[tutorialStage - 1].SetActive(false);
                StartCoroutine(TutorialHook());
                player.Jump();
            }
        }

        if(tutorialStage == 5)  // 후크 튜토리얼
        {
            if (Input.GetMouseButtonDown(0) || isTap)
            {
                tutorialObject[tutorialStage - 1].SetActive(false);
                StartCoroutine(ResumeTutorial(true));
                player.hookShoot();
                StartCoroutine(TutorialEndDelay());
            }
        }
    }

    // 튜토리얼 오브젝트가 trigger에 닿으면 튜토리얼 단계 시작 (tutorialDesigner로 조절)
    private void OnTriggerExit(Collider other) {
        if(other.gameObject.tag == "TutorialDetector")
        {
            inputUsed = false;
            tutorialStage++;

            // 일시정지 상태 설정
            isPaused = true;    
            Time.timeScale = 0f;
            tutorialObject[tutorialStage - 1].SetActive(true);
        }  
    }

    // 퀵 다이브 튜토리얼
    IEnumerator TutorialDive()
    {
        inputUsed = true;
        isPaused = false;
        Time.timeScale = 1f;

        // 약간의 딜레이 후 일시정지
        yield return new WaitForSecondsRealtime(0.2f);

        isPaused = true;
        Time.timeScale = 0f;
        tutorialObject[tutorialStage].SetActive(true);

        tutorialStage++;
        inputUsed = false;
    }

    // 후크 튜토리얼
    IEnumerator TutorialHook()
    {
        inputUsed = true;
        isPaused = false;
        Time.timeScale = 1f;

        // 약간의 딜레이 후 일시정지
        yield return null;  // 한 프레임 대기
        yield return null;
        yield return new WaitForSecondsRealtime(0.1f);

        isPaused = true;
        Time.timeScale = 0f;
        tutorialObject[tutorialStage].SetActive(true);

        tutorialStage = 5;
        inputUsed = false;
    }    
    
    // 원하는 조작 시 튜토리얼 재개
    IEnumerator ResumeTutorial(bool endTutorial = false)
    {
        inputUsed = true;
        isPaused = false;
        Time.timeScale = 1f;

        yield return null;
        yield return null;

        // 튜토리얼 끝나면 tutorialStage 6으로 설정
        if (endTutorial)
        {
            tutorialStage = 6;
        }
    }

    // 튜토리얼 중 후크 이후 약간의 조작 잠금 (퀵 다이브등으로 튜토리얼 중 죽는것 방지)
    IEnumerator TutorialEndDelay()
    {
        yield return new WaitForSecondsRealtime(0.5f);

        if(Player.Instance != null)
            Player.Instance.isControl = true;
            
        UIController.tutorialSkip = true;

        // 튜토리얼 종료 플래그 설정
        yield return new WaitForSecondsRealtime(3f);
        isTutorialEnd = true;
    }
}
