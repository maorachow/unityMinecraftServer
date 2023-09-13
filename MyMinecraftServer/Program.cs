using System.Net;
using System.Net.Security;
using System.Net.Sockets;
//using Newtonsoft.Json;
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
using Utf8Json;
using System.Security.Cryptography;

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


public class UserData { 
    public float posX;
    public float posY;
    public float posZ;
    public float rotY;
    public string userName;



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
class Program
{
    
    static Chunk world;
    static Dictionary<Vector2Int,Chunk> chunks= new Dictionary<Vector2Int,Chunk>();
    static object listLock = new object();
    static PriorityQueue<KeyValuePair<Socket, Message>, int> toDoList2 = new PriorityQueue<KeyValuePair<Socket, Message>, int>();
    static PriorityQueue<KeyValuePair<Socket,Message>,int> toDoList=new PriorityQueue<KeyValuePair<Socket,Message>,int>();
    static IPAddress ip = IPAddress.Parse("127.0.0.1");
    static int port = 11111;
    static Socket serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
    static List<Socket> allClientSocketsOnline=new List<Socket>();
    static List<UserData> allUserData=new List<UserData>();
    public static void UserLogout(Socket socket) {
        int index = allClientSocketsOnline.FindIndex(delegate (Socket cl) { return cl == socket; });
        if (index!=-1)
        {
            allClientSocketsOnline.RemoveAt(index);
            allUserData.RemoveAt(index);
            Console.WriteLine(socket.RemoteEndPoint.ToString() + "  logged out");
            CastToAllClients(new Message("ReturnAllUserData", JsonSerializer.ToJsonString(allUserData)));
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
                           lock (listLock)
                          {
                            //   object o= JsonSerializer.Deserialize<object>(x);
                            if (x.Length > 65536)
                            {
                                UserLogout(s);
                            }
                            Message m = JsonSerializer.Deserialize<Message>(x);
                        switch (m.messageType)
                        {
                            case "UpdateChunk":
                                toDoList.Enqueue(new KeyValuePair<Socket, Message>(s, JsonSerializer.Deserialize<Message>(x)),0);
                                break;
                            case "GenChunk":
                                toDoList.Enqueue(new KeyValuePair<Socket, Message>(s, JsonSerializer.Deserialize<Message>(x)), 1);
                                break;
                            case "UpdateUser":
                                toDoList.Enqueue(new KeyValuePair<Socket, Message>(s, JsonSerializer.Deserialize<Message>(x)), 10);
                                break;
                                default:
                                    toDoList.Enqueue(new KeyValuePair<Socket, Message>(s, JsonSerializer.Deserialize<Message>(x)), 1);
                                    break;
                        }
                    
                    }
                //  Console.WriteLine("Recieved Message:" + JsonSerializer.Deserialize<Message>(str));
               
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
            s.Send(System.Text.Encoding.Default.GetBytes(JsonSerializer.ToJsonString(msg)+ '&'));
           // s.Send(System.Text.Encoding.Default.GetBytes("&"));
        }
        catch
        {
            Console.WriteLine("Message sending failed");
        }
        
    }
    public static void UpdateData()
    {
        int time = 0;
        while (true)
        {
            time = (time + 20) % 100000;
            Thread.Sleep(20);
          //  Console.WriteLine("Update");
          lock(listLock)
            {
                if (time % 20 == 0)
                {
            toDoList.Enqueue(new KeyValuePair<Socket,Message>(null, new Message("UpdateAllUser", "update")),10);
                }
                    
            }
            
        }
    }
    public static void UserLogin(Socket s,string data)
    {
        UserData u = JsonSerializer.Deserialize<UserData>(data);
        int idx = allUserData.FindIndex(delegate (UserData cl) { return cl.userName == u.userName; });
        if (idx!=-1)
        {
            SendToClient(s, new Message("LoginReturn", "Failed"));
        
            s.Close();
        }
        else
        {
           
            SendToClient(s, new Message("LoginReturn", "Success"));
            allClientSocketsOnline.Add(s);
            allUserData.Add(u);
            SendToClient(s, new Message("userCount", allClientSocketsOnline.Count.ToString()));
            CastToAllClients(new Message("ReturnAllUserData",JsonSerializer.ToJsonString(allUserData)));
        }
    }
    public static void CastToAllClients(Message msg)
    {
        foreach (Socket socket in allClientSocketsOnline)
        {
            if(socket != null&&socket.Connected==true)
            {
                socket.Send(System.Text.Encoding.Default.GetBytes(JsonSerializer.ToJsonString(msg)+'&'));
            }
            
         //   socket.Send(System.Text.Encoding.Default.GetBytes("&"));
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
            foreach(UserData u in allUserData)
            {
                Console.WriteLine(JsonSerializer.ToJsonString(u));
            }
                    break;
                case '2':
                    foreach(KeyValuePair<Vector2Int,Chunk> c in chunks)
                    {
                        Console.WriteLine(JsonSerializer.ToJsonString(c.Key));
                    }
                        
                    
                    break;
                case '3':
                    Console.WriteLine("\nMessage Count:"+toDoList.Count.ToString());
                    break;
                        
           
        }
        }
      

                
    }


