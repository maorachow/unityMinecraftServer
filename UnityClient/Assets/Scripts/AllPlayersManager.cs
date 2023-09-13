using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class AllPlayersManager : MonoBehaviour
{
    public static Dictionary<string,GameObject> playerPrefabsInClient=new Dictionary<string,GameObject>();
    public static GameObject playerPrefab;
    public static List<UserData> clientPlayerList=new List<UserData>();
    public static void InitPlayerManager(){
        playerPrefabsInClient=new Dictionary<string,GameObject>();
        clientPlayerList=new List<UserData>();
       
    }
    public void RemovePlayer(){
            List<KeyValuePair<string,GameObject>> toDestroy=new List<KeyValuePair<string,GameObject>>();
         foreach(KeyValuePair<string,GameObject> p in playerPrefabsInClient){
            if(clientPlayerList.FindIndex(delegate (UserData cl) { return cl.userName ==p.Key; })==-1){
                toDestroy.Add(p);
            }
        }
        for(int i=0;i<toDestroy.Count;i++){
            Destroy(toDestroy[i].Value);
            playerPrefabsInClient.Remove(toDestroy[i].Key);
        }
    }
    public void UpdateAllPLayers(){

        foreach(UserData u in clientPlayerList){

            
            if(!playerPrefabsInClient.ContainsKey(u.userName)){
                GameObject a=Instantiate(playerPrefab,new Vector3(u.posX,u.posY,u.posZ),Quaternion.Euler(0f,u.rotY,0f));
                playerPrefabsInClient.Add(u.userName,a);
            }
            if(u.userName==NetworkProgram.clientUserName){playerPrefabsInClient[u.userName].GetComponent<PlayerMove>().isCurrentPlayer=true;playerPrefabsInClient[u.userName].transform.GetChild(0).GetChild(0).GetComponent<TMP_Text>().text=u.userName; continue;}
         //   Debug.Log(new Vector3(u.posX,u.posY,u.posZ));
            playerPrefabsInClient[u.userName].GetComponent<CharacterController>().enabled=false;
            playerPrefabsInClient[u.userName].transform.position=new Vector3(u.posX,u.posY,u.posZ);
            playerPrefabsInClient[u.userName].transform.rotation=Quaternion.Euler(0f,u.rotY,0f);
            playerPrefabsInClient[u.userName].transform.GetChild(0).GetChild(0).GetComponent<TMP_Text>().text=u.userName;
             playerPrefabsInClient[u.userName].transform.GetChild(0).LookAt(new Vector3(NetworkProgram.currentPlayer.posX,NetworkProgram.currentPlayer.posY,NetworkProgram.currentPlayer.posZ));
           // playerPrefabsInClient[u.userName].transform.GetChild(0).rotation=
        }
       
        
    }
    void Awake(){
        playerPrefab=Resources.Load<GameObject>("Prefabs/player");
        
    }
    // Start is called before the first frame update
    void Start()
    {
        InitPlayerManager();
    }
    void FixedUpdate(){
       
        
    }
    // Update is called once per frame
    void Update()
    {   RemovePlayer();
         UpdateAllPLayers();
    }
}
