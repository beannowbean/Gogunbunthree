using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class Tutorial : MonoBehaviour
{
    int tutorialStage = 0;
    bool isPaused = false;
    public TextMeshProUGUI[] text;
    Player player;
    void Start() 
    {
        player = GameObject.FindGameObjectWithTag("Player").GetComponent<Player>();
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
            if(player.isJump == true)
            {
                isPaused = false;
                Time.timeScale = 1f;
                text[tutorialStage - 1].gameObject.SetActive(false);
                player.isJump = false;
                player.isControl = false;
            }
        }
        if(tutorialStage == 2)
        {
            if(player.isMove == true)
            {
                isPaused = false;
                Time.timeScale = 1f;
                text[tutorialStage - 1].gameObject.SetActive(false);
                player.isMove = false;
                player.isControl = false;
            }
        }
        if(tutorialStage == 3)
        {
            if(player.isJump == true)
            {
                text[tutorialStage - 1].gameObject.SetActive(false);
                player.isJump = false;
                player.isControl = false;
                StartCoroutine(TutorialHook());
            }
        }
        if(tutorialStage == 4)
        {
            if(player.isHook == true)
            {
                isPaused = false;
                Time.timeScale = 1f;
                text[tutorialStage - 1].gameObject.SetActive(false);
                player.isHook = false;
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
            text[tutorialStage - 1].gameObject.SetActive(true);
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
        text[tutorialStage].gameObject.SetActive(true);

        tutorialStage = 4;
    }
}
