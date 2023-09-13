using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Net;
using System.Net.Security;
using System.Net.Sockets;
public class GameLauncher : MonoBehaviour
{
    public static string returnToMenuTextContent="Offline";
    public static Text returnToMenuText;
    public Button startGameButton;
    public InputField playerNameField;
    public InputField IPField;
    void PlayerNameFieldOnValueChanged(string s){
        NetworkProgram.clientUserName=s;
    }
    void IPFieldOnEndEdit(string s){
        
        IPAddress.TryParse(s,out NetworkProgram.ip);
    }
    void StartGameButtonOnClick(){
        SceneManager.LoadScene(1);
    }
    void Start(){
        returnToMenuText=GameObject.Find("connectfailtext").GetComponent<Text>();
        returnToMenuText.text=returnToMenuTextContent;
        startGameButton=GameObject.Find("startgamebutton").GetComponent<Button>();
        playerNameField=GameObject.Find("playernamefield").GetComponent<InputField>();
         IPField=GameObject.Find("ipfield").GetComponent<InputField>();
        startGameButton.onClick.AddListener(StartGameButtonOnClick);
        playerNameField.onValueChanged.AddListener(PlayerNameFieldOnValueChanged);
        IPField.onValueChanged.AddListener(IPFieldOnEndEdit);
    }
}
