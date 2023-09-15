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
public class Chunk : MonoBehaviour
{
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
    public delegate void TmpBuildFace(int typeid, Vector3 corner, Vector3 up, Vector3 right, bool reversed, List<Vector3> verts, List<Vector2> uvs, List<int> tris, int side);
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
    public bool isChunkMessageSent=false;
     public Vector3[] opqVerts;
    public Vector2[] opqUVs;
    public int[] opqTris;
    public Vector3[] NSVerts;
    public Vector2[] NSUVs;
    public int[] NSTris;
    public Vector2Int chunkPos;
    void InvokeGetMap(){
        if(map==null&&chunks.ContainsKey(chunkPos)&&isChunkMessageSent==false){
            Debug.Log("null");
            NetworkProgram.SendMessageToServer(new Message("ChunkGen",MessagePackSerializer.Serialize(chunkPos)));
         //   BuildChunk();
            isChunkMessageSent=true;
        }
    }

    public static void AddBlockInfo(){
        //left right bottom top back front
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

    void OnEnable(){
        chunkPos=new Vector2Int((int)transform.position.x,(int)transform.position.z);

       // instance=this;
       
         chunks.TryAdd(chunkPos,this);
       Debug.Log(chunks.Count);
       
        meshFilter=GetComponent<MeshFilter>();
        meshCollider=GetComponent<MeshCollider>();
        InvokeRepeating("InvokeGetMap",1f,0.5f);
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
        List<Vector3> vertsNS = new List<Vector3>();
        List<Vector2> uvsNS = new List<Vector2>();
        List<int> trisNS = new List<int>();
        List<Vector3> verts = new List<Vector3>();
        List<Vector2> uvs = new List<Vector2>();
        List<int> tris = new List<int>();

    
   //     FreshGenMap(pos);
        
      
        

        GenerateMesh(verts,uvs,tris,vertsNS,uvsNS,trisNS);
   
    }

     public void GenerateMesh(List<Vector3> verts, List<Vector2> uvs, List<int> tris, List<Vector3> vertsNS, List<Vector2> uvsNS, List<int> trisNS){
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
        opqVerts=verts.ToArray();
        opqUVs=uvs.ToArray();
        opqTris=tris.ToArray();
        NSVerts=vertsNS.ToArray();
        NSUVs=uvsNS.ToArray();
        NSTris=trisNS.ToArray();
        
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



     void BuildFace(int typeid, Vector3 corner, Vector3 up, Vector3 right, bool reversed, List<Vector3> verts, List<Vector2> uvs, List<int> tris, int side){
        int index = verts.Count;
    
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
  //  Debug.Log("BuildChunk");
    chunkMesh = new Mesh();
    chunkNonSolidMesh=new Mesh();
    
    await Task.Run(()=>{ClientInitMap(chunkPos);});
  
                
    chunkMesh.vertices =opqVerts;
    chunkMesh.uv = opqUVs;
    chunkMesh.triangles = opqTris;
    chunkMesh.RecalculateBounds();
    chunkMesh.RecalculateNormals();
    chunkNonSolidMesh.vertices =NSVerts;
    chunkNonSolidMesh.uv = NSUVs;
    chunkNonSolidMesh.triangles = NSTris;
    chunkNonSolidMesh.RecalculateBounds();
    chunkNonSolidMesh.RecalculateNormals();
    meshFilter.mesh = chunkMesh;
    meshFilterNS.mesh=chunkNonSolidMesh;
    meshCollider.sharedMesh = chunkMesh;
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
    public static IEnumerator SetBlock(float x,float y,float z,int type){
        BlockModifyData b = new BlockModifyData(x, y, z, type);
         Chunk chunkNeededUpdate=GetChunk(Vec3ToChunkPos(new Vector3(x,y,z)));
        chunkNeededUpdate.isWaitingForNewChunkData=true;
        NetworkProgram.SendMessageToServer(new Message("UpdateChunk",MessagePackSerializer.Serialize(b)));

       
        
       yield return new WaitUntil(()=>chunkNeededUpdate.isWaitingForNewChunkData==false);
  //      Debug.Log("finished");
          if(chunkNeededUpdate.frontChunk!=null){
           chunkNeededUpdate.frontChunk.isChunkUpdated=true;
            }
            if(chunkNeededUpdate.backChunk!=null){
            chunkNeededUpdate.backChunk.isChunkUpdated=true;
            }
            if(chunkNeededUpdate.leftChunk!=null){
            chunkNeededUpdate.leftChunk.isChunkUpdated=true;
            }
            if(chunkNeededUpdate.rightChunk!=null){
            chunkNeededUpdate.rightChunk.isChunkUpdated=true;
            }
            yield break;
    }
    void Update(){
    if(isChunkUpdated==true){
        BuildChunk();
        isChunkUpdated=false;
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
