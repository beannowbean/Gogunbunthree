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
            if(Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.Space))
            {
                isPaused = false;
                Time.timeScale = 1f;
                tutorialObject[tutorialStage - 1].SetActive(false);
            }
        }
        if(tutorialStage == 2)
        {
            if(Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.RightArrow))            
            {
                isPaused = false;
                Time.timeScale = 1f;
                tutorialObject[tutorialStage - 1].SetActive(false);
            }
        }
        if(tutorialStage == 3)
        {
            if(Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.Space))
            {
                tutorialObject[tutorialStage - 1].SetActive(false);
                StartCoroutine(TutorialHook());
            }
        }
        if(tutorialStage == 4)
        {
            if (Input.GetMouseButtonDown(0))
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
        isPaused = true;
        Time.timeScale = 0f;
        tutorialObject[tutorialStage].SetActive(true);

        tutorialStage = 4;
    }
}
