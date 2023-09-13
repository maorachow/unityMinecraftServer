using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

using System.Text.RegularExpressions;

public class Chunk : MonoBehaviour
{
    public bool isChunkUpdated=false;
  
    public static Dictionary<Vector2Int,Chunk> chunks=new Dictionary<Vector2Int,Chunk>();
    public static Chunk instance;
    public Mesh chunkMesh;
    public MeshFilter meshFilter;
    public MeshCollider meshCollider;
    public int[,,] map;
    public static int chunkWidth=16;
    public static int chunkHeight=64;
    [SerializeField]
    public Vector2Int chunkPos;
    void Start(){
        chunkPos=new Vector2Int((int)transform.position.x,(int)transform.position.z);

       // instance=this;
       
         chunks.TryAdd(chunkPos,this);
       Debug.Log(chunks.Count);
       
        meshFilter=GetComponent<MeshFilter>();
        meshCollider=GetComponent<MeshCollider>();
    }
    public void BuildChunk()
    {
  //  Debug.Log("BuildChunk");
    chunkMesh = new Mesh();
    List<Vector3> verts = new List<Vector3>();
    List<Vector2> uvs = new List<Vector2>();
    List<int> tris = new List<int>();
    
    //遍历chunk, 生成其中的每一个Block
    for (int x = 0; x < chunkWidth; x++)
    {
        for (int y = 0; y < chunkHeight; y++)
        {
            for (int z = 0; z < chunkWidth; z++)
            {
                BuildBlock(x, y, z, verts, uvs, tris);
            }
        }
    }
                
    chunkMesh.vertices = verts.ToArray();
    chunkMesh.uv = uvs.ToArray();
    chunkMesh.triangles = tris.ToArray();
    chunkMesh.RecalculateBounds();
    chunkMesh.RecalculateNormals();
    
    meshFilter.mesh = chunkMesh;
    meshCollider.sharedMesh = chunkMesh;
    }

void BuildBlock(int x, int y, int z, List<Vector3> verts, List<Vector2> uvs, List<int> tris)
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
    }

    bool CheckNeedBuildFace(int x, int y, int z)
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
