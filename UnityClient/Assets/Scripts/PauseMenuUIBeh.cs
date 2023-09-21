using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class PauseMenuUIBeh : MonoBehaviour
{
    public static PauseMenuUIBeh instance;
public Button returnToMainMenuButton;
public Button resumeButton;
void Start(){
    instance=this;
    returnToMainMenuButton=GameObject.Find("quitgamebutton").GetComponent<Button>();
    resumeButton=GameObject.Find("resumebutton").GetComponent<Button>();
    returnToMainMenuButton.onClick.AddListener(ReturnToMainMenuButtonOnClick);
    resumeButton.onClick.AddListener(ResumeButtonOnClick);
}
void ReturnToMainMenuButtonOnClick(){
    NetworkProgram.QuitGame();
}
void ResumeButtonOnClick(){
    NetworkProgram.Resume();
}
}
