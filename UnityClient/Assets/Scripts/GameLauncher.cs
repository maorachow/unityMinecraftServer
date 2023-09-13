using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class GameLauncher : MonoBehaviour
{
    public Button startGameButton;
    public InputField playerNameField;
    void PlayerNameFieldOnValueChanged(string s){
        NetworkProgram.clientUserName=s;
    }
    void StartGameButtonOnClick(){
        SceneManager.LoadScene(1);
    }
    void Start(){
        startGameButton=GameObject.Find("startgamebutton").GetComponent<Button>();
        playerNameField=GameObject.Find("playernamefield").GetComponent<InputField>();
        startGameButton.onClick.AddListener(StartGameButtonOnClick);
        playerNameField.onValueChanged.AddListener(PlayerNameFieldOnValueChanged);
    }
}
