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
public class UserData {
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
public class Program
{
    static MessagePackSerializerOptions lz4Options = MessagePackSerializerOptions.Standard.WithCompression(MessagePackCompression.Lz4BlockArray);
    static object allClientSocketsOnlineLock = new object();
    static Chunk world;
    static Dictionary<Vector2Int,Chunk> chunks= new Dictionary<Vector2Int,Chunk>();
    static object listLock = new object();
    static PriorityQueue<KeyValuePair<Socket, Message>, int> toDoList2 = new PriorityQueue<KeyValuePair<Socket, Message>, int>();//双线程处理消息
    static PriorityQueue<KeyValuePair<Socket,Message>,int> toDoList=new PriorityQueue<KeyValuePair<Socket,Message>,int>();
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
            CastToAllClients(new Message("ReturnAllUserData", MessagePackSerializer.Serialize(allUserData)));
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
    public static Vector2Int Vec3ToChunkPos(Vector3 pos)
    {
        Vector3 tmp = pos;
        tmp.X = MathF.Floor(tmp.X / (float)Chunk.chunkWidth) * Chunk.chunkWidth;
        tmp.Z = MathF.Floor(tmp.Z / (float)Chunk.chunkWidth) * Chunk.chunkWidth;
        Vector2Int value = new Vector2Int((int)tmp.X, (int)tmp.Z);
        Console.WriteLine(value.x+" "+value.y+"\n");
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
        byte[] bb = new byte[102400];
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
                int count =s.Receive(bb);
                if (count>65536)
                {
                    UserLogout(s);
                }
                string str = System.Text.Encoding.UTF8.GetString(bb.ToArray(),0,count);
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
                                Message m = JsonConvert.DeserializeObject<Message>(x);
                                switch (m.messageType)
                                {
                                    case "UpdateChunk":
                                        toDoList2.Enqueue(new KeyValuePair<Socket, Message>(s, JsonConvert.DeserializeObject<Message>(x)), 0);
                                        break;
                                    case "ChunkGen":
                                        toDoList2.Enqueue(new KeyValuePair<Socket, Message>(s, JsonConvert.DeserializeObject<Message>(x)), 1);
                                        break;
                                    case "UpdateUser":
                                        toDoList2.Enqueue(new KeyValuePair<Socket, Message>(s, JsonConvert.DeserializeObject<Message>(x)), 10);
                                        break;
                                    default:
                                        toDoList2.Enqueue(new KeyValuePair<Socket, Message>(s, JsonConvert.DeserializeObject<Message>(x)), 1);
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
                    }
                 
        }
        catch (Exception ex) { Console.WriteLine("Connection stopped : "+ex.ToString());UserLogout(s);
                break; }
        }
      

    }
    public static void SendToClient(Socket s,Message msg)
    {
        try
        {
            s.Send(System.Text.Encoding.Default.GetBytes(JsonConvert.SerializeObject(msg)+ '&'));
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
                
            toDoList.Enqueue(new KeyValuePair<Socket,Message>(null, new Message("UpdateAllUser", MessagePackSerializer.Serialize("update"))),10);
                
                    
            }
            }
            else
            {
                lock (listLock2)
                {

                    toDoList2.Enqueue(new KeyValuePair<Socket, Message>(null, new Message("UpdateAllUser", MessagePackSerializer.Serialize("update"))), 10);


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
            SendToClient(s, new Message("LoginReturn", MessagePackSerializer.Serialize("Failed")));
        await Task.Delay(100);
            s.Close();
        }
        else
        {
           
            SendToClient(s, new Message("LoginReturn", MessagePackSerializer.Serialize("Success")));
            allClientSocketsOnline.Add(s);
            allUserData.Add(u);
            SendToClient(s, new Message("userCount", MessagePackSerializer.Serialize(allClientSocketsOnline.Count.ToString())));
            CastToAllClients(new Message("ReturnAllUserData",MessagePackSerializer.Serialize(allUserData)));
        }
    }
    public static void CastToAllClients(Message msg)
    {
        lock(allClientSocketsOnlineLock)
        {
            try
            {
                foreach (Socket socket in allClientSocketsOnline)
                {
                    if (socket != null && socket.Connected == true)
                    {
                        socket.Send(System.Text.Encoding.Default.GetBytes(JsonConvert.SerializeObject(msg) + '&'));
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
                        socket.Send(System.Text.Encoding.Default.GetBytes(JsonConvert.SerializeObject(msg) + '&'));
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


    static async void ExecuteToDoList()
    {
        while (true)
        {
       
                Socket s;
                Message message;
                Thread.Sleep(1);
                //  byte[] recieve = new byte[1024];
                if (toDoList.Count > 0)
                {
                lock (listLock)
                {

               
                    message = toDoList.Peek().Value;
                    if (message == null)
                    {
                        Console.WriteLine("Empty Message");
                        toDoList.Dequeue();
                        continue;
                    }
                    s = toDoList.Peek().Key;
                    toDoList.Dequeue();




                

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
                            SendToClient(s, new Message("WorldData", MessagePackSerializer.Serialize(c.ChunkToChunkData())));//推送至客户端
                            }
                            else
                            {
                            SendToClient(s, new Message("WorldData", MessagePackSerializer.Serialize(chunks[v].ChunkToChunkData())));
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
                            CastToAllClients(new Message("WorldData", MessagePackSerializer.Serialize(GetChunk(x).ChunkToChunkData())));
                        }




                        break;
                    case "LogOut":
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
                    case "UpdateAllUser":
                        CastToAllClients(new Message("ClientUpdateUser", MessagePackSerializer.Serialize("update", lz4Options)));

                        break;
                    case "Login":
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
    }
    static async void ExecuteToDoList2()
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
                                SendToClient(s, new Message("WorldData", MessagePackSerializer.Serialize(c.ChunkToChunkData(), lz4Options)));
                            }
                            else
                            {
                                SendToClient(s, new Message("WorldData", MessagePackSerializer.Serialize(chunks[v].ChunkToChunkData(),lz4Options)));
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
                                CastToAllClients(new Message("WorldData", MessagePackSerializer.Serialize(GetChunk(x).ChunkToChunkData(), lz4Options)));
                            }




                            break;
                        case "LogOut":
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
                        case "UpdateAllUser":
                            CastToAllClients(new Message("ClientUpdateUser", MessagePackSerializer.Serialize("update", lz4Options)));

                            break;
                        case "Login":
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
    }
    static void Main(string[] args)
    {
        serverSocket.Bind(new IPEndPoint(ip, port));
        Thread waiterThread = new Thread(()=>socketWait(serverSocket));
        waiterThread.Start();
        Thread updateThread = new Thread(() => UpdateData());
        updateThread.Start();
        Thread ServerConsoleControlThread = new Thread(() => ServerConsoleControl());
        ServerConsoleControlThread.Start();

        Thread executeThread = new Thread(() => ExecuteToDoList());
        executeThread.Start();
        Thread executeThread2 = new Thread(() => ExecuteToDoList2());
        executeThread2.Start();
        //   Thread executeThread2 = new Thread(() => ExecuteToDoList());
        //    executeThread2.Start();
        //  return;
    }
}