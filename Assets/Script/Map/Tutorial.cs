using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tutorial : MonoBehaviour
{
    int tutorialStage = 0;
    bool isPaused = false;
    public GameObject[] tutorialObject;

    void Start() 
    {
        for(int i = 0; i < tutorialObject.Length; i++)
        {
            tutorialObject[i].SetActive(false);
        }    
    }
    void Update()
    {
        // 튜토리얼이 비활성화되어 있으면 실행하지 않음
        if(UIController.Instance == null || !UIController.isFirstPlay) return;
        if(isPaused == false) return;
        if(tutorialStage == 1)
        {
            if(player.isJump == true)
            {
                isPaused = false;
                Time.timeScale = 1f;
                tutorialObject[tutorialStage - 1].SetActive(false);
            }
        }
        if(tutorialStage == 2)
        {
            if(player.isMove == true)
            {
                isPaused = false;
                Time.timeScale = 1f;
                tutorialObject[tutorialStage - 1].SetActive(false);
            }
        }
        if(tutorialStage == 3)
        {
            if(player.isJump == true)
            {
                tutorialObject[tutorialStage - 1].SetActive(false);
                StartCoroutine(TutorialHook());
            }
        }
        if(tutorialStage == 4)
        {
            if(player.isHook == true)
            {
                isPaused = false;
                Time.timeScale = 1f;
                tutorialObject[tutorialStage - 1].SetActive(false);
                
                // 튜토리얼 완료 후 isFirstPlay를 false로 설정
                UIController.isFirstPlay = false;
                Debug.Log("Tutorial completed - isFirstPlay set to false");
            }
        }
    }

    private void OnTriggerExit(Collider other) {
        Debug.Log($"OnTriggerExit - Tag: {other.gameObject.tag}, isFirstPlay: {UIController.isFirstPlay}");
        
        // 튜토리얼이 비활성화되어 있으면 실행하지 않음
        if(UIController.Instance == null || !UIController.isFirstPlay) return;

        if(other.gameObject.tag == "TutorialDetector")
        {
            player.isControl = true;
            tutorialStage++;
            isPaused = true;
            Time.timeScale = 0f;
            Debug.Log($"Tutorial Stage {tutorialStage} started");
            tutorialObject[tutorialStage - 1].SetActive(true);
        }  
    }

    IEnumerator TutorialHook()
    {
        isPaused = false;
        Time.timeScale = 1f;

        yield return new WaitForSecondsRealtime(0.1f);
        player.isControl = true;
        isPaused = true;
        Time.timeScale = 0f;
        tutorialObject[tutorialStage].SetActive(true);

        tutorialStage = 4;
    }
}
