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
            }
        }
    }

    private void OnTriggerExit(Collider other) {
        if(other.gameObject.tag == "TutorialDetector")
        {
            player.isControl = true;
            tutorialStage++;
            isPaused = true;
            Time.timeScale = 0f;
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
