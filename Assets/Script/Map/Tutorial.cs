using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tutorial : MonoBehaviour   // 튜토리얼 스크립트
{
    int tutorialStage = 0;
    public bool isPaused = false;
    public GameObject[] tutorialObject;
    Player player;
    private bool inputUsed = false;

    void Start() 
    {
        player = GameObject.FindGameObjectWithTag("Player").GetComponent<Player>();
        for(int i = 0; i < tutorialObject.Length; i++)
        {
            tutorialObject[i].SetActive(false);
        }    
        if(isPaused == false) return;
        if(UIController.tutorialSkip == true) {
            player.isControl = true;
        }
    }
    void Update()
    {
        if(inputUsed == true) return;
        // 튜토리얼 단계별 입력 대기
        if(tutorialStage == 1)
        {
            if (Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.Space))
            {
                tutorialObject[tutorialStage - 1].SetActive(false);
                StartCoroutine(ResumeTutorial());
                player.Jump();
            }
        }
        if(tutorialStage == 2)
        {
            if (Input.GetKeyDown(KeyCode.LeftArrow)) {
                tutorialObject[tutorialStage - 1].SetActive(false);
                StartCoroutine(ResumeTutorial());
                player.ChangeLane(-1);
            }
            else if (Input.GetKeyDown(KeyCode.RightArrow)) {
                tutorialObject[tutorialStage - 1].SetActive(false);
                StartCoroutine(ResumeTutorial());
                player.ChangeLane(1);
            }
        }
        if(tutorialStage == 3)
        {
            if (Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.Space))
            {
                tutorialObject[tutorialStage - 1].SetActive(false);
                StartCoroutine(TutorialHook());
                player.Jump();
            }
        }
        if(tutorialStage == 4)
        {
            if (Input.GetMouseButtonDown(0))
            {
                tutorialObject[tutorialStage - 1].SetActive(false);
                StartCoroutine(ResumeTutorial(true));
                player.hookShoot();
                StartCoroutine(EndTutorialDelay());
            }
        }
    }

    // 튜토리얼 오브젝트가 trigger에 닿으면 튜토리얼 단계 시작 (tutorialDesigner로 조절)
    private void OnTriggerExit(Collider other) {
        if(other.gameObject.tag == "TutorialDetector")
        {
            inputUsed = false;
            tutorialStage++;
            isPaused = true;
            Time.timeScale = 0f;
            tutorialObject[tutorialStage - 1].SetActive(true);
        }  
    }

    // 후크 튜토리얼
    IEnumerator TutorialHook()
    {
        inputUsed = true;
        isPaused = false;
        Time.timeScale = 1f;
        // 약간의 딜레이 후 일시정지
        yield return null;
        yield return null;
        yield return new WaitForSecondsRealtime(0.1f);
        isPaused = true;
        Time.timeScale = 0f;
        tutorialObject[tutorialStage].SetActive(true);

        tutorialStage = 4;
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
        if (endTutorial)
        {
            tutorialStage = 5;
        }
    }

    // 튜토리얼 중 후크 이후 약간의 조작 잠금 (퀵 다이브등으로 튜토리얼 중 죽는것 방지)
    IEnumerator EndTutorialDelay()
    {
        yield return new WaitForSecondsRealtime(0.5f);
        player.isControl = true;
        UIController.tutorialSkip = true;
    }
}
