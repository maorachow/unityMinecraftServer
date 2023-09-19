using System.Net;
using System.Net.Security;
using System.Net.Sockets;
using Newtonsoft.Json;
using System.Threading;
using System.Text;
using MyMinecraftServer;
using System.Numerics;
using System.Globalization;
using Microsoft.VisualBasic;
using System;
using System.Security.Cryptography.X509Certificates;
using System.Dynamic;
using System.Linq;
//using Utf8Json;
using System.Security.Cryptography;
using MessagePack;
using System.Diagnostics;

[MessagePackObject]
public class ChunkData
{
    [Key(0)]
    public int[,,] map;
    [Key(1)]
    public Vector2Int chunkPos = new Vector2Int(0, 0);
    public ChunkData(int[,,] map, Vector2Int chunkPos)
    {
        this.map = map;
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

    public ParticleData(float posX, float posY, float posZ, int type)
    {
        this.posX = posX;
        this.posY = posY;
        this.posZ = posZ;
        this.type = type;
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
public class Program
{
    static MessagePackSerializerOptions lz4Options = MessagePackSerializerOptions.Standard.WithCompression(MessagePackCompression.Lz4BlockArray);
    static object allClientSocketsOnlineLock = new object();
    static Chunk world;
    static Dictionary<Vector2Int,Chunk> chunks= new Dictionary<Vector2Int,Chunk>();
    static object listLock = new object();
    public static PriorityQueue<KeyValuePair<Socket, MessageProtocol>, int> toDoList2 = new PriorityQueue<KeyValuePair<Socket, MessageProtocol>, int>();//双线程处理消息
    public static PriorityQueue<KeyValuePair<Socket, MessageProtocol>,int> toDoList=new PriorityQueue<KeyValuePair<Socket, MessageProtocol>,int>();
    static IPAddress ip = IPAddress.Parse("0.0.0.0");
    static int port = 11111;
    static Socket serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
    static List<Socket> allClientSocketsOnline=new List<Socket>();
    static List<UserData> allUserData=new List<UserData>();
    static object listLock2 = new object();
    static object chunkLock = new object();
    static object userDataLock = new object();
    public static void UserLogout(Socket socket) {
        int index = allClientSocketsOnline.FindIndex(delegate (Socket cl) { return cl == socket; });
        if (index!=-1)
        {
            allClientSocketsOnline.RemoveAt(index);
            allUserData.RemoveAt(index);
            Console.WriteLine(socket.RemoteEndPoint.ToString() + "  logged out");
            CastToAllClients(new MessageProtocol(135, MessagePackSerializer.Serialize(allUserData)));
            socket.Close();
        }
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
        Console.WriteLine("connected");
        Console.WriteLine(socket.ToString());
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
    public static Vector2Int Vec3ToChunkPos(Vector3 pos)
    {
        Vector3 tmp = pos;
        tmp.X = MathF.Floor(tmp.X / (float)Chunk.chunkWidth) * Chunk.chunkWidth;
        tmp.Z = MathF.Floor(tmp.Z / (float)Chunk.chunkWidth) * Chunk.chunkWidth;
        Vector2Int value = new Vector2Int((int)tmp.X, (int)tmp.Z);
      //  Console.WriteLine(value.x+" "+value.y+"\n");
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
    public static async void RecieveClient(object socket)
    {
        Socket s = (Socket)socket;
      //  byte[] bb = new byte[102400];
      //  ArraySegment<byte> b= new ArraySegment<byte>(bb);
        while(true)
        {
            try
            {//public int Receive (System.Collections.Generic.IList<ArraySegment<byte>> buffers);
                if (s == null||s.Connected==false)
                {
                    Console.WriteLine("Recieve client failed:socket closed");
                    return;
                }
           //     int count =s.Receive(bb);
               
                MessageProtocol mp = null;
                int ReceiveLength = 0;
                byte[] staticReceiveBuffer = new byte[102400];  // 接收缓冲区(固定长度)
                byte[] dynamicReceiveBuffer = new byte[] { };  // 累加数据缓存(不定长)
             
                    ReceiveLength = s.Receive(staticReceiveBuffer);  // 同步接收数据
                    dynamicReceiveBuffer = MessageProtocol.CombineBytes(dynamicReceiveBuffer, 0, dynamicReceiveBuffer.Length, staticReceiveBuffer, 0, ReceiveLength);  // 将之前多余的数据与接收的数据合并,形成一个完整的数据包
                    if (ReceiveLength <= 0)  // 如果接收到的数据长度小于0(通常表示socket已断开,但也不一定,需要进一步判断,此处可以忽略)
                    {
                        Console.WriteLine("收到0字节数据");
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
                     //       Console.WriteLine("Message:"+mp.Command);
                            dynamicReceiveBuffer = mp.MoreData;  // 将拆包后得出多余的字节付给缓存变量,以待下一次循环处理数据时使用,若下一次循环缓存数据长度不能构成一个完整的数据包则不进入循环跳到外层循环继续接收数据并将本次得出的多余数据与之合并重新拆包,依次循环。
                            headInfo = MessageProtocol.GetHeadInfo(dynamicReceiveBuffer);  // 从缓存中解读出下一次数据所需要的协议头信息,已准备下一次拆包循环,如果数据长度不能构成协议头所需的长度,拆包结果为0,下一次循环则不能成功进入,跳到外层循环继续接收数据合并缓存形成一个完整的数据包
                            if (toDoList.Count > toDoList2.Count)
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
                            }
                            else
                            {
                                lock (listLock)
                                {
                                    //   object o= JsonConvert.DeserializeObject<object>(x);
                                   
                                    switch (mp.Command)
                                    {
                                        case 132:
                                            toDoList.Enqueue(new KeyValuePair<Socket, MessageProtocol>(s, mp), 0);
                                            break;
                                        case 134:
                                            toDoList.Enqueue(new KeyValuePair<Socket, MessageProtocol>(s, mp), 1);
                                            break;
                                        case 131:
                                            toDoList.Enqueue(new KeyValuePair<Socket, MessageProtocol>(s, mp), 10);
                                            break;
                                        default:
                                            toDoList.Enqueue(new KeyValuePair<Socket, MessageProtocol>(s, mp), 1);
                                            break;
                                    }

                                }
                            }


                        } // 拆包循环结束
                    }
             
                /* string str = System.Text.Encoding.UTF8.GetString(bb.ToArray(),0,count);
                 foreach (string x in str.Split('&'))
                 {
                       if (x.Length > 0)
                         {
                         //         Console.WriteLine(x);
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

                 //  Console.WriteLine("Recieved Message:" + JsonConvert.DeserializeObject<Message>(str));

             }
                     }*/

            }
        catch (Exception ex) { Console.WriteLine("Connection stopped : "+ex.ToString());UserLogout(s);
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
            Console.WriteLine("Message sending failed: "+ex);
        }
        
    }
    public static void UpdateData()
    {
       
        while (true)
        {
            Thread.Sleep(100);
            //  Console.WriteLine("Update");
            if (toDoList.Count < toDoList2.Count)
            {
            lock(listLock)
            {
                
            toDoList.Enqueue(new KeyValuePair<Socket,MessageProtocol>(null, new MessageProtocol(140, MessagePackSerializer.Serialize("update"))),10);
                
                    
            }
            }
            else
            {
                lock (listLock2)
                {

                    toDoList2.Enqueue(new KeyValuePair<Socket, MessageProtocol>(null, new MessageProtocol(140, MessagePackSerializer.Serialize("update"))), 10);


                }
            }
         
            
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
            Console.WriteLine(s.ToString()+"Logged in");
            SendToClient(s, new MessageProtocol(136, MessagePackSerializer.Serialize("Success")));
            allClientSocketsOnline.Add(s);
            allUserData.Add(u);
       //     SendToClient(s, new Message("userCount", MessagePackSerializer.Serialize(allClientSocketsOnline.Count.ToString())));
            CastToAllClients(new MessageProtocol(135, MessagePackSerializer.Serialize(allUserData)));
        }
    }
    public static void CastToAllClients(MessageProtocol msg)
    {
        lock(allClientSocketsOnlineLock)
        {
            try
            {
                foreach (Socket socket in allClientSocketsOnline)
                {
                    if (socket != null && socket.Connected == true)
                    {
                        socket.Send(msg.GetBytes());
                    }

                    //   socket.Send(System.Text.Encoding.Default.GetBytes("&"));
                }
            }
            catch
            {
                foreach (Socket socket in allClientSocketsOnline)
                {
                    if (socket != null && socket.Connected == true)
                    {
                        socket.Send(msg.GetBytes());
                    }

                    //   socket.Send(System.Text.Encoding.Default.GetBytes("&"));
                }
            }
       
        }
       
    }
    static void ServerConsoleControl()
    {
        Console.WriteLine("Press 1 to list players,press 2 to list chunks,press 3 to get current message count");
        while(true)
        {

            switch (Console.ReadKey().KeyChar)
        {
                case '1':
            Console.WriteLine("\n");
                    try
                    {
                foreach(UserData u in allUserData)
            {
                Console.WriteLine(JsonConvert.SerializeObject(u));
            }
                    }
                    catch
                    {
                        Console.WriteLine("User list is was modified");
                    }
           
                    break;
                case '2':
                    foreach(KeyValuePair<Vector2Int,Chunk> c in chunks)
                    {
                        Console.WriteLine(JsonConvert.SerializeObject(c.Key));
                    }
                        
                    
                    break;
                case '3':
                    Console.WriteLine("\nMessage Count:"+toDoList.Count.ToString());
                    Console.WriteLine("\nMessage Count:" + toDoList2.Count.ToString());
                    break;
                        
           
        }
        }
      

                
    }


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
                        Console.WriteLine("Empty Message");
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
                            Console.WriteLine("updateinternal");
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
                                    pd = new ParticleData((binternal.x)+0.5f,(binternal.y) + 0.5f, (binternal.z) + 0.5f, GetChunk(x).map[(int)chunkSpacePos.X, (int)chunkSpacePos.Y, (int)chunkSpacePos.Z]);
                                    Console.WriteLine("Emit");
                                }
                                else
                                {
                                    pd = null;
                                }
                                GetChunk(x).map[(int)chunkSpacePos.X, (int)chunkSpacePos.Y, (int)chunkSpacePos.Z] = binternal.convertType;
                           
                                if (binternal.convertType == 0 && pd != null)
                                {
                                    CastToAllClients(new MessageProtocol(138, MessagePackSerializer.Serialize(pd)));

                                }
                                CastToAllClients(new MessageProtocol(139, MessagePackSerializer.Serialize(binternal)));

                            }





                            break;
                    //message content type:blockmodifydata
                    case 132:
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
                                     pd = new ParticleData(b.x,b.y,b.z, GetChunk(x).map[(int)chunkSpacePos.X, (int)chunkSpacePos.Y, (int)chunkSpacePos.Z]);
                                    Console.WriteLine("Emit");
                                }
                                else
                                {
                                    pd = null;
                                }
                                GetChunk(x).map[(int)chunkSpacePos.X, (int)chunkSpacePos.Y, (int)chunkSpacePos.Z] = b.convertType;
                               
                                if (b.convertType == 0 && pd != null)
                                {
                                    CastToAllClients(new MessageProtocol(138, MessagePackSerializer.Serialize(pd)));
                                    
                                }
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
                        Console.WriteLine("Empty Message");
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
    static void Main(string[] args)
    {
        serverSocket.Bind(new IPEndPoint(ip, port));
        Thread waiterThread = new Thread(()=>socketWait(serverSocket));
        waiterThread.Start();
        Thread updateThread = new Thread(() => UpdateData());
        updateThread.Start();
        Thread ServerConsoleControlThread = new Thread(() => ServerConsoleControl());
        ServerConsoleControlThread.Start();

        Thread executeThread = new Thread(() => ExecuteToDoList(toDoList,listLock));
        executeThread.Start();
        Thread executeThread2 = new Thread(() => ExecuteToDoList(toDoList2, listLock2));
        executeThread2.Start();
        //   Thread executeThread2 = new Thread(() => ExecuteToDoList());
        //    executeThread2.Start();
        //  return;
    }
}