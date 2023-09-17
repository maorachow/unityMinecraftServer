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

using MessagePack;
[MessagePackObject]
public class ChunkData{
    [Key(0)]
    public int[,,] map;
    [Key(1)]
    public Vector2Int chunkPos=new Vector2Int(0,0);
    public ChunkData(int[,,] map,Vector2Int chunkPos){
        this.map=map;
        this.chunkPos=chunkPos;
    }
}
[MessagePackObject]
public class ParticleData
{
    [Key(0)]
    public float posX;
    [Key(1)]
    public float posY;
    [Key(2)]
    public float posZ;
    [Key(3)]
    public int type;

    public ParticleData(float posX, float posY, float posZ, int type)
    {
        this.posX = posX;
        this.posY = posY;
        this.posZ = posZ;
        this.type = type;
    }
}


[MessagePackObject]
public class BlockModifyData
{
    [Key(0)]
    public float x;
    [Key(1)]
    public float y;
    [Key(2)]
    public float z;
    [Key(3)]
    public int convertType;

    public BlockModifyData(float x, float y, float z, int convertType)
    {
        this.x = x;
        this.y = y;
        this.z = z;
        this.convertType = convertType;
    }
}
[MessagePackObject]
public class UserData  
{
    [Key(0)]
    public float posX;
    [Key(1)]
    public float posY;
    [Key(2)]
   public float posZ;
   [Key(3)]
   public float rotY;
   [Key(4)]
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
    public byte[] messageContent;
   public Message(string messageType, byte[] messageContent)
 {
     this.messageType = messageType;
     this.messageContent = messageContent;
 }
}

public class NetworkProgram : MonoBehaviour
{ 
    public static MessagePackSerializerOptions lz4Options = MessagePackSerializerOptions.Standard.WithCompression(MessagePackCompression.Lz4BlockArray);
 //   public static Dictionary<Vector2Int,Chunk> chunks=new Dictionary<Vector2Int,Chunk>();
    public static Queue<Message> toDoList=new Queue<Message>();
    public static UserData currentPlayer;
    public static string clientUserName = "Default User";
    public static IPAddress ip = IPAddress.Parse("127.0.0.1");
    static int port = 11111;
    static Socket clientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
    public static bool isGoingToQuitGame=false;

    public static void ClientUpdateUserInfo(){
        Message m=new Message("UpdateUser", MessagePackSerializer.Serialize(currentPlayer,lz4Options));
        SendMessageToServer(m);
    }
    public static UnityEngine.Vector3 SysVec3ToUnityVec3(System.Numerics.Vector3 v){
        return new UnityEngine.Vector3(v.X,v.Y,v.Z);
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
            SendMessageToServer(new Message("Login", MessagePackSerializer.Serialize(currentPlayer,lz4Options)));
         //   SendMessageToServer(new Message("ChunkGen", MessagePackSerializer.Serialize(new Vector2Int(0,0))));
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
            if(m==null){
                Debug.Log("Empty message");
                   toDoList.Dequeue();
            }
            switch (m.messageType)
                        {
                    case "ClientUpdateUser":
             //       Debug.Log("UserUpdate");
                        ClientUpdateUserInfo();
                        break;
                    case "LoginReturn":
                        UnityEngine.Debug.Log("Server:" + MessagePackSerializer.Deserialize<string>(m.messageContent));
                        if(MessagePackSerializer.Deserialize<string>(m.messageContent,lz4Options)=="Failed"){
                              GameLauncher.returnToMenuTextContent="Failed connecting: a player with the same name has joined the world.";
                        }

                        break;
                    case "userCount":
                        UnityEngine.Debug.Log("Server:" + m.messageContent);
                        break;
                    case "UnknownMessage":
                        UnityEngine.Debug.Log("Server:"+ m.messageContent);
                        break;
                    case "WorldData":
                    ChunkData cd= MessagePackSerializer.Deserialize<ChunkData>(m.messageContent,lz4Options);
                    if(!Chunk.chunks.ContainsKey(cd.chunkPos)){
                        break;
                    }else{
                   
                        Chunk.chunks[cd.chunkPos].map=cd.map;
                        Chunk.chunks[cd.chunkPos].isChunkUpdated=true;
                         Chunk.chunks[cd.chunkPos].isChunkDataDownloaded=true;
                        Chunk.chunks[cd.chunkPos].isWaitingForNewChunkData=false;
                    }
                      
                            
                    break;
                    case "ClientModifyChunk":
                    BlockModifyData b = MessagePackSerializer.Deserialize<BlockModifyData>(m.messageContent,lz4Options);
                    Vector2Int cPos = Chunk.Vec3ToChunkPos(new Vector3(b.x, b.y, b.z));
                    Vector3 chunkSpacePos = new Vector3(b.x, b.y, b.z) - new Vector3(cPos.x, 0, cPos.y);
                    if(Chunk.GetChunk(cPos) == null){
                        break;
                    }else{
                   
                        Chunk.chunks[cPos].map[(int)chunkSpacePos.x,(int)chunkSpacePos.y,(int)chunkSpacePos.z]=b.convertType;
                        Chunk.chunks[cPos].isChunkUpdated=true;
                         Chunk.chunks[cPos].isChunkDataDownloaded=true;
                        Chunk.chunks[cPos].isWaitingForNewChunkData=false;
                    }
                      
                            
                        break;
                    case "EmitParticle":
                    Debug.Log("emit");
                    ParticleData pd=MessagePackSerializer.Deserialize<ParticleData>(m.messageContent,lz4Options);
                    particleAndEffectBeh.SpawnParticle(new UnityEngine.Vector3(pd.posX,pd.posY,pd.posZ),pd.type);
                    break;
                    case "ReturnAllUserData":
               //     Debug.Log("datareturn");
                        AllPlayersManager.clientPlayerList= MessagePackSerializer.Deserialize<List<UserData>>(m.messageContent,lz4Options);
                        AllPlayersManager.isPlayerDataUpdated=true;
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
                    Debug.Log(s.Length);
                    if(s.Length>10240000){
                    continue;
                    }
                        try{
                             Message m=JsonConvert.DeserializeObject<Message>(s);
                            toDoList.Enqueue(m);
                        }catch{
                                Debug.Log("Incomplete message");
                        }
                       
                    
                    }
                }
              
            }
            catch (Exception e)
            {
                UnityEngine.Debug.Log("Connection Lost: "+e);
                
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
        SendMessageToServer(new Message("LogOut",MessagePackSerializer.Serialize("null",lz4Options)));
        //clientSocket.Close();
    }
}
