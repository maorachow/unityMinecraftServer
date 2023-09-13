using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Net;
using System.Net.Security;
using System.Net.Sockets;
using Newtonsoft.Json;
using System.Threading;
using System.Text;
using System;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Numerics;

public class ChunkData{
    public int[,,] map;
    public Vector2Int chunkPos=new Vector2Int(0,0);
    public ChunkData(int[,,] map,Vector2Int chunkPos){
        this.map=map;
        this.chunkPos=chunkPos;
    }
}
public class BlockModifyData
{
    public float x;
    public float y;
    public float z;
    public int convertType;

    public BlockModifyData(float x, float y, float z, int convertType)
    {
        this.x = x;
        this.y = y;
        this.z = z;
        this.convertType = convertType;
    }
}

public class UserData  
{
    public float posX;
    
    public float posY;
   public float posZ;
   public float rotY;
   public string userName;

    public UserData(float posX, float posY, float posZ, float rotY, string userName)
    {
        this.posX = posX;
        this.posY = posY;
        this.posZ = posZ;
        this.rotY = rotY;
        this.userName = userName;
    }


    //  Quaternion rotation;
}

public class Message
{
    public string messageType;
    public string messageContent;
   public Message(string messageType, string messageContent)
 {
     this.messageType = messageType;
     this.messageContent = messageContent;
 }
}

public class NetworkProgram : MonoBehaviour
{
 //   public static Dictionary<Vector2Int,Chunk> chunks=new Dictionary<Vector2Int,Chunk>();
    public static Queue<Message> toDoList=new Queue<Message>();
    public static UserData currentPlayer;
    public static string clientUserName = "Default User";
    public static IPAddress ip = IPAddress.Parse("127.0.0.1");
    static int port = 11111;
    static Socket clientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
    public static bool isGoingToQuitGame=false;

    public static void ClientUpdateUserInfo(){
        Message m=new Message("UpdateUser", JsonConvert.SerializeObject(currentPlayer));
        SendMessageToServer(m);
    }

    public static void InitNetwork(){
        Chunk.chunks=new Dictionary<Vector2Int,Chunk>();
        toDoList=new Queue<Message>();
        clientSocket=new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            try{
            clientSocket.Connect(ip, port);
            }catch(Exception e){
                isGoingToQuitGame=true;
             //   warnText.text="Failed Connecting: "+e.ToString();
             GameLauncher.returnToMenuTextContent="Failed Connecting:"+e.ToString();
                return;
            }
            currentPlayer = new UserData(UnityEngine.Random.Range(-1f,1f) * 10f+15f, 100f, UnityEngine.Random.Range(-1f,1f) * 10f+15f, UnityEngine.Random.Range(-1f,1f)  * 10f, clientUserName);
            SendMessageToServer(new Message("Login", JsonConvert.SerializeObject(currentPlayer)));
         //   SendMessageToServer(new Message("ChunkGen", JsonConvert.SerializeObject(new Vector2Int(0,0))));
            isGoingToQuitGame=false;
            Thread thread = new Thread(new ThreadStart(RecieveServer));
            thread.Start();
    }
    public static void ChangePlayerName(string s){
        clientUserName=s;
    }
    public static void ChangeIP(string s){
        ip=IPAddress.Parse(s);
    }
    public static bool IsSocketAvaliable(Socket s){ return s!=null&&s.Connected==true;}
    public static void ToDoListExecute(){
        if(isGoingToQuitGame==true){
            return;
        }
        if(toDoList.Count>0){
            Message m=toDoList.Peek();
            switch (m.messageType)
                        {
                    case "ClientUpdateUser":
             //       Debug.Log("UserUpdate");
                        ClientUpdateUserInfo();
                        break;
                    case "LoginReturn":
                        UnityEngine.Debug.Log("Server:" + m.messageContent);
                        break;
                    case "userCount":
                        UnityEngine.Debug.Log("Server:" + m.messageContent);
                        break;
                    case "UnknownMessage":
                        UnityEngine.Debug.Log("Server:"+ m.messageContent);
                        break;
                    case "WorldData":
                    ChunkData cd=JsonConvert.DeserializeObject<ChunkData>(m.messageContent);
                    if(!Chunk.chunks.ContainsKey(cd.chunkPos)){
                        break;
                    }else{
                   
                        Chunk.chunks[cd.chunkPos].map=cd.map;
                        Chunk.chunks[cd.chunkPos].isChunkUpdated=true;
                        
                    }
                      
                            
                        break;
                    case "ReturnAllUserData":
               //     Debug.Log("datareturn");
                        AllPlayersManager.clientPlayerList=JsonConvert.DeserializeObject<List<UserData>>(m.messageContent);
                    break;
                    default:
                        UnityEngine.Debug.Log("Client: Unknown Message Type:"+m.messageType);
                        break;
                        }
                        toDoList.Dequeue();
        }
    }
    public static void RecieveServer()
    {
        while (IsSocketAvaliable(clientSocket))
        {
          //  Thread.Sleep(10);
            try
            {
            byte[] data = new byte[10240000];
           // clientSocket.Receive(data);
                int count = clientSocket.Receive(data);
                string str = System.Text.Encoding.UTF8.GetString(data, 0, count);
                
           
                foreach(string s in str.Split('&'))
                {
                   
                if (s.Length > 0) {
                  //   Debug.Log(s);
                  if(s.Length>10240000){
                    continue;
                  }
                        Message m=JsonConvert.DeserializeObject<Message>(s);
                        toDoList.Enqueue(m);
                    
                    }
                }
              
            }
            catch (Exception e)
            {
                UnityEngine.Debug.Log("Connection Lost"+e);
                
                 clientSocket.Close();
                 isGoingToQuitGame=true;
                //  SceneManager.LoadScene(0);
                AllPlayersManager.InitPlayerManager();
             break;
            }
        
        }
      
    }
    public static void SendMessageToServer(Message m)
    {
        try
        {
            clientSocket.Send(System.Text.Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(m)+'&'));
    //        clientSocket.Send(System.Text.Encoding.UTF8.GetBytes("&"));
        }
        catch(Exception e)
        {
            UnityEngine.Debug.Log("Sending message failed "+e);
            clientSocket.Close();
            isGoingToQuitGame=true;
        AllPlayersManager.InitPlayerManager();

        }
    }
   
    public static int FloatToInt(float f)
    {
        if (f >= 0)
        {
            return (int)f;
        }
        else
        {
            return (int)f - 1;
        }
    }
    public void StartGameButtonOnClick(){

             
    }
    
       


    void Start()
    {
         InitNetwork();
    }

    void Update()
    {
      
        if(isGoingToQuitGame==true){
            SceneManager.LoadScene(0);
        }

        ToDoListExecute();
    }
    void OnDestroy(){
        SendMessageToServer(new Message("LogOut","null"));
        //clientSocket.Close();
    }
}
