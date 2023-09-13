using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using MessagePack;
using System.Runtime.Serialization;
using System.Runtime.CompilerServices;

namespace MyMinecraftServer
{
    public struct Vector2Int:IEquatable<Vector2Int>
    {
        public int x;
        public int y;
        [SerializationConstructor]
        public Vector2Int(int a, int b)
        {
            x = a;
            y = b;
        }
        public float magnitude { get { return MathF.Sqrt((float)(x * x + y * y)); } }

        // Returns the squared length of this vector (RO).
        public int sqrMagnitude {  get { return x * x + y * y; } }
        public override bool Equals(object other)
        {
            if (!(other is Vector2Int)) return false;

            return Equals((Vector2Int)other);
        }
        public bool Equals(Vector2Int other)
        {
            return x == other.x && y == other.y;
        }

        public override int GetHashCode()
        {
            return x.GetHashCode() ^ (y.GetHashCode() << 2);
        }

      
        public static Vector2Int operator -(Vector2Int v)
        {
            return new Vector2Int(-v.x, -v.y);
        }

       
        public static Vector2Int operator +(Vector2Int a, Vector2Int b)
        {
            return new Vector2Int(a.x + b.x, a.y + b.y);
        }

        public static Vector2Int operator -(Vector2Int a, Vector2Int b)
        {
            return new Vector2Int(a.x - b.x, a.y - b.y);
        }

     
        public static Vector2Int operator *(Vector2Int a, Vector2Int b)
        {
            return new Vector2Int(a.x * b.x, a.y * b.y);
        }

        
        public static Vector2Int operator *(int a, Vector2Int b)
        {
            return new Vector2Int(a * b.x, a * b.y);
        }

        
        public static Vector2Int operator *(Vector2Int a, int b)
        {
            return new Vector2Int(a.x * b, a.y * b);
        }

      
        public static Vector2Int operator /(Vector2Int a, int b)
        {
            return new Vector2Int(a.x / b, a.y / b);
        }

     
        public static bool operator ==(Vector2Int lhs, Vector2Int rhs)
        {
            return lhs.x == rhs.x && lhs.y == rhs.y;
        }

        public static bool operator !=(Vector2Int lhs, Vector2Int rhs)
        {
            return !(lhs == rhs);
        }

    }
    public class Vector3Int
    {
        public int x;
        public int y;
        public int z;
        public Vector3Int(int a, int b,int c)
        {
            a = x;
            b = y;
            c= z;
        }
        public static Vector3Int operator +(Vector3Int b, Vector3Int c)
        {
            Vector3Int v = new Vector3Int(b.x + c.x, b.y + c.y, b.z + c.z);
            return v;
        }
        public static Vector3Int operator -(Vector3Int b, Vector3Int c)
        {
            Vector3Int v = new Vector3Int(b.x - c.x, b.y - c.y, b.z- c.z);
            return v;
        }
    }

    public class Chunk
    {
        [IgnoreDataMember]
        public static int worldGenMode = 1;//1 superflat 0 inf
        [IgnoreDataMember]
        public static int chunkWidth=16;
        [IgnoreDataMember]
        public static int chunkHeight=64;
        public int[,,] map;
        public Vector2Int chunkPos=new Vector2Int(0,0);

        public Chunk(Vector2Int chunkPos)
        {
            this.chunkPos = chunkPos;
        }

     
        public void InitMap(Vector2Int chunkPos)
        {
            map = new int[chunkWidth + 2, chunkHeight + 2, chunkWidth + 2];
            if(worldGenMode == 1)
            {
            for(int i = 0; i < chunkWidth; i++)
            {
                for(int j = 0; j < chunkWidth; j++)
                {
                    for(int k=0;k< chunkHeight/4; k++)
                    {
                            map[i, k, j] = 1;
                    }
                }
            }
            }
           
        }



    }
}
