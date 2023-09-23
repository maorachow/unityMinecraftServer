using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Net.Sockets;
using System.Net;
using System.Text.RegularExpressions;
using Newtonsoft.Json;
using System.Threading.Tasks;
using MessagePack;
using Unity.Jobs;
using Unity.Collections;
using Unity.Burst;
using UnityEngine.Rendering;
using Unity.Collections;
public class Chunk : MonoBehaviour
{


   [BurstCompile(CompileSynchronously = true)]
    public struct BakeJob:IJobParallelFor{
        public NativeArray<int> meshes;
        public void Execute(int index){
            Physics.BakeMesh(meshes[index],false);
        }
    }

    public struct Vertex{
        public Vector3 pos;
        public Vector3 normal;
        public Vector2 uvPos;
        public Vertex(Vector3 v3,Vector3 nor,Vector2 v2){
            pos=v3;
            normal=nor;
            uvPos=v2; 
        }
    }
     [BurstCompile(CompileSynchronously = true)]
     public struct MeshBuildJob:IJob{
       // public NativeArray<VertexAttributeDescriptor> vertexAttributes;
        public NativeArray<Vector3> verts;
        public NativeArray<Vector2> uvs;
        public NativeArray<int> tris;
       // public NativeArray<VertexAttributeDescriptor> vertsDes;
        //    public int vertLen;
       //     public int uvsLen;
         //   public int trisLen;
            public Mesh.MeshDataArray dataArray;
         public void Execute(){
             // Allocate mesh data for one mesh.
      //  dataArray = Mesh.AllocateWritableMeshData(1);
        var data = dataArray[0];
        // Tetrahedron vertices with positions and normals.
        // 4 faces with 3 unique vertices in each -- the faces
        // don't share the vertices since normals have to be
        // different for each face.
         /*   */
    /*    data.SetVertexBufferParams(vertLen,new VertexAttributeDescriptor(VertexAttribute.Position, VertexAttributeFormat.Float32, 3),
            new VertexAttributeDescriptor(VertexAttribute.Normal, VertexAttributeFormat.Float32, 3),
            new VertexAttributeDescriptor(VertexAttribute.TexCoord0, VertexAttributeFormat.Float32, 2)
       );*/
        // Four tetrahedron vertex positions:
   //     var sqrt075 = Mathf.Sqrt(0.75f);
   //     var p0 = new Vector3(0, 0, 0);
   //     var p1 = new Vector3(1, 0, 0);
   //     var p2 = new Vector3(0.5f, 0, sqrt075);
   //     var p3 = new Vector3(0.5f, sqrt075, sqrt075 / 3);
        // The first vertex buffer data stream is just positions;
        // fill them in.
        var pos = data.GetVertexData<Vertex>();
      //  pos=verts;
        for(int i=0;i<pos.Length;i++){
            pos[i]=new Vertex(verts[i],new Vector3(1f,1f,1f),uvs[i]);
           
        }
        // Note: normals will be calculated later in RecalculateNormals.
        // Tetrahedron index buffer: 4 triangles, 3 indices per triangle.
        // All vertices are unique so the index buffer is just a
        // 0,1,2,...,11 sequence.
    //    data.SetIndexBufferParams(verts.Length, IndexFormat.UInt16);
       data.SetIndexBufferParams((int)(pos.Length/2)*3, IndexFormat.UInt32);
        var ib = data.GetIndexData<int>();
        for (int i = 0; i < ib.Length; ++i)
            ib[i] = tris[i];
        // One sub-mesh with all the indices.
        data.subMeshCount = 1;
        data.SetSubMesh(0, new SubMeshDescriptor(0, ib.Length));
        // Create the mesh and apply data to it:
     //   Debug.Log("job");
  //   int threadId = System.Threading.Thread.CurrentThread.ManagedThreadId;
   //        UnityEngine.Debug.Log(threadId == 1 ? "Main thread" : $"Worker thread {threadId}");
           pos.Dispose();
           ib.Dispose();
        }

    }
    public bool isChunkBuilding=false;
    public bool isChunkUpdated=false;
    public bool isWaitingForNewChunkData=false;
   public static FastNoise noiseGenerator=new FastNoise();
    //0None 1Stone 2Grass 3Dirt 4Side grass block 5Bedrock 6WoodX 7WoodY 8WoodZ 9Leaves 10Diamond Ore
  //100Water 101Grass
  //200Leaves
  //0-99solid blocks
  //100-199no hitbox blocks
  //200-299hitbox nonsolid blocks
      public static bool isBlockInfoAdded=false;
    public static bool isJsonReadFromDisk=false;
    public static bool isWorldDataSaved=false;
   // public bool isMapGenCompleted=false;
    public bool isMeshBuildCompleted=false;
 //   public bool isChunkMapUpdated=false;
   // public bool isSavedInDisk=false;
    public bool isChunkDataDownloaded=false;
    public bool isModifiedInGame=false;
    public bool isChunkPosInited=false;
    public bool isStrongLoaded=false;
    public Chunk frontChunk;
    public Chunk backChunk;
    public Chunk leftChunk;
    public Chunk rightChunk;
    public Chunk frontLeftChunk;
    public Chunk frontRightChunk;
    public Chunk backLeftChunk;
    public Chunk backRightChunk;
    public static Dictionary<int,AudioClip> blockAudioDic=new Dictionary<int,AudioClip>();
   public delegate bool TmpCheckFace(int x,int y,int z);
    public delegate void TmpBuildFace(int typeid, Vector3 corner, Vector3 up, Vector3 right, bool reversed, NativeList<Vector3> verts, NativeList<Vector2> uvs, NativeList<int> tris, int side);
     public static Dictionary<int,List<Vector2>> itemBlockInfo=new Dictionary<int,List<Vector2>>();
    public static Dictionary<int,List<Vector2>> blockInfo=new Dictionary<int,List<Vector2>>();
    public static Dictionary<Vector2Int,Chunk> chunks=new Dictionary<Vector2Int,Chunk>();
    public static Chunk instance;
        public Mesh chunkMesh;
         public Mesh chunkNonSolidMesh;
   public MeshCollider meshCollider;
    public MeshRenderer meshRenderer;
    public MeshFilter meshFilter;
    public MeshCollider meshColliderNS;
    public MeshRenderer meshRendererNS;
    public MeshFilter meshFilterNS;
    public int[,,] map;
     public static int chunkSeaLevel=63;
    public static int chunkWidth=16;
    public static int chunkHeight=256;
  //  public bool isChunkMessageSent=false;
     public Vector3[] opqVerts;
    public Vector2[] opqUVs;
    public int[] opqTris;
    public Vector3[] NSVerts;
    public Vector2[] NSUVs;
    public int[] NSTris;
    public Vector2Int chunkPos;
    public NativeArray<Vector3> opqVertsNative;
    public NativeArray<Vector2> opqUVsNative;
    public NativeArray<int> opqTrisNative;
    public NativeArray<Vector3> NSVertsNative;
    public NativeArray<Vector2> NSUVsNative;
    public NativeArray<int> NSTrisNative;
        NativeList<Vector3> vertsNS ;
        NativeList<Vector2> uvsNS ;
        NativeList<int> trisNS;
        NativeList<Vector3> verts;
        NativeList<Vector2> uvs;
        NativeList<int> tris ;
    public static void AddBlockInfo(){
        //left right bottom top back front
        blockAudioDic.TryAdd(0,null);
        blockAudioDic.TryAdd(1,Resources.Load<AudioClip>("Audios/Stone_dig2"));
        blockAudioDic.TryAdd(2,Resources.Load<AudioClip>("Audios/Grass_dig1"));
        blockAudioDic.TryAdd(3,Resources.Load<AudioClip>("Audios/Gravel_dig1"));
        blockAudioDic.TryAdd(4,Resources.Load<AudioClip>("Audios/Grass_dig1"));
        blockAudioDic.TryAdd(5,Resources.Load<AudioClip>("Audios/Stone_dig2"));
        blockAudioDic.TryAdd(6,Resources.Load<AudioClip>("Audios/Wood_dig1"));
        blockAudioDic.TryAdd(7,Resources.Load<AudioClip>("Audios/Wood_dig1"));
        blockAudioDic.TryAdd(8,Resources.Load<AudioClip>("Audios/Wood_dig1"));
        blockAudioDic.TryAdd(9,Resources.Load<AudioClip>("Audios/Grass_dig1"));
        blockAudioDic.TryAdd(10,Resources.Load<AudioClip>("Audios/Stone_dig2"));
        blockAudioDic.TryAdd(100,Resources.Load<AudioClip>("Audios/Stone_dig2"));
        blockAudioDic.TryAdd(101,Resources.Load<AudioClip>("Audios/Grass_dig1"));
        blockInfo.TryAdd(1,new List<Vector2>{new Vector2(0f,0f),new Vector2(0f,0f),new Vector2(0f,0f),new Vector2(0f,0f),new Vector2(0f,0f),new Vector2(0f,0f)});
        blockInfo.TryAdd(2,new List<Vector2>{new Vector2(0.0625f,0f),new Vector2(0.0625f,0f),new Vector2(0.0625f,0f),new Vector2(0.0625f,0f),new Vector2(0.0625f,0f),new Vector2(0.0625f,0f)});
        blockInfo.TryAdd(3,new List<Vector2>{new Vector2(0.125f,0f),new Vector2(0.125f,0f),new Vector2(0.125f,0f),new Vector2(0.125f,0f),new Vector2(0.125f,0f),new Vector2(0.125f,0f)});
        blockInfo.TryAdd(4,new List<Vector2>{new Vector2(0.1875f,0f),new Vector2(0.1875f,0f),new Vector2(0.125f,0f),new Vector2(0.0625f,0f),new Vector2(0.1875f,0f),new Vector2(0.1875f,0f)});
        blockInfo.TryAdd(100,new List<Vector2>{new Vector2(0f,0f),new Vector2(0f,0f),new Vector2(0f,0f),new Vector2(0f,0f),new Vector2(0f,0f),new Vector2(0f,0f)});
        blockInfo.TryAdd(101,new List<Vector2>{new Vector2(0f,0.0625f)});
        blockInfo.TryAdd(5,new List<Vector2>{new Vector2(0.375f,0f),new Vector2(0.375f,0f),new Vector2(0.375f,0f),new Vector2(0.375f,0f),new Vector2(0.375f,0f),new Vector2(0.375f,0f)});
        blockInfo.TryAdd(6,new List<Vector2>{new Vector2(0.25f,0f),new Vector2(0.25f,0f),new Vector2(0.5f,0f),new Vector2(0.5f,0f),new Vector2(0.5f,0f),new Vector2(0.5f,0f)});
        blockInfo.TryAdd(7,new List<Vector2>{new Vector2(0.3125f,0f),new Vector2(0.3125f,0f),new Vector2(0.25f,0f),new Vector2(0.25f,0f),new Vector2(0.3125f,0f),new Vector2(0.3125f,0f)});
        blockInfo.TryAdd(8,new List<Vector2>{new Vector2(0.5f,0f),new Vector2(0.5f,0f),new Vector2(0.5f,0f),new Vector2(0.5f,0f),new Vector2(0.25f,0f),new Vector2(0.25f,0f)});
        blockInfo.TryAdd(9,new List<Vector2>{new Vector2(0.4375f,0f),new Vector2(0.4375f,0f),new Vector2(0.4375f,0f),new Vector2(0.4375f,0f),new Vector2(0.4375f,0f),new Vector2(0.4375f,0f)});
        blockInfo.TryAdd(10,new List<Vector2>{new Vector2(0.5625f,0f),new Vector2(0.5625f,0f),new Vector2(0.5625f,0f),new Vector2(0.5625f,0f),new Vector2(0.5625f,0f),new Vector2(0.5625f,0f)});

        itemBlockInfo.TryAdd(1,new List<Vector2>{new Vector2(0f,0f),new Vector2(0f,0f),new Vector2(0f,0f),new Vector2(0f,0f),new Vector2(0f,0f),new Vector2(0f,0f)});
        itemBlockInfo.TryAdd(2,new List<Vector2>{new Vector2(0.0625f,0f),new Vector2(0.0625f,0f),new Vector2(0.0625f,0f),new Vector2(0.0625f,0f),new Vector2(0.0625f,0f),new Vector2(0.0625f,0f)});
        itemBlockInfo.TryAdd(3,new List<Vector2>{new Vector2(0.125f,0f),new Vector2(0.125f,0f),new Vector2(0.125f,0f),new Vector2(0.125f,0f),new Vector2(0.125f,0f),new Vector2(0.125f,0f)});
        itemBlockInfo.TryAdd(4,new List<Vector2>{new Vector2(0.1875f,0f),new Vector2(0.1875f,0f),new Vector2(0.125f,0f),new Vector2(0.0625f,0f),new Vector2(0.1875f,0f),new Vector2(0.1875f,0f)});
        itemBlockInfo.TryAdd(100,new List<Vector2>{new Vector2(0.0625f,0.0625f),new Vector2(0.0625f,0.0625f),new Vector2(0.0625f,0.0625f),new Vector2(0.0625f,0.0625f),new Vector2(0.0625f,0.0625f),new Vector2(0.0625f,0.0625f)});
        itemBlockInfo.TryAdd(101,new List<Vector2>{new Vector2(0f,0.0625f)});
        itemBlockInfo.TryAdd(5,new List<Vector2>{new Vector2(0.375f,0f),new Vector2(0.375f,0f),new Vector2(0.375f,0f),new Vector2(0.375f,0f),new Vector2(0.375f,0f),new Vector2(0.375f,0f)});
        itemBlockInfo.TryAdd(6,new List<Vector2>{new Vector2(0.25f,0f),new Vector2(0.25f,0f),new Vector2(0.5f,0f),new Vector2(0.5f,0f),new Vector2(0.5f,0f),new Vector2(0.5f,0f)});
        itemBlockInfo.TryAdd(7,new List<Vector2>{new Vector2(0.3125f,0f),new Vector2(0.3125f,0f),new Vector2(0.25f,0f),new Vector2(0.25f,0f),new Vector2(0.3125f,0f),new Vector2(0.3125f,0f)});
        itemBlockInfo.TryAdd(8,new List<Vector2>{new Vector2(0.5f,0f),new Vector2(0.5f,0f),new Vector2(0.5f,0f),new Vector2(0.5f,0f),new Vector2(0.25f,0f),new Vector2(0.25f,0f)});
        itemBlockInfo.TryAdd(9,new List<Vector2>{new Vector2(0.4375f,0f),new Vector2(0.4375f,0f),new Vector2(0.4375f,0f),new Vector2(0.4375f,0f),new Vector2(0.4375f,0f),new Vector2(0.4375f,0f)});
        isBlockInfoAdded=true;

    }
     void Awake(){
       // playerPos=GameObject.FindGameObjectWithTag("Player").GetComponent<Transform>();
        meshRenderer = GetComponent<MeshRenderer>();
	    meshCollider = GetComponent<MeshCollider>();
	    meshFilter = GetComponent<MeshFilter>();
        meshRendererNS = transform.GetChild(0).gameObject.GetComponent<MeshRenderer>();
	    meshColliderNS = transform.GetChild(0).gameObject.GetComponent<MeshCollider>();
	    meshFilterNS = transform.GetChild(0).gameObject.GetComponent<MeshFilter>();
        if(!isBlockInfoAdded){
             AddBlockInfo();
        }
       
    }