    static void ExecuteToDoList()
    {
        while (true)
        {
       
                Socket s;
                Message message;
               // Thread.Sleep(10);
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
                        Vector2Int v = JsonSerializer.Deserialize<Vector2Int>(message.messageContent);
                        if (!chunks.ContainsKey(v))
                        {
                            Chunk c = new Chunk(v);
                            chunks.TryAdd(v, c);
                            c.InitMap(v);
                            //   world = new Chunk(new Vector2Int(0, 0));
                            //  world.InitMap(new Vector2Int(0, 0));
                            SendToClient(s, new Message("WorldData", JsonSerializer.ToJsonString(c)));
                        }
                        else
                        {
                            SendToClient(s, new Message("WorldData", JsonSerializer.ToJsonString(chunks[v])));
                        }

                        break;
                    //message content type:blockmodifydata
                    case "UpdateChunk":
                        BlockModifyData b = JsonSerializer.Deserialize<BlockModifyData>(message.messageContent);
                        if (GetChunk(Vec3ToChunkPos(new Vector3(b.x, b.y, b.z))) == null)
                        {
                            SendToClient(s, new Message("ChunkNotFound", "ChunkNotFound"));
                        }
                        else
                        {
                            Vector2Int x = Vec3ToChunkPos(new Vector3(b.x, b.y, b.z));
                            Vector3 chunkSpacePos = new Vector3(b.x, b.y, b.z) - new Vector3(x.x, 0, x.y);
                            GetChunk(x).map[(int)chunkSpacePos.X, (int)chunkSpacePos.Y, (int)chunkSpacePos.Z] = b.convertType;
                            CastToAllClients(new Message("WorldData", JsonSerializer.ToJsonString(GetChunk(x))));
                        }




                        break;
                    case "LogOut":
                        UserLogout(s);
                        break;
                    //message content type:userdata
                    case "UpdateUser":
                        UserData u = JsonSerializer.Deserialize<UserData>(message.messageContent);


                        int idx = allUserData.FindIndex(delegate (UserData pl) { return pl.userName == u.userName; });
                        if (idx != -1)
                        {
                            allUserData[idx] = u;

                        }
                        CastToAllClients(new Message("ReturnAllUserData", JsonSerializer.ToJsonString(allUserData)));
                        break;
                    case "UpdateAllUser":
                        CastToAllClients(new Message("ClientUpdateUser", "update"));

                        break;
                    case "Login":
                        UserLogin(s, message.messageContent);
                        //  s.Send(System.Text.Encoding.Default.GetBytes(JsonSerializer.ToJsonString(new Message("LoginReturn", "Hello"))));

                        break;
                    default:
                        s.Send(System.Text.Encoding.Default.GetBytes(JsonSerializer.ToJsonString(new Message("UnknownMessage", "UnknownMessage")) + '&'));
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
     //   Thread executeThread2 = new Thread(() => ExecuteToDoList());
    //    executeThread2.Start();
        //  return;
    }
}