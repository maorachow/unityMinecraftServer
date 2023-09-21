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
using System.Threading.Tasks;
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
    [Key(4)]
    public bool isSoundOnly = false;

    public ParticleData(float posX, float posY, float posZ, int type, bool isSoundOnly)
    {
        this.posX = posX;
        this.posY = posY;
        this.posZ = posZ;
        this.type = type;
        this.isSoundOnly = isSoundOnly;
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
public class UserData {
    [Key(0)]
    public float posX;
    [Key(1)]
    public float posY;
    [Key(2)]
    public float posZ;
    [Key(3)]
    public float rotX;
    [Key(4)]
    public float rotY;
    [Key(5)]
    public float rotZ;
    [Key(6)]
    public string userName;
    [Key(7)]
    public bool isAttacking;

    public UserData(float posX, float posY, float posZ, float rotX, float rotY, float rotZ, string userName, bool isAttacking)
    {
        this.posX = posX;
        this.posY = posY;
        this.posZ = posZ;
        this.rotX = rotX;
        this.rotY = rotY;
        this.rotZ = rotZ;
        this.userName = userName;
        this.isAttacking = isAttacking;
    }
}


public class NetworkProgram : MonoBehaviour
{ 
    public static bool isGamePaused=false;
    public static object listLock=new object();
    public static MessagePackSerializerOptions lz4Options = MessagePackSerializerOptions.Standard.WithCompression(MessagePackCompression.Lz4BlockArray);
 //   public static Dictionary<Vector2Int,Chunk> chunks=new Dictionary<Vector2Int,Chunk>();
    public static Queue<MessageProtocol> toDoList=new Queue<MessageProtocol>();
    public static UserData currentPlayer;
    public static string clientUserName = "Default User";
    public static IPAddress ip = IPAddress.Parse("127.0.0.1");
    static int port = 11111;
    static Socket clientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
    public static bool isGoingToQuitGame=false;

    public static void ClientUpdateUserInfo(){
        MessageProtocol m=new MessageProtocol(131, MessagePackSerializer.Serialize(currentPlayer,lz4Options));
        SendMessageToServer(m);
    }
    public static UnityEngine.Vector3 SysVec3ToUnityVec3(System.Numerics.Vector3 v){
        return new UnityEngine.Vector3(v.X,v.Y,v.Z);
    }
    public static void InitNetwork(){
        isGamePaused=false;
        Chunk.chunks=new Dictionary<Vector2Int,Chunk>();
        toDoList=new Queue<MessageProtocol>();
        clientSocket=new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            try{
            clientSocket.Connect(ip, port);
            }catch(Exception e){
                isGoingToQuitGame=true;
             //   warnText.text="Failed Connecting: "+e.ToString();
             GameLauncher.returnToMenuTextContent="Failed Connecting:"+e.ToString();
                return;
            }
             Thread thread = new Thread(new ThreadStart(RecieveServer));
            thread.Start();
            currentPlayer = new UserData(UnityEngine.Random.Range(-1f,1f) * 10f+15f, 100f, UnityEngine.Random.Range(-1f,1f) * 10f+15f, UnityEngine.Random.Range(-1f,1f)  * 10f,UnityEngine.Random.Range(-1f,1f)  * 10f,UnityEngine.Random.Range(-1f,1f)  * 10f, clientUserName,false);
            SendMessageToServer(new MessageProtocol(129, MessagePackSerializer.Serialize(currentPlayer,lz4Options)));
         //   SendMessageToServer(new MessageProtocol(137, MessagePackSerializer.Serialize(new Vector2Int(0,0))));
            isGoingToQuitGame=false;
           PauseMenuUIBeh.instance.gameObject.SetActive(false);
           Resume();
    }
    public static void ChangePlayerName(string s){
        clientUserName=s;
    }
    public static void ChangeIP(string s){
        ip=IPAddress.Parse(s);
    }
    public static bool IsSocketAvaliable(Socket s){ return s!=null&&s.Connected==true;}
    public static async void ToDoListExecute(){
        if(isGoingToQuitGame==true){
            return;
        }
        if(toDoList.Count>0){
            
            MessageProtocol m=toDoList.Peek();
            if(m==null){
                Debug.Log("Empty message");
                   toDoList.Dequeue();
                   return;
            }
            switch (m.Command)
                        {
                    case 141:
             //       Debug.Log("UserUpdate");
                        ClientUpdateUserInfo();
                        break;
                    case 136:
                        UnityEngine.Debug.Log("Server:" + MessagePackSerializer.Deserialize<string>(m.MessageData));
                        if(MessagePackSerializer.Deserialize<string>(m.MessageData,lz4Options)=="Failed"){
                            isGoingToQuitGame=true;
                              GameLauncher.returnToMenuTextContent="Failed connecting: a player with the same name has joined the world.";
                        }

                        break;
                //    case "userCount":
              ////          UnityEngine.Debug.Log("Server:" + m.MessageData);
              //          break;
              //      case "UnknownMessage":
              //          UnityEngine.Debug.Log("Server:"+ m.MessageData);
              //          break;
                    case 128:
                    ChunkData cd= MessagePackSerializer.Deserialize<ChunkData>(m.MessageData,lz4Options);
                    if(!Chunk.chunks.ContainsKey(cd.chunkPos)){
                        break;
                    }else{
                   
                        Chunk.chunks[cd.chunkPos].map=cd.map;
                        Chunk.chunks[cd.chunkPos].isChunkUpdated=true;
                         Chunk.chunks[cd.chunkPos].isChunkDataDownloaded=true;
                        Chunk.chunks[cd.chunkPos].isWaitingForNewChunkData=false;
                    }

                            
                    break;
                    case 139:

                    Task.Run(()=> {BlockModifyData b = MessagePackSerializer.Deserialize<BlockModifyData>(m.MessageData,lz4Options);
                    Vector2Int cPos = Chunk.Vec3ToChunkPos(new Vector3(b.x, b.y, b.z));
                    Vector3 chunkSpacePos = new Vector3(b.x, b.y, b.z) - new Vector3(cPos.x, 0, cPos.y);
                    if(Chunk.GetChunk(cPos) == null){
                        return;
                    }else{
                   
                        Chunk.chunks[cPos].map[(int)chunkSpacePos.x,(int)chunkSpacePos.y,(int)chunkSpacePos.z]=b.convertType;
                        Chunk.chunks[cPos].isChunkUpdated=true;
                         Chunk.chunks[cPos].isChunkDataDownloaded=true;
                        Chunk.chunks[cPos].isWaitingForNewChunkData=false;
                    }});
                      
                            
                        break;
                    case 138:
                    Debug.Log("emit");
                    ParticleData pd=MessagePackSerializer.Deserialize<ParticleData>(m.MessageData,lz4Options);
                    particleAndEffectBeh.SpawnParticle(new UnityEngine.Vector3(pd.posX,pd.posY,pd.posZ),pd.type,pd.isSoundOnly);
                    break;
                    case 135:
                         Task.Run(()=> {AllPlayersManager.clientPlayerList= MessagePackSerializer.Deserialize<List<UserData>>(m.MessageData,lz4Options);
                        AllPlayersManager.isPlayerDataUpdated=true;});
                       
                        break;
                        default:
                        UnityEngine.Debug.Log("Client: Unknown Message Type:"+System.Text.Encoding.UTF8.GetString(m.MessageData));
                        break;
                        }
                      
                       toDoList.Dequeue();     
                        
                        
        }
    }
    public static void RecieveServer()
    {
        while (true)
        {
          //   Debug.Log("1");
            Thread.Sleep(3);
            try
            {
            MessageProtocol mp = null;
            int ReceiveLength = 0;
            byte[] staticReceiveBuffer = new byte[1024000];  // 接收缓冲区(固定长度)
            byte[] dynamicReceiveBuffer = new byte[] { };  // 累加数据缓存(不定长)
  
 
            ReceiveLength = clientSocket.Receive(staticReceiveBuffer);  // 同步接收数据
           //  Debug.Log(ReceiveLength);
            dynamicReceiveBuffer = MessageProtocol.CombineBytes(dynamicReceiveBuffer, 0, dynamicReceiveBuffer.Length, staticReceiveBuffer, 0, ReceiveLength);  // 将之前多余的数据与接收的数据合并,形成一个完整的数据包
            if (ReceiveLength <= 0)  // 如果接收到的数据长度小于0(通常表示socket已断开,但也不一定,需要进一步判断,此处可以忽略)
            {
            Console.WriteLine("收到0字节数据");
            break;  // 终止接收循环
            }
            else if (dynamicReceiveBuffer.Length < MessageProtocol.HEADLENGTH)  // 如果缓存中的数据长度小于协议头长度,则继续接收
            {
        
            continue;  // 跳过本次循环继续接收数据
                }
            else  // 缓存中的数据大于等于协议头的长度
            {
            var headInfo = MessageProtocol.GetHeadInfo(dynamicReceiveBuffer);  // 解读协议头的信息
             while (dynamicReceiveBuffer.Length - MessageProtocol.HEADLENGTH >= headInfo.DataLength)  // 当缓存数据长度减去协议头长度大于等于实际数据的长度则进入循环进行拆包处理
                {
           
              mp = new MessageProtocol(dynamicReceiveBuffer);  // 拆包
         //      Debug.Log(mp);
              dynamicReceiveBuffer = mp.MoreData;  // 将拆包后得出多余的字节付给缓存变量,以待下一次循环处理数据时使用,若下一次循环缓存数据长度不能构成一个完整的数据包则不进入循环跳到外层循环继续接收数据并将本次得出的多余数据与之合并重新拆包,依次循环。
              headInfo = MessageProtocol.GetHeadInfo(dynamicReceiveBuffer);  // 从缓存中解读出下一次数据所需要的协议头信息,已准备下一次拆包循环,如果数据长度不能构成协议头所需的长度,拆包结果为0,下一次循环则不能成功进入,跳到外层循环继续接收数据合并缓存形成一个完整的数据包
                toDoList.Enqueue(mp);


            } // 拆包循环结束
            }
       
        }
            catch (Exception e)
           {
                UnityEngine.Debug.Log("Connection Lost: "+e);
                
                 clientSocket.Close();
                 isGoingToQuitGame=true;
        //  //      //  SceneManager.LoadScene(0);
            AllPlayersManager.InitPlayerManager();
           break;
           }
        
        }
      
    }
    public static void SendMessageToServer(MessageProtocol m)
    {
        try
        {
            clientSocket.Send(m.GetBytes());
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

    public static void QuitGame(){
     //       UnityEngine.Debug.Log("Sending message failed "+e);
       //     clientSocket.Close();
            isGoingToQuitGame=true;
        AllPlayersManager.InitPlayerManager();
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
        if(Input.GetKeyDown(KeyCode.Escape)){
            PauseOrResume();
        }
    }
    void PauseOrResume(){
       if(isGamePaused==true){
        Resume();
       }else{
        Pause();
       }
    }
    public static void Pause(){
        Time.timeScale=0;
        PauseMenuUIBeh.instance.gameObject.SetActive(true);
        isGamePaused=true;
    }
    public static void Resume(){
        Time.timeScale=1;
        PauseMenuUIBeh.instance.gameObject.SetActive(false);
        isGamePaused=false;
    }
    void OnDestroy(){
        SendMessageToServer(new MessageProtocol(130,MessagePackSerializer.Serialize("null",lz4Options)));
        //clientSocket.Close();
    }
}