    void ReInitData(){
        chunkPos=new Vector2Int((int)transform.position.x,(int)transform.position.z);
        isChunkPosInited=true;

       // instance=this;
       if(chunks.ContainsKey(chunkPos)){
        chunks.Remove(chunkPos);
        chunks.Add(chunkPos,this);
       }
        chunks.TryAdd(chunkPos,this);
        Debug.Log(chunks.Count);
       
        //meshFilter=GetComponent<MeshFilter>();
       // meshCollider=GetComponent<MeshCollider>();
       // InvokeRepeating("InvokeGetMap",1f,0.5f);
    }

    void OnDisable(){
      //  CancelInvoke();
        chunks.Remove(chunkPos);

         chunkPos=new Vector2Int(-10240,-10240);
         
         isChunkPosInited=false;
         isChunkDataDownloaded=false;
        isChunkUpdated=false;
    }
       void  ClientInitMap(Vector2Int pos){
      //  Thread.Sleep(1000);
        frontChunk=GetChunk(new Vector2Int(chunkPos.x,chunkPos.y+chunkWidth));
        frontLeftChunk=GetChunk(new Vector2Int(chunkPos.x-chunkWidth,chunkPos.y+chunkWidth));
        frontRightChunk=GetChunk(new Vector2Int(chunkPos.x+chunkWidth,chunkPos.y+chunkWidth));
        backLeftChunk=GetChunk(new Vector2Int(chunkPos.x-chunkWidth,chunkPos.y-chunkWidth));
        backRightChunk=GetChunk(new Vector2Int(chunkPos.x+chunkWidth,chunkPos.y-chunkWidth));
        backChunk=GetChunk(new Vector2Int(chunkPos.x,chunkPos.y-chunkWidth));
           
        leftChunk=GetChunk(new Vector2Int(chunkPos.x-chunkWidth,chunkPos.y));
      
        rightChunk=GetChunk(new Vector2Int(chunkPos.x+chunkWidth,chunkPos.y));
   
     
       
       // await Task.Run(()=>{while(frontChunk==null||backChunk==null||leftChunk==null||rightChunk==null){}});
    

    
   //     FreshGenMap(pos);
        
    vertsNS = new NativeList<Vector3>(Allocator.TempJob);
     uvsNS = new NativeList<Vector2>(Allocator.TempJob);
    trisNS = new NativeList<int>(Allocator.TempJob);
     verts = new NativeList<Vector3>(Allocator.TempJob);
   uvs = new NativeList<Vector2>(Allocator.TempJob);
   tris = new NativeList<int>(Allocator.TempJob);
        

        GenerateMesh(verts,uvs,tris,vertsNS,uvsNS,trisNS);
   
    }

