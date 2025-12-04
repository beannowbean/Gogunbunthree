using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tutorial : MonoBehaviour
{
    int tutorialStage = 0;
    bool isPaused = false;
    public GameObject[] tutorialObject;
    public Player player;

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
        if(UIController.Instance == null || UIController.tutorialSkip) return;
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
                tutorialObject[tutorialStage - 2].SetActive(false);
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
                
                // 튜토리얼 완료 후 tutorialSkip을 true로 설정
                UIController.tutorialSkip = true;
                Debug.Log("Tutorial completed - tutorialSkip set to true");
            }
        }
    }

    private void OnTriggerExit(Collider other) {
        Debug.Log($"OnTriggerExit - Tag: {other.gameObject.tag}, tutorialSkip: {UIController.tutorialSkip}");
        
        // 튜토리얼이 비활성화되어 있으면 실행하지 않음
        if(UIController.Instance == null || UIController.tutorialSkip) return;

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
