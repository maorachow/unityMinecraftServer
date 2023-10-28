//using Utf8Json;
using MessagePack;
using MyMinecraftServer;
using Newtonsoft.Json;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Numerics;

[MessagePackObject]
public class ChunkData
{
    [Key(0)]
    public int[,,] map;
    [Key(1)]
    public Vector2Int chunkPos = new Vector2Int(0, 0);
    [JsonConstructor]
    public ChunkData(int[,,] map, Vector2Int chunkPos)
    {
        this.map = map;
        this.chunkPos = chunkPos;
    }
    public ChunkData(Vector2Int chunkPos)
    {
      //  this.map = map;
        this.chunkPos = chunkPos;
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
[MessagePackObject]
public class HurtEntityData
{
    [Key(0)]
    public string entityID;
    [Key(1)]
    public float hurtValue;

    public HurtEntityData(string entityID, float hurtValue)
    {
        this.entityID = entityID;
        this.hurtValue = hurtValue;
    }
}

/*public class Message
{
    public string messageType;
    public byte[] messageContent;
    public Message(string messageType, byte[] messageContent)
    {
        this.messageType = messageType;
        this.messageContent = messageContent;
    }
    
}*/
public sealed class Program
{
    static Form1 mainForm;
    static MessagePackSerializerOptions lz4Options = MessagePackSerializerOptions.Standard.WithCompression(MessagePackCompression.Lz4BlockArray);
    static object allClientSocketsOnlineLock = new object();
    //  static Chunk world;
    public static string gameWorldDataPath= AppDomain.CurrentDomain.BaseDirectory;
    public static Dictionary<Vector2Int,Chunk> chunks= new Dictionary<Vector2Int,Chunk>();
    public static Dictionary<Vector2Int, ChunkData> chunkDataReadFromDisk = new Dictionary<Vector2Int, ChunkData>();
   /* public static object listLock = new object(); 
    public static object listLock2 = new object();
    public static object listLock3 = new object();
    public static object listLock4 = new object();
    public static PriorityQueue<KeyValuePair<Socket, MessageProtocol>, int> toDoList2 = new PriorityQueue<KeyValuePair<Socket, MessageProtocol>, int>();
    public static PriorityQueue<KeyValuePair<Socket, MessageProtocol>,int> toDoList=new PriorityQueue<KeyValuePair<Socket, MessageProtocol>,int>();
    public static PriorityQueue<KeyValuePair<Socket, MessageProtocol>, int> toDoList3 = new PriorityQueue<KeyValuePair<Socket, MessageProtocol>, int>();
    public static PriorityQueue<KeyValuePair<Socket, MessageProtocol>, int> toDoList4 = new PriorityQueue<KeyValuePair<Socket, MessageProtocol>, int>();*/
    public static List<object> listLocks = new List<object> { new object(), new object(),new object(), new object(), new object(), new object(), new object(), new object(),
     new object(), new object(),new object(), new object(), new object(), new object(), new object(), new object()};
    public static List<PriorityQueue<KeyValuePair<Socket, MessageProtocol>, int>> toDoLists=new List<PriorityQueue<KeyValuePair<Socket, MessageProtocol>, int>> { 
        new PriorityQueue<KeyValuePair<Socket, MessageProtocol>, int>() , 
        new PriorityQueue<KeyValuePair<Socket, MessageProtocol>, int>() , 
        new PriorityQueue<KeyValuePair<Socket, MessageProtocol>, int>() , 
        new PriorityQueue<KeyValuePair<Socket, MessageProtocol>, int>(), 
        new PriorityQueue<KeyValuePair<Socket, MessageProtocol>, int>() ,
        new PriorityQueue<KeyValuePair<Socket, MessageProtocol>, int>() ,
        new PriorityQueue<KeyValuePair<Socket, MessageProtocol>, int>() ,
        new PriorityQueue<KeyValuePair<Socket, MessageProtocol>, int>(),
        new PriorityQueue<KeyValuePair<Socket, MessageProtocol>, int>() ,
        new PriorityQueue<KeyValuePair<Socket, MessageProtocol>, int>() ,
        new PriorityQueue<KeyValuePair<Socket, MessageProtocol>, int>() ,
        new PriorityQueue<KeyValuePair<Socket, MessageProtocol>, int>(),
        new PriorityQueue<KeyValuePair<Socket, MessageProtocol>, int>() ,
        new PriorityQueue<KeyValuePair<Socket, MessageProtocol>, int>() ,
        new PriorityQueue<KeyValuePair<Socket, MessageProtocol>, int>() ,
        new PriorityQueue<KeyValuePair<Socket, MessageProtocol>, int>()};//8线程
    public static IPAddress ip = IPAddress.Parse("0.0.0.0");
    public static int port = 11111;
    public static Socket serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
    public static List<Socket> allClientSocketsOnline=new List<Socket>();
    public static List<UserData> allUserData=new List<UserData>();
   
    static object chunkLock = new object();
    static object userDataLock = new object();

    public static bool isJsonReadFromDisk { get; private set; }
    public static bool isWorldDataSaved { get; private set; }

    public static void UserLogout(Socket socket) {
        int index = allClientSocketsOnline.FindIndex(delegate (Socket cl) { return cl == socket; });
        if (index!=-1)
        {
            allClientSocketsOnline.RemoveAt(index);
            allUserData.RemoveAt(index);
            mainForm.LogOnTextbox(socket.RemoteEndPoint.ToString() + "  logged out");
            CastToAllClients(new MessageProtocol(135, MessagePackSerializer.Serialize(allUserData)));
            socket.Close();
        }
    }
    public static void SaveWorldData()
    {

        FileStream fs;
        if (File.Exists(gameWorldDataPath + "unityMinecraftServerData/GameData/world.json"))
        {
            fs = new FileStream(gameWorldDataPath + "unityMinecraftServerData/GameData/world.json", FileMode.Truncate, FileAccess.Write);//Truncate模式打开文件可以清空。
        }
        else
        {
            fs = new FileStream(gameWorldDataPath + "unityMinecraftServerData/GameData/world.json", FileMode.Create, FileAccess.Write);
        }
        fs.Close();
        foreach (KeyValuePair<Vector2Int, Chunk> c in chunks)
        {
            // int[] worldDataMap=ThreeDMapToWorldData(c.Value.map);
            //   int x=(int)c.Value.transform.position.x;
            //  int z=(int)c.Value.transform.position.z;
            //   WorldData wd=new WorldData();
            //   wd.map=worldDataMap;
            //   wd.posX=x;
            //   wd.posZ=z;
            //   string tmpData=JsonMapper.ToJson(wd);
            //   File.AppendAllText(Application.dataPath+"/GameData/world.json",tmpData+"\n");
            c.Value.SaveSingleChunk();
        }
     
    //    foreach (KeyValuePair<Vector2Int, ChunkData> wd in chunkDataReadFromDisk)
     //   {
      //      string tmpData = JsonConvert.SerializeObject(wd.Value);
            
        //    }
        byte[] allWorldData=MessagePackSerializer.Serialize(chunkDataReadFromDisk);
            File.WriteAllBytes(gameWorldDataPath + "unityMinecraftServerData/GameData/world.json",allWorldData);
        isWorldDataSaved = true;
    }
    public static void ReadJson()
    {
        chunkDataReadFromDisk.Clear();
     //   gameWorldDataPath = WorldManager.gameWorldDataPath;

        if (!Directory.Exists(gameWorldDataPath + "unityMinecraftServerData"))
        {
            Directory.CreateDirectory(gameWorldDataPath + "unityMinecraftServerData");

        }
        if (!Directory.Exists(gameWorldDataPath + "unityMinecraftServerData/GameData"))
        {
            Directory.CreateDirectory(gameWorldDataPath + "unityMinecraftServerData/GameData");
        }

        if (!File.Exists(gameWorldDataPath + "unityMinecraftServerData" + "/GameData/world.json"))
        {
           FileStream fs= File.Create(gameWorldDataPath + "unityMinecraftServerData" + "/GameData/world.json");
            fs.Close();
        }

        byte[] worldData = File.ReadAllBytes(gameWorldDataPath + "unityMinecraftServerData/GameData/world.json");
      /*  List<ChunkData> tmpList = new List<ChunkData>();
        foreach (string s in worldData)
        {
            ChunkData tmp = JsonConvert.DeserializeObject<ChunkData>(s);
            tmpList.Add(tmp);
        }
        foreach (ChunkData w in tmpList)
        {
            chunkDataReadFromDisk.Add(new Vector2Int(w.chunkPos.x, w.chunkPos.y), w);
        }*/
      if(worldData.Length > 0)
        {
chunkDataReadFromDisk=MessagePackSerializer.Deserialize<Dictionary<Vector2Int,ChunkData>>(worldData);
        }
      
        isJsonReadFromDisk = true;
    }
    public static void socketWait(Socket socket)
    {
        while (true)
        {
            
        socket.Listen();
        Socket s=socket.Accept();
     //   allClientSockets.Add(s);
        Thread t=new Thread(new ParameterizedThreadStart(RecieveClient));
        t.Start(s);
        mainForm.LogOnTextbox("connected");
        mainForm.LogOnTextbox(socket.ToString());
        }
       
      
    }
    public static Vector3Int Vec3ToBlockPos(Vector3 pos)
    {
        Vector3Int intPos = new Vector3Int(FloatToInt(pos.X), FloatToInt(pos.Y), FloatToInt(pos.Z));
        return intPos;
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
    public static int FloorFloat(float n)
    {
        int i = (int)n;
        return n >= i ? i : i - 1;
    }

    public static int CeilFloat(float n)
    {
        int i = (int)(n + 1);
        return n >= i ? i : i - 1;
    }
    public static Vector2Int Vec3ToChunkPos(Vector3 pos)
    {
        Vector3 tmp = pos;
        tmp.X = MathF.Floor(tmp.X / (float)Chunk.chunkWidth) * Chunk.chunkWidth;
        tmp.Z = MathF.Floor(tmp.Z / (float)Chunk.chunkWidth) * Chunk.chunkWidth;
        Vector2Int value = new Vector2Int((int)tmp.X, (int)tmp.Z);
      //  mainForm.LogOnTextbox(value.x+" "+value.y+"\n");
        return value;
    }
    public static Chunk GetChunk(Vector2Int chunkPos)
    {
        if (chunks.ContainsKey(chunkPos))
        {
            Chunk tmp = chunks[chunkPos];
            return tmp;
        }
        else
        {
            return null;
        }

    }
    public static void AppendMessage(Socket s,MessageProtocol mp)
    {
    Random rand=new Random();
        var tdl = toDoLists[rand.Next(0, 4)];

          
                lock (listLocks[toDoLists.IndexOf(tdl)])
                {
                    //   object o= JsonConvert.DeserializeObject<object>(x);


                    switch (mp.Command)
                    {
                        case 132:
                            tdl.Enqueue(new KeyValuePair<Socket, MessageProtocol>(s, mp), 0);
                            break;
                        case 134:
                            tdl.Enqueue(new KeyValuePair<Socket, MessageProtocol>(s, mp), 1);
                            break;
                        case 131:
                            tdl.Enqueue(new KeyValuePair<Socket, MessageProtocol>(s, mp), 10);
                            break;
                        default:
                            tdl.Enqueue(new KeyValuePair<Socket, MessageProtocol>(s, mp), 1);
                            break;
                    }

                }
            
        
    }
    public static async void RecieveClient(object socket)
    {
        Socket s = (Socket)socket;
 MessageProtocol mp = null;
                int ReceiveLength = 0;
                byte[] staticReceiveBuffer = new byte[102400];  // 接收缓冲区(固定长度)
                byte[] dynamicReceiveBuffer = new byte[] { };  // 累加数据缓存(不定长)
      //  byte[] bb = new byte[102400];
      //  ArraySegment<byte> b= new ArraySegment<byte>(bb);
        while(true)
        {
            if (s == null||s.Connected==false)
                {
                    mainForm.LogOnTextbox("Recieve client failed:socket closed");
                    return;
                }
            try
            {//public int Receive (System.Collections.Generic.IList<ArraySegment<byte>> buffers);
             
                //     int count =s.Receive(bb);


                // ReceiveLength = s.Receive(staticReceiveBuffer);

              
                ReceiveLength = s.Receive(staticReceiveBuffer);  // 同步接收数据
                    dynamicReceiveBuffer = MessageProtocol.CombineBytes(dynamicReceiveBuffer, 0, dynamicReceiveBuffer.Length, staticReceiveBuffer, 0, ReceiveLength);  // 将之前多余的数据与接收的数据合并,形成一个完整的数据包
                    if (ReceiveLength <= 0)  // 如果接收到的数据长度小于0(通常表示socket已断开,但也不一定,需要进一步判断,此处可以忽略)
                    {
                        mainForm.LogOnTextbox("收到0字节数据");
                        UserLogout(s);
                        return;  // 终止接收循环
                    }
                    else if (dynamicReceiveBuffer.Length < MessageProtocol.HEADLENGTH)  // 如果缓存中的数据长度小于协议头长度,则继续接收
                    {
                        continue;  // 跳过本次循环继续接收数据
                    }
                    else  // 缓存中的数据大于等于协议头的长度(dynamicReadBuffer.Length >= 6)
                    {
                        var headInfo = MessageProtocol.GetHeadInfo(dynamicReceiveBuffer);  // 解读协议头的信息
                        while (dynamicReceiveBuffer.Length - MessageProtocol.HEADLENGTH >= headInfo.DataLength)  // 当缓存数据长度减去协议头长度大于等于实际数据的长度则进入循环进行拆包处理
                        {
                            mp = new MessageProtocol(dynamicReceiveBuffer);  // 拆包
                     //       mainForm.LogOnTextbox("Message:"+mp.Command);
                            dynamicReceiveBuffer = mp.MoreData;  // 将拆包后得出多余的字节付给缓存变量,以待下一次循环处理数据时使用,若下一次循环缓存数据长度不能构成一个完整的数据包则不进入循环跳到外层循环继续接收数据并将本次得出的多余数据与之合并重新拆包,依次循环。
                            headInfo = MessageProtocol.GetHeadInfo(dynamicReceiveBuffer);  // 从缓存中解读出下一次数据所需要的协议头信息,已准备下一次拆包循环,如果数据长度不能构成协议头所需的长度,拆包结果为0,下一次循环则不能成功进入,跳到外层循环继续接收数据合并缓存形成一个完整的数据包
                        AppendMessage(s, mp);
                          /*  if (toDoList.Count > toDoList2.Count)
                            {
                                lock (listLock2)
                                {
                                    //   object o= JsonConvert.DeserializeObject<object>(x);
                              
                              
                                    switch (mp.Command)
                                    {
                                        case 132:
                                            toDoList2.Enqueue(new KeyValuePair<Socket, MessageProtocol>(s, mp), 0);
                                            break;
                                        case 134:
                                            toDoList2.Enqueue(new KeyValuePair<Socket, MessageProtocol>(s, mp), 1);
                                            break;
                                        case 131:
                                            toDoList2.Enqueue(new KeyValuePair<Socket, MessageProtocol>(s, mp), 10);
                                            break;
                                        default:
                                            toDoList2.Enqueue(new KeyValuePair<Socket, MessageProtocol>(s, mp), 1);
                                            break;
                                    }

                                }
                            }*/
                       


                        } // 拆包循环结束
                    }
                
               
             
                /* string str = System.Text.Encoding.UTF8.GetString(bb.ToArray(),0,count);
                 foreach (string x in str.Split('&'))
                 {
                       if (x.Length > 0)
                         {
                         //         mainForm.LogOnTextbox(x);
                         if (toDoList.Count > toDoList2.Count){
                             lock (listLock2)
                             {
                                 //   object o= JsonConvert.DeserializeObject<object>(x);
                                 if (x.Length > 65536)
                                 {
                                     UserLogout(s);
                                 }
                                 MessageProtocol m = JsonConvert.DeserializeObject<MessageProtocol>(x);
                                 switch (m.Command)
                                 {
                                     case 132:
                                         toDoList2.Enqueue(new KeyValuePair<Socket, MessageProtocol>(s, JsonConvert.DeserializeObject<MessageProtocol>(x)), 0);
                                         break;
                                     case 134:
                                         toDoList2.Enqueue(new KeyValuePair<Socket, MessageProtocol>(s, JsonConvert.DeserializeObject<MessageProtocol>(x)), 1);
                                         break;
                                     case 131:
                                         toDoList2.Enqueue(new KeyValuePair<Socket, MessageProtocol>(s, JsonConvert.DeserializeObject<MessageProtocol>(x)), 10);
                                         break;
                                     default:
                                         toDoList2.Enqueue(new KeyValuePair<Socket, MessageProtocol>(s, JsonConvert.DeserializeObject<MessageProtocol>(x)), 1);
                                         break;
                                 }

                             }
                         }
                         else
                         {
                         lock (listLock)
                           {
                             //   object o= JsonConvert.DeserializeObject<object>(x);
                             if (x.Length > 65536)
                             {
                                 UserLogout(s);
                             }
                             Message m = JsonConvert.DeserializeObject<Message>(x);
                         switch (m.messageType)
                         {
                             case "UpdateChunk":
                                 toDoList.Enqueue(new KeyValuePair<Socket, Message>(s, JsonConvert.DeserializeObject<Message>(x)),0);
                                 break;
                             case "ChunkGen":
                                 toDoList.Enqueue(new KeyValuePair<Socket, Message>(s, JsonConvert.DeserializeObject<Message>(x)), 1);
                                 break;
                             case "UpdateUser":
                                 toDoList.Enqueue(new KeyValuePair<Socket, Message>(s, JsonConvert.DeserializeObject<Message>(x)), 10);
                                 break;
                                 default:
                                     toDoList.Enqueue(new KeyValuePair<Socket, Message>(s, JsonConvert.DeserializeObject<Message>(x)), 1);
                                     break;
                         }

                     }
                         }

                 //  mainForm.LogOnTextbox("Recieved Message:" + JsonConvert.DeserializeObject<Message>(str));

             }
                     }*/

            }
        catch (Exception ex) { mainForm.LogOnTextbox("Connection stopped : "+ex.ToString());UserLogout(s);
                break; }
        }
      

    }
    public static void SendToClient(Socket s,MessageProtocol msg)
    {
        try
        {
            s.Send(msg.GetBytes());
           // s.Send(System.Text.Encoding.Default.GetBytes("&"));
        }
        catch(Exception ex) 
        {
            mainForm.LogOnTextbox("Message sending failed: "+ex);
        }
        
    }


    public static void UpdateData()
    {
       
        while (true)
        {
            Thread.Sleep(50);
         
            List<EntityData> entityList = new List<EntityData>();
            lock (EntityBeh.worldEntities)
            {
            for(int i=0;i<EntityBeh.worldEntities.Count;i++)
            {
                    EntityBeh e = EntityBeh.worldEntities[i];
                e.OnUpdate();
                entityList.Add(e.ToEntityData());
            }
            }
            for(int i=0;i<allUserData.Count;i++)
            {
               
                Random rand = new Random();
                if (rand.Next(100) > 98&&EntityBeh.worldEntities.Count<70)
                {
                    Vector2 monsterSpawnPos=new Vector2(allUserData[i].posX+ rand.Next(-40,40), allUserData[i].posZ + rand.Next(-40, 40));
                    EntityBeh.SpawnNewEntity(new Vector3(monsterSpawnPos.X, Chunk.GetBlockLandingPoint(monsterSpawnPos), monsterSpawnPos.Y), 0, 0, 0, 0);
                }
                

            }
            AppendMessage(null, new MessageProtocol(140, MessagePackSerializer.Serialize("update")));
            AppendMessage(null, new MessageProtocol(142, MessagePackSerializer.Serialize(entityList, lz4Options)));



        }
    }
    public static async void UserLogin(Socket s,byte[] data)
    {
        UserData u = MessagePackSerializer.Deserialize<UserData>(data);
        int idx = allUserData.FindIndex(delegate (UserData cl) { return cl.userName == u.userName; });
        if (idx!=-1)
        {
            SendToClient(s, new MessageProtocol(136, MessagePackSerializer.Serialize("Failed")));
            await Task.Delay(100);
            s.Close();
        }
        else
        {
            mainForm.LogOnTextbox(s.ToString()+"Logged in");
            SendToClient(s, new MessageProtocol(136, MessagePackSerializer.Serialize("Success")));
            allClientSocketsOnline.Add(s);
            allUserData.Add(u);
       //     SendToClient(s, new Message("userCount", MessagePackSerializer.Serialize(allClientSocketsOnline.Count.ToString())));
            CastToAllClients(new MessageProtocol(135, MessagePackSerializer.Serialize(allUserData)));
        }
    }
    public static void CastToAllClients(MessageProtocol msg)
    {
        
            try
            {
                for(int i=0;i<allClientSocketsOnline.Count;i++)
                {

                allClientSocketsOnline[i].Send(msg.GetBytes());
                   

                    //   socket.Send(System.Text.Encoding.Default.GetBytes("&"));
                }
            }
            catch
            {
             
            }
       
        
       
    }
   /* static void ServerConsoleControl()
    {
        mainForm.LogOnTextbox("Press 1 to list players,press 2 to list chunks,press 3 to get current message count,press 4 to stop server");
        while(true)
        {

            switch (Console.ReadKey().KeyChar)
        {
                case '1':
            mainForm.LogOnTextbox("\n");
                    try
                    {
                foreach(UserData u in allUserData)
            {
                mainForm.LogOnTextbox(JsonConvert.SerializeObject(u));
            }
                    }
                    catch
                    {
                        mainForm.LogOnTextbox("User list is was modified");
                    }
           
                    break;
                case '2':
                    foreach(KeyValuePair<Vector2Int,Chunk> c in chunks)
                    {
                        mainForm.LogOnTextbox(JsonConvert.SerializeObject(c.Key));
                    }
                        
                    
                    break;
                case '3':
                    mainForm.LogOnTextbox("\nMessage Count:"+toDoList.Count.ToString());
                    mainForm.LogOnTextbox("\nMessage Count:" + toDoList2.Count.ToString());
                    break;
                case '4':
                    StopServer();
                    break;

            }
        }
      

                
    }
   */

    static async void ExecuteToDoList(PriorityQueue<KeyValuePair<Socket,MessageProtocol>,int> listToExecute,object objLock)
    {
        while (true)
        {
       
                Socket s;
                MessageProtocol message;
                Thread.Sleep(5);
                //  byte[] recieve = new byte[1024];
                if (listToExecute.Count > 0)
                {
                lock (objLock)
                {

               
                    message = listToExecute.Peek().Value;
                    if (message == null)
                    {
                        mainForm.LogOnTextbox("Empty Message");
                        listToExecute.Dequeue();
                        continue;
                    }
                    s = listToExecute.Peek().Key;
                    listToExecute.Dequeue();




                

                switch (message.Command)
                {
                    //message content type:Vector2int
                    case 134:
                        Vector2Int v = MessagePackSerializer.Deserialize<Vector2Int>(message.MessageData);
                         //   mainForm.LogOnTextbox("chunkdata");
                            lock (chunkLock)
                            {
                            if (!chunks.ContainsKey(v))
                            {
                            Chunk c = new Chunk(v);
                            chunks.TryAdd(v, c);
                            c.InitMap(v);
                            //   world = new Chunk(new Vector2Int(0, 0));
                            //  world.InitMap(new Vector2Int(0, 0));
                            SendToClient(s, new MessageProtocol(128, MessagePackSerializer.Serialize(c.ChunkToChunkData())));//推送至客户端
                            }
                            else
                            {
                            SendToClient(s, new MessageProtocol(128, MessagePackSerializer.Serialize(chunks[v].ChunkToChunkData())));
                            }
                            }
                        

                        break;
                        //message content type:blockmodifydata
                        case 133:
                    //        mainForm.LogOnTextbox("updateinternal");
                            BlockModifyData binternal = MessagePackSerializer.Deserialize<BlockModifyData>(message.MessageData);
                            if (GetChunk(Vec3ToChunkPos(new Vector3(binternal.x, binternal.y, binternal.z))) == null)
                            {
                               //SendToClient(s, new Message("ChunkNotFound", MessagePackSerializer.Serialize("ChunkNotFound")));
                            }
                            else
                            {
                                Vector2Int x = Vec3ToChunkPos(new Vector3(binternal.x, binternal.y, binternal.z));
                                Vector3 chunkSpacePos = new Vector3(binternal.x, binternal.y, binternal.z) - new Vector3(x.x, 0, x.y);
                                ParticleData pd;
                                if (binternal.convertType == 0)
                                {
                                    pd = new ParticleData((binternal.x)+0.5f,(binternal.y) + 0.5f, (binternal.z) + 0.5f, GetChunk(x).map[(int)chunkSpacePos.X, (int)chunkSpacePos.Y, (int)chunkSpacePos.Z],false);
                            //        mainForm.LogOnTextbox("Emit");
                                }
                                else
                                {
                                    pd = new ParticleData(binternal.x, binternal.y, binternal.z, binternal.convertType, true);
                                }
                                GetChunk(x).map[(int)chunkSpacePos.X, (int)chunkSpacePos.Y, (int)chunkSpacePos.Z] = binternal.convertType;
                                GetChunk(x).isModifiedInGame = true;
                                    //     if (binternal.convertType == 0 && pd != null)
                                    //   {
                                    CastToAllClients(new MessageProtocol(138, MessagePackSerializer.Serialize(pd)));

                              //  }
                                CastToAllClients(new MessageProtocol(139, MessagePackSerializer.Serialize(binternal)));

                            }





                            break;
                    //message content type:blockmodifydata
                    case 132:
                      //      mainForm.LogOnTextbox("updatechunk");
                        BlockModifyData b = MessagePackSerializer.Deserialize<BlockModifyData>(message.MessageData);
                        if (GetChunk(Vec3ToChunkPos(new Vector3(b.x, b.y, b.z))) == null)
                        {
                 //           SendToClient(s, new MessageProtocol("ChunkNotFound", MessagePackSerializer.Serialize("ChunkNotFound")));
                        }
                        else
                        {
                            Vector2Int x = Vec3ToChunkPos(new Vector3(b.x, b.y, b.z));
                            Vector3 chunkSpacePos = new Vector3(b.x, b.y, b.z) - new Vector3(x.x, 0, x.y);
                                ParticleData pd;
                                if (b.convertType == 0)
                                {
                                     pd = new ParticleData(b.x,b.y,b.z, GetChunk(x).map[(int)chunkSpacePos.X, (int)chunkSpacePos.Y, (int)chunkSpacePos.Z], false);
                      //              mainForm.LogOnTextbox("Emit");
                                }
                                else
                                {
                                    pd = new ParticleData(b.x, b.y, b.z,b.convertType, true);
                                    //pd = null;
                                }
                                GetChunk(x).map[(int)chunkSpacePos.X, (int)chunkSpacePos.Y, (int)chunkSpacePos.Z] = b.convertType;
                                GetChunk(x).isModifiedInGame = true;
                                //     if (b.convertType == 0 && pd != null)
                                //      {
                                CastToAllClients(new MessageProtocol(138, MessagePackSerializer.Serialize(pd)));
                                    
                          //      }
                                CastToAllClients(new MessageProtocol(139, MessagePackSerializer.Serialize(b)));
                                GetChunk(x).BFSInit((int)chunkSpacePos.X, (int)chunkSpacePos.Y, (int)chunkSpacePos.Z, 7,0);
                        }




                        break;
                    case 130:
                        UserLogout(s);
                        break;
                    //message content type:userdata
                    case 131:
                        UserData u = MessagePackSerializer.Deserialize<UserData>(message.MessageData);

                            lock (userDataLock)
                            {
                                int idx = allUserData.FindIndex(delegate (UserData pl) { return pl.userName == u.userName; });
                                if (idx != -1)
                                {
                                    allUserData[idx] = u;

                                }
                                CastToAllClients(new MessageProtocol(135, MessagePackSerializer.Serialize(allUserData, lz4Options)));
                            }
                            break;
                    case 140:
                        CastToAllClients(new MessageProtocol(141, MessagePackSerializer.Serialize("update", lz4Options)));

                        break;
                     case 142:
                            CastToAllClients(new MessageProtocol(142, message.MessageData));
                            break;
                     case 143:
                            HurtEntityData hed = MessagePackSerializer.Deserialize<HurtEntityData>(message.MessageData);
                            EntityBeh.HurtEntity(hed.entityID, hed.hurtValue);
                            break;
                    case 129:
                        UserLogin(s, message.MessageData);
                        //  s.Send(System.Text.Encoding.Default.GetBytes(JsonConvert.SerializeObject(new Message("LoginReturn", "Hello"))));

                        break;
                    default:
                     //   s.Send(System.Text.Encoding.Default.GetBytes(JsonConvert.SerializeObject(new Message("UnknownMessage", MessagePackSerializer.Serialize("UnknownMessage", lz4Options))) + '&'));
                        break;
                }
                }
                   
            }



        }
    }
   /* static async void ExecuteToDoList2()
    {
        while (true)
        {

            Socket s;
            Message message;
            Thread.Sleep(1);
            //  byte[] recieve = new byte[1024];
            if (toDoList2.Count > 0)
            {
                lock (listLock2)
                {


                    message = toDoList2.Peek().Value;
                    if (message == null)
                    {
                        mainForm.LogOnTextbox("Empty Message");
                        toDoList2.Dequeue();
                        continue;
                    }
                    s = toDoList2.Peek().Key;
                    toDoList2.Dequeue();






                    switch (message.messageType)
                    {
                        //message content type:Vector2int
                        case "ChunkGen":
                            Vector2Int v = MessagePackSerializer.Deserialize<Vector2Int>(message.messageContent);
                            lock (chunkLock)
                            {
                            if (!chunks.ContainsKey(v))
                            {
                                Chunk c = new Chunk(v);
                                chunks.TryAdd(v, c);
                                c.InitMap(v);
                                //   world = new Chunk(new Vector2Int(0, 0));
                                //  world.InitMap(new Vector2Int(0, 0));
                                SendToClient(s, new Message(128, MessagePackSerializer.Serialize(c.ChunkToChunkData(), lz4Options)));
                            }
                            else
                            {
                                SendToClient(s, new Message(128, MessagePackSerializer.Serialize(chunks[v].ChunkToChunkData(),lz4Options)));
                            }
                            }
                            

                            break;
                        //message content type:blockmodifydata
                        case "UpdateChunk":
                            BlockModifyData b = MessagePackSerializer.Deserialize<BlockModifyData>(message.messageContent);
                            if (GetChunk(Vec3ToChunkPos(new Vector3(b.x, b.y, b.z))) == null)
                            {
                                SendToClient(s, new Message("ChunkNotFound", MessagePackSerializer.Serialize("ChunkNotFound")));
                            }
                            else
                            {
                                Vector2Int x = Vec3ToChunkPos(new Vector3(b.x, b.y, b.z));
                                Vector3 chunkSpacePos = new Vector3(b.x, b.y, b.z) - new Vector3(x.x, 0, x.y);
                                GetChunk(x).map[(int)chunkSpacePos.X, (int)chunkSpacePos.Y, (int)chunkSpacePos.Z] = b.convertType;
                                CastToAllClients(new Message(128, MessagePackSerializer.Serialize(GetChunk(x).ChunkToChunkData(), lz4Options)));
                            }




                            break;
                        case 130:
                            UserLogout(s);
                            break;
                        //message content type:userdata
                        case "UpdateUser":
                            UserData u = MessagePackSerializer.Deserialize<UserData>(message.messageContent);

                            lock (userDataLock)
                            {
                            int idx = allUserData.FindIndex(delegate (UserData pl) { return pl.userName == u.userName; });
                            if (idx != -1)
                            {
                                allUserData[idx] = u;

                            }
                            CastToAllClients(new Message("ReturnAllUserData", MessagePackSerializer.Serialize(allUserData, lz4Options)));
                            }
                            
                            break;
                        case 140:
                            CastToAllClients(new Message(141, MessagePackSerializer.Serialize("update", lz4Options)));

                            break;
                        case 129:
                                 UserLogin(s, message.messageContent);
                            //  s.Send(System.Text.Encoding.Default.GetBytes(JsonConvert.SerializeObject(new Message("LoginReturn", "Hello"))));

                            break;
                        default:
                            s.Send(System.Text.Encoding.Default.GetBytes(JsonConvert.SerializeObject(new Message("UnknownMessage", MessagePackSerializer.Serialize("UnknownMessage", lz4Options))) + '&'));
                            break;
                    }
                }

            }



        }
    }*/
   public static void StopServer()
    {
        lock (allClientSocketsOnlineLock)
        {
        for(int i=0;i<allClientSocketsOnline.Count;i++)
        {
                UserLogout(allClientSocketsOnline[i]);
        }
        }
       
        SaveWorldData();
        Environment.Exit(0);
    }
    public static void LoadApp()
    {
        Chunk.biomeNoiseGenerator.SetFrequency(0.002f);
        mainForm.LogOnTextbox("Reading World Data...");
        Task t = new Task(() => ReadJson());
        t.RunSynchronously();
        serverSocket.Bind(new IPEndPoint(ip, port));
        Thread waiterThread = new Thread(() => socketWait(serverSocket));
        waiterThread.Start();
        Thread updateThread = new Thread(() => UpdateData());
        updateThread.Start();

      /*  Thread executeThread = new Thread(() => ExecuteToDoList(toDoList, listLock));    
        executeThread.Start();
        Thread executeThread2 = new Thread(() => ExecuteToDoList(toDoList2, listLock2));
        executeThread2.Start();*/
      foreach(var tdl in toDoLists)
        {
          //  Debug.WriteLine(toDoLists.Count);
         
            Thread executeThread = new Thread(() => ExecuteToDoList(tdl, listLocks[toDoLists.IndexOf(tdl)]));
            executeThread.Start();
        }
        mainForm.LogOnTextbox("Server Started! IP:"+ip.ToString()+":"+port.ToString());
      //  Thread ServerConsoleControlThread = new Thread(() => ServerConsoleControl());
     //   ServerConsoleControlThread.Start();
    }
    static void Main(string[] args)
    {   Application.EnableVisualStyles();
        Application.SetCompatibleTextRenderingDefault(false);
        mainForm = new Form1();
        Application.Run(mainForm);
        
    /*    Task t= new Task(()=> ReadJson());
        t.RunSynchronously();
        serverSocket.Bind(new IPEndPoint(ip, port));
        Thread waiterThread = new Thread(()=>socketWait(serverSocket));
        waiterThread.Start();
        Thread updateThread = new Thread(() => UpdateData());
        updateThread.Start();
      
        

        Thread executeThread = new Thread(() => ExecuteToDoList(toDoList,listLock));
        executeThread.Start();
        Thread executeThread2 = new Thread(() => ExecuteToDoList(toDoList2, listLock2));
        executeThread2.Start();
        mainForm.LogOnTextbox("Server Started!");*/
       // Thread ServerConsoleControlThread = new Thread(() => ServerConsoleControl());
      //  ServerConsoleControlThread.Start();
        
        //2.开启窗口的消息循环，初始化并启动Form1窗口
       


        //   Thread executeThread2 = new Thread(() => ExecuteToDoList());
        //    executeThread2.Start();
        //  return;
    }
}