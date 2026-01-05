using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tutorial : MonoBehaviour
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
        if(tutorialStage == 1)
        {
            if (Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.Space))
            {
                tutorialObject[tutorialStage - 1].SetActive(false);
                StartCoroutine(ResumeTutorial());
                // player.Jump();
            }
        }
        if(tutorialStage == 2)
        {
            if (Input.GetKeyDown(KeyCode.LeftArrow)) {
                tutorialObject[tutorialStage - 1].SetActive(false);
                StartCoroutine(ResumeTutorial());
                // player.ChangeLane(-1);
            }
            else if (Input.GetKeyDown(KeyCode.RightArrow)) {
                tutorialObject[tutorialStage - 1].SetActive(false);
                StartCoroutine(ResumeTutorial());
                // player.ChangeLane(1);
            }
        }
        if(tutorialStage == 3)
        {
            if (Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.Space))
            {
                tutorialObject[tutorialStage - 1].SetActive(false);
                StartCoroutine(TutorialHook());
                // player.Jump();
            }
        }
        if(tutorialStage == 4)
        {
            if (Input.GetMouseButtonDown(0))
            {
                tutorialObject[tutorialStage - 1].SetActive(false);
                StartCoroutine(ResumeTutorial(true));
                // player.hookShoot();
                StartCoroutine(EndTutorialDelay());
            }
        }
    }

    private void OnTriggerExit(Collider other) {
        if(other.gameObject.tag == "TutorialDetector")
        {
            inputUsed = false;
            tutorialStage++;
            isPaused = true;
            Time.timeScale = 0f;
            Debug.Log($"Tutorial Stage {tutorialStage} started");
            tutorialObject[tutorialStage - 1].SetActive(true);
        }  
    }

    IEnumerator TutorialHook()
    {
        inputUsed = true;
        isPaused = false;
        Time.timeScale = 1f;
        yield return null;
        yield return null;
        yield return new WaitForSecondsRealtime(0.1f);
        isPaused = true;
        Time.timeScale = 0f;
        tutorialObject[tutorialStage].SetActive(true);

        tutorialStage = 4;
        inputUsed = false;
    }    
    
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

    IEnumerator EndTutorialDelay()
    {
        yield return new WaitForSecondsRealtime(0.5f);
        player.isControl = true;
    }
}
