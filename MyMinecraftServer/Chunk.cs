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
    [MessagePackObject]
    public struct Vector2Int:IEquatable<Vector2Int>
    {
        [Key(0)]
        public int x;
        [Key(1)]
        public int y;
        [SerializationConstructor]
        public Vector2Int(int a, int b)
        {
            x = a;
            y = b;
        }
        [IgnoreMember]
        public float magnitude { get { return MathF.Sqrt((float)(x * x + y * y)); } }

        // Returns the squared length of this vector (RO).
        [IgnoreMember]
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
    [MessagePackObject]
    public class Vector3Int: IEquatable<Vector3Int>
    {
        [Key(0)]
        public int x;
        [Key(1)]
        public int y;
        [Key(2)]
        public int z;

        public Vector3Int(int x, int y, int z)
        {
            this.x = x;
            this.y = y;
            this.z = z;
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
        public static bool operator ==(Vector3Int lhs, Vector3Int rhs)
        {
            return lhs.x == rhs.x && lhs.y == rhs.y && lhs.z == rhs.z;
        }

     
        public static bool operator !=(Vector3Int lhs, Vector3Int rhs)
        {
            return !(lhs == rhs);
        }


        public override bool Equals(object other)
        {
            if (!(other is Vector3Int)) return false;

            return Equals((Vector3Int)other);
        }


        public override int GetHashCode()
        {
            var yHash = y.GetHashCode();
            var zHash = z.GetHashCode();
            return x.GetHashCode() ^ (yHash << 4) ^ (yHash >> 28) ^ (zHash >> 4) ^ (zHash << 28);
        }
        public bool Equals(Vector3Int other)
        {
            return this == other;
        }

    }
    public struct RandomGenerator3D
    {
        //  public System.Random rand=new System.Random(0);
        public static int GenerateIntFromVec3(Vector3Int pos)
        {
            System.Random rand = new System.Random(pos.x * pos.y * pos.z * 100);
            return rand.Next(100);
        }
    }
    [MessagePackObject]
    public class Chunk
    {
        [IgnoreMember]
        public static FastNoise noiseGenerator = new FastNoise();
        [IgnoreMember]
        public static int worldGenType = 0;//1 superflat 0 inf
        [IgnoreMember]
        public static int chunkWidth=16;
        [IgnoreMember]
        public static int chunkHeight=256;
        [Key (0)]
        public int[,,] map;
        [IgnoreMember]
        public int[,,] additiveMap = new int[chunkWidth + 2, chunkHeight + 2, chunkWidth + 2];
        [Key(1)]
        public Vector2Int chunkPos=new Vector2Int(0,0);

        public Chunk(Vector2Int chunkPos)
        {
            this.chunkPos = chunkPos;
        }
        [IgnoreMember]
        public static System.Random worldRandomGenerator = new System.Random(0);
        [IgnoreMember]
        public Chunk frontChunk;
        [IgnoreMember]
        public Chunk backChunk;
        [IgnoreMember]
        public Chunk leftChunk;
        [IgnoreMember]
        public Chunk rightChunk;
        [IgnoreMember]
        public Chunk frontLeftChunk;
        [IgnoreMember]
        public Chunk frontRightChunk;
        [IgnoreMember]
        public Chunk backLeftChunk;
        [IgnoreMember]
        public Chunk backRightChunk;
        [IgnoreMember]
        public static int chunkSeaLevel = 63;
        public ChunkData ChunkToChunkData()
        {
            return new ChunkData(this.map, this.chunkPos);
        }
        public async Task InitMap(Vector2Int chunkPos)
        {
            
            map = additiveMap;

            frontChunk = Program.GetChunk(new Vector2Int(chunkPos.x, chunkPos.y + chunkWidth));
            frontLeftChunk = Program.GetChunk(new Vector2Int(chunkPos.x - chunkWidth, chunkPos.y + chunkWidth));
            frontRightChunk = Program.GetChunk(new Vector2Int(chunkPos.x + chunkWidth, chunkPos.y + chunkWidth));
            backLeftChunk = Program.GetChunk(new Vector2Int(chunkPos.x - chunkWidth, chunkPos.y - chunkWidth));
            backRightChunk = Program.GetChunk(new Vector2Int(chunkPos.x + chunkWidth, chunkPos.y - chunkWidth));
            backChunk = Program.GetChunk(new Vector2Int(chunkPos.x, chunkPos.y - chunkWidth));

            leftChunk = Program.GetChunk(new Vector2Int(chunkPos.x - chunkWidth, chunkPos.y));

            rightChunk = Program.GetChunk(new Vector2Int(chunkPos.x + chunkWidth, chunkPos.y));
            if (worldGenType == 1)
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
            else
            {
                FreshGenMap(chunkPos);
            }
              void FreshGenMap(Vector2Int pos)
               {
                   map = additiveMap;
                   if (worldGenType == 0)
                   {
                                    bool isFrontLeftChunkUpdated=false;
                                   bool isFrontRightChunkUpdated=false;
                                   bool isBackLeftChunkUpdated=false;
                                   bool isBackRightChunkUpdated=false;
                                   bool isLeftChunkUpdated=false;
                                   bool isRightChunkUpdated=false;
                                   bool isFrontChunkUpdated=false;
                                   bool isBackChunkUpdated=false;
                       //    System.Random random=new System.Random(pos.x+pos.y);
                       int treeCount = 10;

                       for (int i = 0; i < chunkWidth; i++)
                       {
                           for (int j = 0; j < chunkWidth; j++)
                           {
                               //  float noiseValue=200f*Mathf.PerlinNoise(pos.x*0.01f+i*0.01f,pos.y*0.01f+j*0.01f);
                               float noiseValue = chunkSeaLevel + noiseGenerator.GetSimplex(pos.x + i, pos.y + j) * 20f;
                               for (int k = 0; k < chunkHeight; k++)
                               {
                                   if (noiseValue > k + 3)
                                   {
                                       map[i, k, j] = 1;
                                   }
                                   else if (noiseValue > k)
                                   {

                                       map[i, k, j] = 3;
                                   }
                                   else
                                   {
                                       if (map[i, k, j] == 0)
                                       {
                                           map[i, k, j] = 0;
                                       }

                                   }

                               }
                           }
                       }


                       for (int i = 0; i < chunkWidth; i++)
                       {
                           for (int j = 0; j < chunkWidth; j++)
                           {

                               for (int k = chunkHeight - 1; k >= 0; k--)
                               {

                                   if (map[i, k, j] != 0 && k >= chunkSeaLevel)
                                   {
                                       map[i, k, j] = 4;
                                       break;
                                   }

                                   if (k > chunkSeaLevel && map[i, k, j] == 0 && map[i, k - 1, j] != 0 && map[i, k - 1, j] != 100 && worldRandomGenerator.Next(100) > 80)
                                   {
                                       map[i, k, j] = 101;
                                   }
                                   if (k < chunkSeaLevel && map[i, k, j] == 0)
                                   {
                                       map[i, k, j] = 100;
                                   }

                               }
                           }
                       }

                       for (int i = 0; i < chunkWidth; i++)
                       {
                           for (int j = 0; j < chunkWidth; j++)
                           {

                               for (int k = chunkHeight - 1; k >= 0; k--)
                               {

                                   if (k > chunkSeaLevel && map[i, k, j] == 0 && map[i, k - 1, j] == 4 && map[i, k - 1, j] != 100)
                                   {
                                       if (treeCount > 0)
                                       {
                               //         Console.WriteLine(RandomGenerator3D.GenerateIntFromVec3(new Vector3Int(i, k, j)));
                                           if (RandomGenerator3D.GenerateIntFromVec3(new Vector3Int(i, k, j)) > 98)
                                           {


                                               for (int x = -2; x < 3; x++)
                                               {
                                                   for (int y = 3; y < 5; y++)
                                                   {
                                                       for (int z = -2; z < 3; z++)
                                                       {
                                                           if (x + i < 0 || x + i >= chunkWidth || z + j < 0 || z + j >= chunkWidth)
                                                           {



                                                               if (x + i < 0)
                                                               {
                                                                   if (z + j >= 0 && z + j < chunkWidth)
                                                                   {
                                                                       if (leftChunk != null)
                                                                       {

                                                                           leftChunk.additiveMap[chunkWidth + (x + i), y + k, z + j] = 9;

                                                                           isLeftChunkUpdated = true;

                                                                           //    WorldManager.chunkLoadingQueue.UpdatePriority(leftChunk,0);
                                                                           //         leftChunk.isChunkMapUpdated=true;
                                                                       }
                                                                   }
                                                                   else if (z + j < 0)
                                                                   {
                                                                       if (backLeftChunk != null)
                                                                       {
                                                                           backLeftChunk.additiveMap[chunkWidth + (x + i), y + k, chunkWidth - 1 + (z + j)] = 9;

                                                                           isBackLeftChunkUpdated = true;

                                                                           //    WorldManager.chunkLoadingQueue.UpdatePriority(backLeftChunk,0);
                                                                           //               backLeftChunk.isChunkMapUpdated=true;
                                                                       }

                                                                   }
                                                                   else if (z + j >= chunkWidth)
                                                                   {
                                                                       if (frontLeftChunk != null)
                                                                       {
                                                                           frontLeftChunk.additiveMap[chunkWidth + (x + i), y + k, (z + j) - chunkWidth] = 9;

                                                                           isFrontLeftChunkUpdated = true;

                                                                           //     WorldManager.chunkLoadingQueue.UpdatePriority(frontLeftChunk,0);
                                                                           //                 frontLeftChunk.isChunkMapUpdated=true;
                                                                       }
                                                                   }

                                                               }
                                                               else
                                                               if (x + i >= chunkWidth)
                                                               {
                                                                   if (z + j >= 0 && z + j < chunkWidth)
                                                                   {
                                                                       if (rightChunk != null)
                                                                       {

                                                                           rightChunk.additiveMap[(x + i) - chunkWidth, y + k, z + j] = 9;

                                                                           isRightChunkUpdated = true;

                                                                           //   WorldManager.chunkLoadingQueue.UpdatePriority(rightChunk,0);
                                                                           //      rightChunk.isChunkMapUpdated=true;
                                                                       }
                                                                   }
                                                                   else if (z + j < 0)
                                                                   {
                                                                       if (backRightChunk != null)
                                                                       {
                                                                           backRightChunk.additiveMap[(x + i) - chunkWidth, y + k, chunkWidth + (z + j)] = 9;

                                                                           isBackRightChunkUpdated = true;

                                                                           //    WorldManager.chunkLoadingQueue.UpdatePriority(backRightChunk,0);
                                                                           //         backRightChunk.isChunkMapUpdated=true;
                                                                       }

                                                                   }
                                                                   else if (z + j >= chunkWidth)
                                                                   {
                                                                       if (frontRightChunk != null)
                                                                       {
                                                                           frontRightChunk.additiveMap[(x + i) - chunkWidth, y + k, (z + j) - chunkWidth] = 9;

                                                                           isFrontRightChunkUpdated = true;

                                                                           //     WorldManager.chunkLoadingQueue.UpdatePriority(frontRightChunk,0);
                                                                           //          frontRightChunk.isChunkMapUpdated=true;
                                                                       }
                                                                   }
                                                               }
                                                               else
                                                               if (z + j < 0)
                                                               {

                                                                   if (x + i >= 0 && x + i < chunkWidth)
                                                                   {
                                                                       if (backChunk != null)
                                                                       {

                                                                           backChunk.additiveMap[x + i, y + k, chunkWidth + (z + j)] = 9;

                                                                           isBackChunkUpdated = true;

                                                                           //    WorldManager.chunkLoadingQueue.UpdatePriority(backChunk,0);
                                                                           //         backChunk.isChunkMapUpdated=true;
                                                                       }
                                                                   }
                                                                   else if (x + i < 0)
                                                                   {
                                                                       if (backLeftChunk != null)
                                                                       {
                                                                           backLeftChunk.additiveMap[chunkWidth + (x + i), y + k, chunkWidth - 1 + (z + j)] = 9;

                                                                           isBackLeftChunkUpdated = true;

                                                                           //    WorldManager.chunkLoadingQueue.UpdatePriority(backLeftChunk,0);
                                                                           //            backLeftChunk.isChunkMapUpdated=true;
                                                                       }

                                                                   }
                                                                   else if (x + i >= chunkWidth)
                                                                   {
                                                                       if (backRightChunk != null)
                                                                       {
                                                                           backRightChunk.additiveMap[(x + i) - chunkWidth, y + k, chunkWidth - 1 + (z + j)] = 9;

                                                                           isBackRightChunkUpdated = true;

                                                                           //       WorldManager.chunkLoadingQueue.UpdatePriority(backRightChunk,0);
                                                                           //      backRightChunk.isChunkMapUpdated=true;    
                                                                       }
                                                                   }

                                                               }
                                                               else
                                                               if (z + j >= chunkWidth)
                                                               {

                                                                   if (x + i >= 0 && x + i < chunkWidth)
                                                                   {
                                                                       if (frontChunk != null)
                                                                       {

                                                                           frontChunk.additiveMap[x + i, y + k, (z + j) - chunkWidth] = 9;

                                                                           isFrontChunkUpdated = true;

                                                                           //    WorldManager.chunkLoadingQueue.UpdatePriority(frontChunk,0);
                                                                           //   frontChunk.isChunkMapUpdated=true;
                                                                       }
                                                                   }
                                                                   else if (x + i < 0)
                                                                   {
                                                                       if (frontLeftChunk != null)
                                                                       {
                                                                           frontLeftChunk.additiveMap[chunkWidth + (x + i), y + k, (z + j) - chunkWidth] = 9;

                                                                           isBackLeftChunkUpdated = true;

                                                                           //        WorldManager.chunkLoadingQueue.UpdatePriority(frontLeftChunk,0);
                                                                           //    frontLeftChunk.isChunkMapUpdated=true;
                                                                       }

                                                                   }
                                                                   else if (x + i >= chunkWidth)
                                                                   {
                                                                       if (frontRightChunk != null)
                                                                       {
                                                                           frontRightChunk.additiveMap[(x + i) - chunkWidth, y + k, (z + j) - chunkWidth] = 9;

                                                                           isFrontRightChunkUpdated = true;

                                                                           //  WorldManager.chunkLoadingQueue.UpdatePriority(frontRightChunk,0);
                                                                           //      frontRightChunk.isChunkMapUpdated=true;
                                                                       }
                                                                   }
                                                               }


                                                           }
                                                           else
                                                           {
                                                               map[x + i, y + k, z + j] = 9;
                                                           }
                                                       }
                                                   }
                                               }
                                               map[i, k, j] = 7;
                                               map[i, k + 1, j] = 7;
                                               map[i, k + 2, j] = 7;
                                               map[i, k + 3, j] = 7;
                                               map[i, k + 4, j] = 7;
                                               map[i, k + 5, j] = 9;
                                               map[i, k + 6, j] = 9;

                                               if (i + 1 < chunkWidth)
                                               {
                                                   map[i + 1, k + 5, j] = 9;
                                                   map[i + 1, k + 6, j] = 9;

                                               }
                                               else
                                               {
                                                   if (rightChunk != null)
                                                   {
                                                       rightChunk.additiveMap[0, k + 5, j] = 9;
                                                       rightChunk.additiveMap[0, k + 6, j] = 9;

                                                       //      rightChunk.isChunkMapUpdated=true;
                                                   }
                                               }

                                               if (i - 1 >= 0)
                                               {
                                                   map[i - 1, k + 5, j] = 9;
                                                   map[i - 1, k + 6, j] = 9;

                                               }
                                               else
                                               {
                                                   if (leftChunk != null)
                                                   {
                                                       leftChunk.additiveMap[chunkWidth - 1, k + 5, j] = 9;
                                                       leftChunk.additiveMap[chunkWidth - 1, k + 6, j] = 9;

                                                       // leftChunk.isChunkMapUpdated=true;
                                                   }
                                               }
                                               if (j + 1 < chunkWidth)
                                               {
                                                   map[i, k + 5, j + 1] = 9;
                                                   map[i, k + 6, j + 1] = 9;

                                               }
                                               else
                                               {
                                                   if (frontChunk != null)
                                                   {
                                                       frontChunk.additiveMap[i, k + 5, 0] = 9;
                                                       frontChunk.additiveMap[i, k + 6, 0] = 9;

                                                       //   frontChunk.isChunkMapUpdated=true;
                                                   }
                                               }

                                               if (j - 1 >= 0)
                                               {
                                                   map[i, k + 5, j - 1] = 9;
                                                   map[i, k + 6, j - 1] = 9;

                                               }
                                               else
                                               {
                                                   if (backChunk != null)
                                                   {
                                                       backChunk.additiveMap[i, k + 5, chunkWidth - 1] = 9;
                                                       backChunk.additiveMap[i, k + 6, chunkWidth - 1] = 9;

                                                       //  backChunk.isChunkMapUpdated=true;
                                                   }
                                               }

                                               treeCount--;
                                           }
                                       }
                                   }

                               }
                           }
                       }
                       for (int i = 0; i < chunkWidth; i++)
                       {
                           for (int j = 0; j < chunkWidth; j++)
                           {
                               for (int k = 0; k < chunkHeight / 4; k++)
                               {

                                   if (0 < k && k < 12)
                                   {
                                       if (RandomGenerator3D.GenerateIntFromVec3(new Vector3Int(pos.x, 0, pos.y) + new Vector3Int(i, k, j)) > 96)
                                       {

                                           map[i, k, j] = 10;

                                       }

                                   }

                               }

                           }
                       }
                       for (int i = 0; i < chunkWidth; i++)
                       {
                           for (int j = 0; j < chunkWidth; j++)
                           {
                               map[i, 0, j] = 5;
                           }
                       }
                      
                   }
                   else if (worldGenType == 1)
                   {
                       for (int i = 0; i < chunkWidth; i++)
                       {
                           for (int j = 0; j < chunkWidth; j++)
                           {
                               //  float noiseValue=200f*Mathf.PerlinNoise(pos.x*0.01f+i*0.01f,pos.z*0.01f+j*0.01f);
                               for (int k = 0; k < chunkHeight / 4; k++)
                               {

                                   map[i, k, j] = 1;

                               }
                           }
                       }
                   }

          
               }
        }



    }
}