     public void GenerateMesh(NativeList<Vector3> verts, NativeList<Vector2> uvs, NativeList<int> tris, NativeList<Vector3> vertsNS, NativeList<Vector2> uvsNS, NativeList<int> trisNS){
     //   Thread.Sleep(10);
         TmpCheckFace tmp=new TmpCheckFace(CheckNeedBuildFace);
        TmpBuildFace TmpBuildFace=new TmpBuildFace(BuildFace);
        for (int x = 0; x < chunkWidth; x++){
            for (int y = 0; y < chunkHeight; y++){
                for (int z = 0; z < chunkWidth; z++){
                   //     BuildBlock(x, y, z, verts, uvs, tris, vertsNS, uvsNS, trisNS);
        if (this.map[x, y, z] == 0) continue;
        int typeid = this.map[x, y, z];
        if(0<typeid&&typeid<100){
        //Left
        if (tmp(x - 1, y, z))
          TmpBuildFace(typeid, new Vector3(x, y, z), Vector3.up, Vector3.forward, false, verts, uvs, tris,0);
        //Right
        if (tmp(x + 1, y, z))
         TmpBuildFace(typeid, new Vector3(x + 1, y, z), Vector3.up, Vector3.forward, true, verts, uvs, tris,1);

        //Bottom
        if (tmp(x, y - 1, z))
         TmpBuildFace(typeid, new Vector3(x, y, z), Vector3.forward, Vector3.right, false, verts, uvs, tris,2);
        //Top
        if (tmp(x, y + 1, z))
        TmpBuildFace(typeid, new Vector3(x, y + 1, z), Vector3.forward, Vector3.right, true, verts, uvs, tris,3);

        //Back
        if (tmp(x, y, z - 1))
        TmpBuildFace(typeid, new Vector3(x, y, z), Vector3.up, Vector3.right, true, verts, uvs, tris,4);
        //Front
        if (tmp(x, y, z + 1))
        TmpBuildFace(typeid, new Vector3(x, y, z + 1), Vector3.up, Vector3.right, false, verts, uvs, tris,5); 



        }else if(100<=typeid&&typeid<200){

    if(typeid==100){



        //water
        //left
        if (tmp(x-1,y,z)&&GetBlockType(x-1,y,z)!=100){
            if(GetBlockType(x,y+1,z)!=100){
            TmpBuildFace(typeid, new Vector3(x, y, z), new Vector3(0f,0.8f,0f), Vector3.forward, false, vertsNS, uvsNS, trisNS,0); 




            }else{
     TmpBuildFace(typeid, new Vector3(x, y, z), new Vector3(0f,1f,0f), Vector3.forward, false, vertsNS, uvsNS, trisNS,0); 





      
            }
           
        }
            
        //Right
        if (tmp(x+1,y,z)&&GetBlockType(x+1,y,z)!=100){
                if(GetBlockType(x,y+1,z)!=100){
     TmpBuildFace(typeid, new Vector3(x + 1, y, z), new Vector3(0f,0.8f,0f), Vector3.forward, true, vertsNS, uvsNS, trisNS,1);



                }else{
         TmpBuildFace(typeid, new Vector3(x + 1, y, z), new Vector3(0f,1f,0f), Vector3.forward, true, vertsNS, uvsNS, trisNS,1);



            }  

        }

            

        //Bottom
        if (tmp(x,y-1,z)&&GetBlockType(x,y-1,z)!=100){
       TmpBuildFace(typeid, new Vector3(x, y, z), Vector3.forward, Vector3.right, false, vertsNS, uvsNS, trisNS,2);




        }
            
        //Top
        if (tmp(x,y+1,z)&&GetBlockType(x,y+1,z)!=100){
        TmpBuildFace(typeid, new Vector3(x, y + 0.8f, z), Vector3.forward, Vector3.right, true, vertsNS, uvsNS, trisNS,3);




        }
           



        //Back
        if (tmp(x,y,z-1)&&GetBlockType(x,y,z-1)!=100){
            if(GetBlockType(x,y+1,z)!=100){
            TmpBuildFace(typeid, new Vector3(x, y, z), new Vector3(0f,0.8f,0f), Vector3.right, true, vertsNS, uvsNS, trisNS,4);



       
            }else{
            TmpBuildFace(typeid, new Vector3(x, y, z), new Vector3(0f,1f,0f), Vector3.right, true, vertsNS, uvsNS, trisNS,4);






 
            }
            
        }

            
        //Front
        if (tmp(x,y,z+1)&&GetBlockType(x,y,z+1)!=100){
            if(GetBlockType(x,y+1,z)!=100){
            TmpBuildFace(typeid, new Vector3(x, y, z + 1), new Vector3(0f,0.8f,0f), Vector3.right, false, vertsNS, uvsNS, trisNS,5) ;


            }else{
            TmpBuildFace(typeid, new Vector3(x, y, z+1), new Vector3(0f,1f,0f), Vector3.right, false, vertsNS, uvsNS, trisNS,4);

            }
             
        }   
    }
            
        if(typeid>=101&&typeid<150){
            Vector3 randomCrossModelOffset=new Vector3(0f,0f,0f);
            TmpBuildFace(typeid, new Vector3(x, y, z)+randomCrossModelOffset, new Vector3(0f,1f,0f)+randomCrossModelOffset, new Vector3(1f,0f,1f)+randomCrossModelOffset, false, vertsNS, uvsNS, trisNS,0);
            


            TmpBuildFace(typeid, new Vector3(x, y, z)+randomCrossModelOffset, new Vector3(0f,1f,0f)+randomCrossModelOffset, new Vector3(1f,0f,1f)+randomCrossModelOffset, true, vertsNS, uvsNS, trisNS,0);



          TmpBuildFace(typeid, new Vector3(x, y, z+1f)+randomCrossModelOffset, new Vector3(0f,1f,0f)+randomCrossModelOffset, new Vector3(1f,0f,-1f)+randomCrossModelOffset, false, vertsNS, uvsNS, trisNS,0);



         TmpBuildFace(typeid, new Vector3(x, y, z+1f)+randomCrossModelOffset, new Vector3(0f,1f,0f)+randomCrossModelOffset, new Vector3(1f,0f,-1f)+randomCrossModelOffset, true, vertsNS, uvsNS, trisNS,0);


        }

                        
                    }
                }
            }
        }
       // opqVerts=verts.ToArray();
      //  opqUVs=uvs.ToArray();
      //  opqTris=tris.ToArray();
       // NSVerts=vertsNS.ToArray();
       // NSUVs=uvsNS.ToArray();
       // NSTris=trisNS.ToArray();
        opqVertsNative=verts.AsArray();
        opqUVsNative=uvs.AsArray();
        opqTrisNative=tris.AsArray();
        NSVertsNative=vertsNS.AsArray();
        NSUVsNative=uvsNS.AsArray();
        NSTrisNative=trisNS.AsArray();
    /*    verts.Dispose();
        uvs.Dispose();
        tris.Dispose();
        vertsNS.Dispose();
        uvsNS.Dispose();
        trisNS.Dispose();*/
        isMeshBuildCompleted=true;
    }



