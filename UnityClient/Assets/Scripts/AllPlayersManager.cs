using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Newtonsoft.Json;
using MessagePack;
public class AllPlayersManager : MonoBehaviour
{
    public static bool isPlayerDataUpdated=false;
    public static Dictionary<string,GameObject> playerPrefabsInClient=new Dictionary<string,GameObject>();
    public static Transform curPlayerTrans;
    public static GameObject playerPrefab;
    public static List<UserData> clientPlayerList=new List<UserData>();
    public static void InitPlayerManager(){
        playerPrefabsInClient=new Dictionary<string,GameObject>();
        clientPlayerList=new List<UserData>();
        curPlayerTrans=null;
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
          //  return;
        }
    }
    public void UpdateAllPLayers(){

        foreach(UserData u in clientPlayerList){

            PlayerMove pm=null;
            if(!playerPrefabsInClient.ContainsKey(u.userName)){
                GameObject a=Instantiate(playerPrefab,new Vector3(u.posX,u.posY,u.posZ),Quaternion.identity);
                playerPrefabsInClient.Add(u.userName,a);
                pm=playerPrefabsInClient[u.userName].GetComponent<PlayerMove>();
            }else{
                   pm=playerPrefabsInClient[u.userName].GetComponent<PlayerMove>();
            }
            if(u.userName==NetworkProgram.clientUserName){
               pm.isCurrentPlayer=true;
                 playerPrefabsInClient[u.userName].transform.GetChild(1).GetChild(0).GetComponent<MeshRenderer>().enabled=false;
                 if(curPlayerTrans==null){
                    curPlayerTrans=pm.transform;
                 }
                 continue;
                 }
                 playerPrefabsInClient[u.userName].transform.GetChild(1).GetChild(0).GetComponent<MeshRenderer>().enabled=true;
         //   Debug.Log(new Vector3(u.posX,u.posY,u.posZ));
        // playerPrefabsInClient[u.userName].transform.GetChild(0).GetChild(0).GetComponent<TMP_Text>().enabled=true;
            playerPrefabsInClient[u.userName].GetComponent<CharacterController>().enabled=false;
            pm.transform.position=Vector3.Lerp( pm.transform.position,new Vector3(u.posX,u.posY,u.posZ),10f*Time.deltaTime);
            pm.isPlayerAttacking=u.isAttacking;
            if( pm.headTrans!=null){
                pm.headTrans.rotation=Quaternion.Slerp( pm.headTrans.rotation,Quaternion.Euler(u.rotX,u.rotY,u.rotZ),10f*Time.deltaTime);
            }
            
            playerPrefabsInClient[u.userName].transform.GetChild(1).GetChild(0).GetComponent<TMP_Text>().text=u.userName;
             playerPrefabsInClient[u.userName].transform.GetChild(1).LookAt(new Vector3(NetworkProgram.currentPlayer.posX,NetworkProgram.currentPlayer.posY,NetworkProgram.currentPlayer.posZ));
           // playerPrefabsInClient[u.userName].transform.GetChild(0).rotation=
        }
       
        
    }
    void Awake(){
        playerPrefab=Resources.Load<GameObject>("Prefabs/player");
        
    }
   
    void Start()
    {
        InitPlayerManager();
    }
    void Update(){
        RemovePlayer();
         UpdateAllPLayers();
    }
 
  
}
