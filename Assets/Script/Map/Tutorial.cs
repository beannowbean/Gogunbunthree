using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class Tutorial : MonoBehaviour
{
    int tutorialStage = 0;
    bool isPaused = false;
    public TextMeshProUGUI[] text;
    void Start() 
    {
        for(int i = 0; i < text.Length; i++)
        {
            text[i].gameObject.SetActive(false);
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
                text[tutorialStage - 1].gameObject.SetActive(false);
            }
        }
        if(tutorialStage == 2)
        {
            if(Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.RightArrow))            
            {
                isPaused = false;
                Time.timeScale = 1f;
                text[tutorialStage - 1].gameObject.SetActive(false);
            }
        }
        if(tutorialStage == 3)
        {
            if(Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.Space))
            {
                text[tutorialStage - 1].gameObject.SetActive(false);
                StartCoroutine(TutorialHook());
            }
        }
        if(tutorialStage == 4)
        {
            if (Input.GetMouseButtonDown(0))
            {
                isPaused = false;
                Time.timeScale = 1f;
                text[tutorialStage - 1].gameObject.SetActive(false);
            }
        }
    }

    private void OnTriggerExit(Collider other) {
        if(other.gameObject.tag == "TutorialDetector")
        {
            tutorialStage++;
            isPaused = true;
            Time.timeScale = 0f;
            text[tutorialStage - 1].gameObject.SetActive(true);
        }  
    }

    IEnumerator TutorialHook()
    {
        isPaused = false;
        Time.timeScale = 1f;

        yield return new WaitForSecondsRealtime(0.1f);
        isPaused = true;
        Time.timeScale = 0f;
        text[tutorialStage].gameObject.SetActive(true);

        tutorialStage = 4;
    }
}