    bool CheckNeedBuildFace(int x, int y, int z){
        if (y < 0) return false;
        var type = GetBlockType(x, y, z);
        bool isNonSolid=false;
        if(type<200&&type>=100){
            isNonSolid=true;
        }
        switch(isNonSolid){
            case true:return true;
            case false:break;
        }
        switch (type)
        {
            
         
            case 0:
                return true;
            default:
                return false;
        }
    }
    public int GenerateBlockType(int x, int y, int z,Vector2Int pos){
      
        float noiseValue=chunkSeaLevel+noiseGenerator.GetSimplex(pos.x+x,pos.y+z)*20f;
        if(noiseValue>y){
            return 1;
        }else{
            if(y<chunkSeaLevel&&y>noiseValue){
                     return 100;       
            }
                return 0;
        }
       // return 0;
    }
    public int GetBlockType(int x, int y, int z){
        if (y < 0 || y > chunkHeight - 1)
        {
            return 0;
        }
        
        if ((x < 0) || (z < 0) || (x >= chunkWidth) || (z >= chunkWidth))
        {
            if(x>=chunkWidth){
                if(rightChunk!=null&&rightChunk.isChunkDataDownloaded==true){
                return rightChunk.map[0,y,z];    
                }else return GenerateBlockType(x,y,z,chunkPos);
                
            }else if(z>=chunkWidth){
                if(frontChunk!=null&&frontChunk.isChunkDataDownloaded==true){
                return frontChunk.map[x,y,0];
                 }else return GenerateBlockType(x,y,z,chunkPos);
            }else if(x<0){
                if(leftChunk!=null&&leftChunk.isChunkDataDownloaded==true){
                return leftChunk.map[chunkWidth-1,y,z];
                 }else return GenerateBlockType(x,y,z,chunkPos);
            }else if(z<0){
                if(backChunk!=null&&backChunk.isChunkDataDownloaded==true){
                return backChunk.map[x,y,chunkWidth-1];
                 }else return GenerateBlockType(x,y,z,chunkPos);
            }
           
        }
        return map[x, y, z];
    }



