using System.Net;
using System.Net.Security;
using System.Net.Sockets;
using Newtonsoft.Json;
using System.Threading;
using System.Text;
using System.Text.Json.Serialization;
using System.Net.Http.Json;
using System.Diagnostics;
using TestClient;
using System.Numerics;


class BlockModifyData
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
public class Vector2Int
{
    public int x;
    public int y;
    public Vector2Int(int a, int b)
    {
        a = x;
        b = y;
    }
}
public class Vector3Int
{
    public int x;
    public int y;
    public int z;
    public Vector3Int(int a, int b, int c)
    {
        a = x;
        b = y;
        c = z;
    }
    public static Vector3Int operator +(Vector3Int b, Vector3Int c)
    {
        Vector3Int v = new Vector3Int(b.x + c.x, b.y + c.y, b.z + c.z);
        return v;
    }
    public static Vector3Int operator -(Vector3Int b, Vector3Int c)
    {
        Vector3Int v = new Vector3Int(b.x - c.x, b.y - c.y, b.z - c.z);
        return v;
    }
}
class UserData  
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

class Message
{
    public string messageType;
    public string messageContent;
    public Message(string a, string b)
    {
        messageType = a;
        messageContent = b;

    }
}
class Program
{
    static UserData currentPlayer;
    static Chunk world;
    static string clientUserName = "Default User";
    static IPAddress ip = IPAddress.Parse("127.0.0.1");
    static int port = 11111;
    static Socket clientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.IP);
    public static float viewRange;

    public static void RecieveServer()
    {
        while (true)
        {
            try
            {
            byte[] data = new byte[65536];
           // clientSocket.Receive(data);
                int count = clientSocket.Receive(data);
                string str = System.Text.Encoding.UTF8.GetString(data, 0, count);
                foreach(string s in str.Split('&'))
                {
                if (s.Length > 0) {
                switch (JsonConvert.DeserializeObject<Message>(System.Text.Encoding.UTF8.GetString(data)).messageType)
                {
                    case "ClientUpdateUser":
                            SendMessageToServer(new Message("UpdateUser", JsonConvert.SerializeObject(currentPlayer)));
                        break;
                    case "LoginReturn":
                        Console.WriteLine("Server:" + JsonConvert.DeserializeObject<Message>(System.Text.Encoding.UTF8.GetString(data)).messageContent);
                        break;
                    case "userCount":
                        Console.WriteLine("Server:" + JsonConvert.DeserializeObject<Message>(System.Text.Encoding.UTF8.GetString(data)).messageContent);
                        break;
                    case "UnknownMessage":
                        Console.WriteLine("Server:"+ JsonConvert.DeserializeObject<Message>(System.Text.Encoding.UTF8.GetString(data)).messageContent);
                        break;
                    case "WorldData":
                            world = JsonConvert.DeserializeObject<Chunk>(JsonConvert.DeserializeObject<Message>(System.Text.Encoding.UTF8.GetString(data)).messageContent);
                        break;
                    default:
                        Console.WriteLine("Client: Unknown Message Type:"+ JsonConvert.DeserializeObject<Message>(System.Text.Encoding.UTF8.GetString(data)).messageType);
                        break;
                }
            }
                }
              
            }
            catch (Exception e) 
            {
                Console.WriteLine("Connection Lost:"+e);
                break;
            }
        
        }
      
    }
    public static void SendMessageToServer(Message m)
    {
        try
        {
            clientSocket.Send(System.Text.Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(m)));
        }
        catch
        {
            Console.WriteLine("Sending message failed");
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
    
    static void Main(string[] args)
    {
        Random rand = new Random();
        if (Console.ReadKey() != null)
        {
            clientSocket.Connect(ip, port);
            currentPlayer = new UserData(rand.NextSingle() * 10f, 100f, rand.NextSingle() * 10f, rand.NextSingle() * 10f, clientUserName);
            SendMessageToServer(new Message("Login", JsonConvert.SerializeObject(currentPlayer)));
            SendMessageToServer(new Message("ChunkGen", "null"));
            Thread thread = new Thread(new ThreadStart(RecieveServer));
            thread.Start();
        }
        while (true)
        {
            if (clientSocket.Connected == false)
            {
                Console.WriteLine("Disconnected");
                break;
            }
            
            //     Console.WriteLine("Enter Message");
            Console.WriteLine("Enter Message Type: 1 update user 2 set block 3 update world");
            char a=Console.ReadKey().KeyChar;
            switch (a)
            {
                case '1':Console.WriteLine("Input player position");
                    currentPlayer.posX = float.Parse(Console.ReadLine());
                    currentPlayer.posY = float.Parse(Console.ReadLine());
                    currentPlayer.posZ = float.Parse(Console.ReadLine());
                    currentPlayer.rotY = float.Parse(Console.ReadLine());
                    SendMessageToServer(new Message("UpdateUser", JsonConvert.SerializeObject(currentPlayer)));
                    break;
                case '2':
                    Console.WriteLine("Input block position");
                    BlockModifyData b = new BlockModifyData(float.Parse(Console.ReadLine()), float.Parse(Console.ReadLine()), float.Parse(Console.ReadLine()), Int32.Parse(Console.ReadLine()));
                   
                  
                    SendMessageToServer(new Message("UpdateChunk", JsonConvert.SerializeObject(b)));
                    break;
                case '3':
                    SendMessageToServer(new Message("ChunkGen","null"));
                    break;
            }
        //    clientSocket.Send(System.Text.Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(new Message(Console.ReadLine(), Console.ReadLine()))));
            Console.WriteLine("Message sent");
        }
    }
}