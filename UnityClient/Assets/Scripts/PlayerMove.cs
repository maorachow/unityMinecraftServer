using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Utf8Json;
using System.Net.Sockets;
using UnityEngine.SceneManagement;
using System.Threading.Tasks;
public class PlayerMove : MonoBehaviour
{
    public static GameObject chunkPrefab;
    public static int viewRange=32;
    public static float mouseSens=5f;
    public Vector3 prePos;
    public Vector3 nowPos;
    public bool isCurrentPlayer=false;
    public float playerSpeed=5f;
    public float gravity=-9.8f;
    public float jumpHeight=5f;
    public float playerY;
    public float breakBlockCD=0f;
 //   public Transform currentPlayer;
    public CharacterController cc;
    public Transform cameraTrans;
    public float cameraX;
    public  static void SetBlock(float x,float y,float z,int type){
         BlockModifyData b = new BlockModifyData(x, y, z, type);

        NetworkProgram.SendMessageToServer(new Message("UpdateChunk",JsonSerializer.ToJsonString(b)));
    }
    void Start(){
        chunkPrefab=Resources.Load<GameObject>("Prefabs/chunk");
         cc=GetComponent<CharacterController>();
         cameraTrans=GameObject.Find("Main Camera").GetComponent<Transform>();   
    }
    async void UpdateWorld(){
        if(this==null){
            return;
        }
        for (float x = transform.position.x - viewRange; x < transform.position.x + viewRange; x += Chunk.chunkWidth)
        {
            for (float z = transform.position.z - viewRange; z < transform.position.z + viewRange; z += Chunk.chunkWidth)
            {
                Vector3 pos = new Vector3(x, 0, z);
               // pos.x = Mathf.Floor(pos.x / (float)Chunk.chunkWidth) * Chunk.chunkWidth;
            //    pos.z = Mathf.Floor(pos.z / (float)Chunk.chunkWidth) * Chunk.chunkWidth;
                Vector2Int chunkPos=Chunk.Vec3ToChunkPos(pos);
                Chunk chunk = Chunk.GetChunk(chunkPos);
                if (chunk != null) {continue;}else{
                  chunk=Instantiate(chunkPrefab,new Vector3(chunkPos.x,0f,chunkPos.y),Quaternion.identity).GetComponent<Chunk>();
               //     chunk.transform.position=new Vector3(chunkPos.x,0,chunkPos.y);
               //     chunk.isChunkPosInited=true;
                //    Debug.Log("genChunk");
                 await Task.Delay(10);
                   await Task.Run(()=>{NetworkProgram.SendMessageToServer(new Message("ChunkGen",JsonSerializer.ToJsonString(chunkPos)));});
         //          WorldManager.chunksToLoad.Add(chunk);
                }
            }
        }
     
    }
    void Update()
    {
       
        if(!isCurrentPlayer){
            nowPos=transform.position;
            transform.position=Vector3.Lerp(prePos,nowPos,5f*Time.deltaTime);
            prePos=transform.position;
            return;
        }

        cameraTrans.SetParent(transform);
        cameraTrans.localPosition=new Vector3(0f,0.5f,0f);
        
        NetworkProgram.currentPlayer.posX=transform.position.x;
        NetworkProgram.currentPlayer.posY=transform.position.y;
        NetworkProgram.currentPlayer.posZ=transform.position.z;
        NetworkProgram.currentPlayer.rotY=transform.eulerAngles.y;     


        float x = Input.GetAxis("Mouse X")*mouseSens;
        float y = Input.GetAxis("Mouse Y")*mouseSens;
        cameraX-=y;
       // float cameraX=cameraTrans.localEulerAngles.x+y;
        cameraX=Mathf.Clamp(cameraX,-90f,90f);
        cameraTrans.localEulerAngles=new Vector3(cameraX,0f,cameraTrans.localEulerAngles.z);
        transform.rotation*= Quaternion.Euler(0f, x, 0f);
        if(cc.isGrounded==true){
            playerY=0f;
        }else{
            playerY+=gravity*Time.deltaTime;
        }
        if(Input.GetButton("Jump")&&cc.isGrounded==true){
            playerY=jumpHeight;
        }
    //    Debug.Log(new Vector3(NetworkProgram.currentPlayer.posX,NetworkProgram.currentPlayer.posY,NetworkProgram.currentPlayer.posZ));
        cc.Move((Input.GetAxis("Horizontal")*transform.right+Input.GetAxis("Vertical")*transform.forward)*playerSpeed*Time.deltaTime);
        cc.Move(new Vector3(0f,playerY,0f)*Time.deltaTime);
  //      NetworkProgram.SendMessageToServer(new Message("UpdatesUer", JsonSerializer.ToJsonString(NetworkProgram.currentPlayer)));
            if(breakBlockCD>0f){
             breakBlockCD-=Time.deltaTime;   
            }
            
        if(Input.GetMouseButton(0)&&breakBlockCD<=0f){
            breakBlockCD=0.3f;
            BreakBlock();
        }

    }
    void FixedUpdate(){
        if(!isCurrentPlayer){
            return;
        }
        UpdateWorld();
    }
     void BreakBlock(){
        Ray ray=Camera.main.ScreenPointToRay(new Vector3(Screen.width / 2, Screen.height / 2, 0));
        RaycastHit info;
        if(Physics.Raycast(ray,out info,10f)){
            Vector3 hitBlockPoint=info.point+cameraTrans.forward*0.01f;
            SetBlock(hitBlockPoint.x,hitBlockPoint.y,hitBlockPoint.z,0);
        }
    }
}