     void BuildFace(int typeid, Vector3 corner, Vector3 up, Vector3 right, bool reversed, NativeList<Vector3> verts, NativeList<Vector2> uvs, NativeList<int> tris, int side){
        int index = verts.Length ;
    
        verts.Add (corner);
        verts.Add (corner + up);
        verts.Add (corner + up + right);
        verts.Add (corner + right);

        Vector2 uvWidth = new Vector2(0.0625f, 0.0625f);
        Vector2 uvCorner = new Vector2(0.00f, 0.00f);

        //uvCorner.x = (float)(typeid - 1) / 16;
        uvCorner=blockInfo[typeid][side];
        uvs.Add(uvCorner);
        uvs.Add(new Vector2(uvCorner.x, uvCorner.y + uvWidth.y));
        uvs.Add(new Vector2(uvCorner.x + uvWidth.x, uvCorner.y + uvWidth.y));
        uvs.Add(new Vector2(uvCorner.x + uvWidth.x, uvCorner.y));
    
        if (reversed)
            {
            tris.Add(index + 0);
            tris.Add(index + 1);
            tris.Add(index + 2);
            tris.Add(index + 2);
            tris.Add(index + 3);
            tris.Add(index + 0);
            }
            else
            {
            tris.Add(index + 1);
            tris.Add(index + 0);
            tris.Add(index + 2);
            tris.Add(index + 3);
            tris.Add(index + 2);
            tris.Add(index + 0);
        }
    
    }

    public async void BuildChunk()
    {
   //  System.Diagnostics.Stopwatch stopWatch = new System.Diagnostics.Stopwatch();
 
      //  stopWatch.Start();
     
       
        isChunkBuilding=true;
  //  Debug.Log("BuildChunk");
    if(map==null){
        Debug.Log("Send");
        NetworkProgram.SendMessageToServer(new MessageProtocol(137,MessagePackSerializer.Serialize(chunkPos)));
       // await Task.Delay(1000);
    }
    chunkMesh = new Mesh();
    chunkNonSolidMesh=new Mesh();
    
    await Task.Run(()=>{ClientInitMap(chunkPos);});
    
    NativeArray<VertexAttributeDescriptor> vertsDesNative=new NativeArray<VertexAttributeDescriptor>(new VertexAttributeDescriptor[]{
        new VertexAttributeDescriptor(VertexAttribute.Position, VertexAttributeFormat.Float32, 3),
            new VertexAttributeDescriptor(VertexAttribute.Normal, VertexAttributeFormat.Float32, 3),
            new VertexAttributeDescriptor(VertexAttribute.TexCoord0, VertexAttributeFormat.Float32, 2)
    },Allocator.TempJob);
 /*    NativeArray<Vector3> vertsNative=new NativeArray<Vector3>(opqVerts,Allocator.TempJob);
     NativeArray<Vector2> uvsNative=new NativeArray<Vector2>(opqUVs,Allocator.TempJob);
     NativeArray<int> trisNative=new NativeArray<int>(opqTris,Allocator.TempJob);
     NativeArray<Vector3> vertsNSNative=new NativeArray<Vector3>(NSVerts,Allocator.TempJob);
     NativeArray<Vector2> uvsNSNative=new NativeArray<Vector2>(NSUVs,Allocator.TempJob);
     NativeArray<int> trisNSNative=new NativeArray<int>(NSTris,Allocator.TempJob);*/
     NativeArray<int> meshesIDNative=new NativeArray<int>(new int[]{chunkMesh.GetInstanceID()},Allocator.TempJob);
    Mesh.MeshDataArray mda=Mesh.AllocateWritableMeshData(1);
    Mesh.MeshDataArray mdaNS=Mesh.AllocateWritableMeshData(1);
    await Task.Run(()=>{mda[0].SetVertexBufferParams(opqVertsNative.Length,new VertexAttributeDescriptor(VertexAttribute.Position, VertexAttributeFormat.Float32, 3),
            new VertexAttributeDescriptor(VertexAttribute.Normal, VertexAttributeFormat.Float32, 3),
            new VertexAttributeDescriptor(VertexAttribute.TexCoord0, VertexAttributeFormat.Float32, 2));
            mdaNS[0].SetVertexBufferParams(NSVertsNative.Length,new VertexAttributeDescriptor(VertexAttribute.Position, VertexAttributeFormat.Float32, 3),
            new VertexAttributeDescriptor(VertexAttribute.Normal, VertexAttributeFormat.Float32, 3),
            new VertexAttributeDescriptor(VertexAttribute.TexCoord0, VertexAttributeFormat.Float32, 2));});
    
   
  
    MeshBuildJob mbj=new MeshBuildJob{verts=opqVertsNative,uvs=opqUVsNative,tris=opqTrisNative,dataArray=mda};
    JobHandle jhmbj=mbj.Schedule();
     MeshBuildJob mbjNS=new MeshBuildJob{verts=NSVertsNative,uvs=NSUVsNative,tris=NSTrisNative,dataArray=mdaNS};
    
    JobHandle jhmbjNS=mbjNS.Schedule();
    JobHandle.CompleteAll(ref jhmbj,ref jhmbjNS);
    Mesh.ApplyAndDisposeWritableMeshData(mbj.dataArray,chunkMesh,MeshUpdateFlags.DontValidateIndices);
    Mesh.ApplyAndDisposeWritableMeshData(mbjNS.dataArray,chunkNonSolidMesh,MeshUpdateFlags.DontValidateIndices);
  //  chunkMesh.vertices =opqVerts;
  //  chunkMesh.uv = opqUVs;
 //   chunkMesh.triangles = opqTris;
    chunkMesh.RecalculateBounds();
    chunkMesh.RecalculateNormals();
  //  chunkNonSolidMesh.vertices =NSVerts;
  //  chunkNonSolidMesh.uv = NSUVs;
 //   chunkNonSolidMesh.triangles = NSTris;
    chunkNonSolidMesh.RecalculateBounds();
    chunkNonSolidMesh.RecalculateNormals();
   
    
 BakeJob bj=new BakeJob{meshes=meshesIDNative};
    JobHandle bjHandle = bj.Schedule(meshesIDNative.Length, 1);
   bjHandle.Complete();
   Task.Run(()=>{
        opqVertsNative.Dispose();
        opqUVsNative.Dispose();
        opqTrisNative.Dispose();
        NSVertsNative.Dispose();
        NSUVsNative.Dispose();
        NSTrisNative.Dispose();
        meshesIDNative.Dispose();
             verts.Dispose();
        uvs.Dispose();
        tris.Dispose();
        vertsNS.Dispose();
        uvsNS.Dispose();
        trisNS.Dispose();
    });
    meshFilter.mesh = chunkMesh;
    meshFilterNS.mesh=chunkNonSolidMesh;
    meshCollider.sharedMesh = chunkMesh;
    isChunkBuilding=false;
  //  stopWatch.Stop();
   // Debug.Log("time used: " +stopWatch.ElapsedMilliseconds);
    }

/*void BuildBlock(int x, int y, int z, List<Vector3> verts, List<Vector2> uvs, List<int> tris)
    {
        if (map[x, y, z] == 0) return;

        int typeid = map[x, y, z];

        //Left
        if (CheckNeedBuildFace(x - 1, y, z))
            BuildFace(typeid, new Vector3(x, y, z), Vector3.up, Vector3.forward, false, verts, uvs, tris);
        //Right
        if (CheckNeedBuildFace(x + 1, y, z))
            BuildFace(typeid, new Vector3(x + 1, y, z), Vector3.up, Vector3.forward, true, verts, uvs, tris);

        //Bottom
        if (CheckNeedBuildFace(x, y - 1, z))
            BuildFace(typeid, new Vector3(x, y, z), Vector3.forward, Vector3.right, false, verts, uvs, tris);
        //Top
        if (CheckNeedBuildFace(x, y + 1, z))
            BuildFace(typeid, new Vector3(x, y + 1, z), Vector3.forward, Vector3.right, true, verts, uvs, tris);

        //Back
        if (CheckNeedBuildFace(x, y, z - 1))
            BuildFace(typeid, new Vector3(x, y, z), Vector3.up, Vector3.right, true, verts, uvs, tris);
        //Front
        if (CheckNeedBuildFace(x, y, z + 1))
            BuildFace(typeid, new Vector3(x, y, z + 1), Vector3.up, Vector3.right, false, verts, uvs, tris);
    }*/

   /* bool CheckNeedBuildFace(int x, int y, int z)
    {
        if (y < 0) return false;
        var type = GetBlockType(x, y, z);
        switch (type)
        {
            case 0:
                return true;
            default:
                return false;
        }
    }

    public int GetBlockType(int x, int y, int z)
    {
        if (y < 0 || y > chunkHeight - 1)
        {
            return 0;
        }

        //当前位置是否在Chunk内
        if ((x < 0) || (z < 0) || (x >= chunkWidth) || (z >= chunkWidth))
        {
           
            return 0;
        }
        return map[x, y, z];
    }
    void BuildFace(int typeid, Vector3 corner, Vector3 up, Vector3 right, bool reversed, List<Vector3> verts, List<Vector2> uvs, List<int> tris)
{
    int index = verts.Count;
    
    verts.Add (corner);
    verts.Add (corner + up);
    verts.Add (corner + up + right);
    verts.Add (corner + right);
    
    Vector2 uvWidth = new Vector2(0.0625f,0.0625f);
    Vector2 uvCorner = new Vector2(0.00f, 0.00f);

    uvCorner.x += (float)(typeid - 1) / 16;
    uvs.Add(uvCorner);
    uvs.Add(new Vector2(uvCorner.x, uvCorner.y + uvWidth.y));
    uvs.Add(new Vector2(uvCorner.x + uvWidth.x, uvCorner.y + uvWidth.y));
    uvs.Add(new Vector2(uvCorner.x + uvWidth.x, uvCorner.y));
    
    if (reversed)
    {
        tris.Add(index + 0);
        tris.Add(index + 1);
        tris.Add(index + 2);
        tris.Add(index + 2);
        tris.Add(index + 3);
        tris.Add(index + 0);
    }
    else
    {
        tris.Add(index + 1);
        tris.Add(index + 0);
        tris.Add(index + 2);
        tris.Add(index + 3);
        tris.Add(index + 2);
        tris.Add(index + 0);
    }
}*/
 public static int FloatToInt(float f){
        if(f>=0){
            return (int)f;
        }else{
            return (int)f-1;
        }
    }
 public static Vector3Int Vec3ToBlockPos(Vector3 pos){
        Vector3Int intPos=new Vector3Int(FloatToInt(pos.x),FloatToInt(pos.y),FloatToInt(pos.z));
        return intPos;
    }
    public static IEnumerator SetBlock(float x,float y,float z,int type){
        BlockModifyData b = new BlockModifyData(x, y, z, type);
         Chunk chunkNeededUpdate=GetChunk(Vec3ToChunkPos(new Vector3(x,y,z)));
        chunkNeededUpdate.isWaitingForNewChunkData=true;
        NetworkProgram.SendMessageToServer(new MessageProtocol(132,MessagePackSerializer.Serialize(b)));

       
        
       yield return new WaitUntil(()=>chunkNeededUpdate.isWaitingForNewChunkData==false);
  //      Debug.Log("finished");
          if(chunkNeededUpdate.frontChunk!=null){
           chunkNeededUpdate.frontChunk.isChunkUpdated=true;
            }else{
                
            }
            if(chunkNeededUpdate.backChunk!=null){
            chunkNeededUpdate.backChunk.isChunkUpdated=true;
            }else{
                
            }
            if(chunkNeededUpdate.leftChunk!=null){
            chunkNeededUpdate.leftChunk.isChunkUpdated=true;
            }else{
                
            }
            if(chunkNeededUpdate.rightChunk!=null){
            chunkNeededUpdate.rightChunk.isChunkUpdated=true;
            }else{
                
            }
            yield break;
    }
    void Update(){
    if(isChunkUpdated==true&&isChunkBuilding==false){
        BuildChunk();
        isChunkUpdated=false;
    }
    }
    void FixedUpdate(){
        TryReleaseChunk();
    }
    void TryReleaseChunk(){

        if(AllPlayersManager.curPlayerTrans==null||!isChunkPosInited){
            return;
        }
        Vector3 pos=AllPlayersManager.curPlayerTrans.position;
        if((Mathf.Abs(pos.x-chunkPos.x)>PlayerMove.viewRange+chunkWidth||Mathf.Abs(pos.z-chunkPos.y)>PlayerMove.viewRange+chunkWidth)&&isChunkPosInited==true){
            ObjectPools.chunkPool.Remove(this.gameObject);
        }
    }
    public static Vector2Int Vec3ToChunkPos(Vector3 pos){
        Vector3 tmp=pos;
        tmp.x = Mathf.Floor(tmp.x / (float)chunkWidth) * chunkWidth;
        tmp.z = Mathf.Floor(tmp.z / (float)chunkWidth) * chunkWidth;
        Vector2Int value=new Vector2Int((int)tmp.x,(int)tmp.z);
        return value;
    }

     public static Chunk GetChunk(Vector2Int chunkPos){
        if(chunks.ContainsKey(chunkPos)){
            Chunk tmp=chunks[chunkPos];
            return tmp;
        }else{
            return null;
        }
        
    }
}
