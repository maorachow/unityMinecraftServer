using System.Collections;
using System.Collections.Generic;
using UnityEngine;
//using Newtonsoft.Json;
using Newtonsoft.Json;
using System.Net.Sockets;
using UnityEngine.SceneManagement;
using System.Threading.Tasks;
using MessagePack;
public class PlayerMove : MonoBehaviour
{
    public Bounds playerBoundingBox;
    public static GameObject blockOutlinePrefab;
    public GameObject blockOutlineInBlock;
    public GameObject blockOutlineColliding;
    public static GameObject chunkPrefab;
    public static int viewRange=128;
    public static float mouseSens=10f;
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
    public Transform headTrans;
    public Transform playerMoveRef;
    public Transform bodyTrans;
    public float cameraX;
    public bool isPlayerAttacking=false;
    public Vector2 curpos;
    public Vector2 lastpos;
    public List<Vector2> speedTempPosList;
    public float playerAnimSpeed;
    public Camera mainCam;
    public Animator am;
    void Start(){
        playerBoundingBox=new Bounds(transform.position,new Vector3(0.8f,1.8f,0.8f));
        if(blockOutlinePrefab==null){
       blockOutlinePrefab=Resources.Load<GameObject>("Prefabs/blockoutline");       
        }
        blockOutlineInBlock=Instantiate(blockOutlinePrefab,transform.position,Quaternion.identity);
        blockOutlineColliding=Instantiate(blockOutlinePrefab,transform.position,Quaternion.identity);
        prePos=transform.position;
        if(chunkPrefab==null){
          chunkPrefab=Resources.Load<GameObject>("Prefabs/chunk");  
        }
        mainCam=Camera.main;
         cc=GetComponent<CharacterController>();
         cameraTrans=GameObject.Find("Main Camera").GetComponent<Transform>();   
         bodyTrans=transform.GetChild(0).GetChild(1);
         headTrans=transform.GetChild(0).GetChild(0);
         playerMoveRef=headTrans.GetChild(1);
         am=GetComponent<Animator>();
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
                  chunk=ObjectPools.chunkPool.Get(chunkPos).GetComponent<Chunk>();
                    chunk.transform.position=new Vector3(chunkPos.x,0,chunkPos.y);
                    chunk.isChunkPosInited=true;
                 //   Debug.Log("genChunk");
                 chunk.SendMessage("ReInitData");
                 await Task.Delay(30);
                 NetworkProgram.SendMessageToServer(new MessageProtocol(134,MessagePackSerializer.Serialize(chunkPos,NetworkProgram.lz4Options)));
         //          WorldManager.chunksToLoad.Add(chunk);
                }
            }
        }
     
    }
    public float GetSpeed(){
        if(NetworkProgram.isGamePaused==true){
            return speedTempPosList[0].magnitude/(1f/144f);
        }
        speedTempPosList .Add(new Vector2(transform.position.x,transform.position.z));//当前点
     //   List<float> tmp=new List<float>();
        float tmpspeed=0f;
       // int tmpspeedCount=0;
        for(int i=0;i<9;i++){
            if(i+1<speedTempPosList.Count){
                 tmpspeed += (Vector3.Magnitude(speedTempPosList[i+1] - speedTempPosList[i]) )/2f; 
              //   tmpspeedCount++;
            }
         
       //     tmp.Add(_speed);
        }
        tmpspeed=(tmpspeed/8);
        if(speedTempPosList.Count>10){
             speedTempPosList.RemoveAt(0);
        }
       
		////与上一个点做计算除去当前帧花的时间。
		//lastpos = curpos;//把当前点保存下一次用
		return tmpspeed/Time.deltaTime;

    }
    void Update()
    {
        if(NetworkProgram.isGamePaused==true){
            am.Play("playerwalk",0,0f);
            return;
        }
        playerAnimSpeed=GetSpeed();
        am.SetFloat("speed",playerAnimSpeed);
        am.SetBool("isattacking",isPlayerAttacking);
        
       playerBoundingBox.center=transform.position;
       
        
        if(!isCurrentPlayer){
        blockOutlineInBlock.GetComponent<MeshRenderer>().enabled=false;
            blockOutlineColliding.GetComponent<MeshRenderer>().enabled=false;
     //       nowPos=transform.position;
       //     transform.position=Vector3.Lerp(prePos,nowPos,20f*Time.deltaTime);
        //    prePos=transform.position;
             playerMoveRef.eulerAngles=new Vector3(0f,playerMoveRef.eulerAngles.y,0f);
            bodyTrans.rotation=Quaternion.Lerp(bodyTrans.rotation,Quaternion.Euler(0f,headTrans.eulerAngles.y,0f),5f*Time.deltaTime);
            am.SetFloat("speed",playerAnimSpeed);
            am.SetBool("isattacking",isPlayerAttacking);
            return;
        }
         
            am.SetFloat("speed",playerAnimSpeed);
            am.SetBool("isattacking",isPlayerAttacking);
      

       
        Ray ray=mainCam.ScreenPointToRay(new Vector3(Screen.width / 2, Screen.height / 2, 0));
        RaycastHit info;
         if(Physics.Raycast(ray,out info,10f)){
            Vector3 hitBlockPoint=info.point+cameraTrans.forward*0.01f;
            Vector3 placeBlockPoint=info.point-cameraTrans.forward*0.01f;
            blockOutlineInBlock.GetComponent<MeshRenderer>().enabled=true;
            blockOutlineColliding.GetComponent<MeshRenderer>().enabled=false;
             blockOutlineInBlock.transform.position=new Vector3(Chunk.Vec3ToBlockPos(hitBlockPoint).x+0.5f,Chunk.Vec3ToBlockPos(hitBlockPoint).y+0.5f,Chunk.Vec3ToBlockPos(hitBlockPoint).z+0.5f);
            blockOutlineColliding.transform.position=new Vector3(Chunk.Vec3ToBlockPos(placeBlockPoint).x+0.5f,Chunk.Vec3ToBlockPos(placeBlockPoint).y+0.5f,Chunk.Vec3ToBlockPos(placeBlockPoint).z+0.5f);
        }else{
             blockOutlineInBlock.transform.position=transform.position;
            blockOutlineColliding.transform.position=transform.position;
              blockOutlineInBlock.GetComponent<MeshRenderer>().enabled=false;
            blockOutlineColliding.GetComponent<MeshRenderer>().enabled=false;
        }
        if(cameraTrans.parent==null){
         cameraTrans.SetParent(headTrans);   
        cameraTrans.localPosition=new Vector3(0f,0.3f,-0.1f); 
        cameraTrans.localEulerAngles=new Vector3(0f,0f,0f);
        }
        
        
        
        NetworkProgram.currentPlayer.posX=transform.position.x;
        NetworkProgram.currentPlayer.posY=transform.position.y;
        NetworkProgram.currentPlayer.posZ=transform.position.z;
        NetworkProgram.currentPlayer.rotX=headTrans.eulerAngles.x;     
        NetworkProgram.currentPlayer.rotY=headTrans.eulerAngles.y;    
        NetworkProgram.currentPlayer.rotZ=headTrans.eulerAngles.z;    
        NetworkProgram.currentPlayer.isAttacking=isPlayerAttacking;    
        float x = Input.GetAxis("Mouse X")*mouseSens;
        float y = Input.GetAxis("Mouse Y")*mouseSens;
        y=Mathf.Clamp(y,-90f,90f);
        cameraX-=y;
       // float cameraX=cameraTrans.localEulerAngles.x+y;
        cameraX=Mathf.Clamp(cameraX,-90f,90f);
        
        headTrans.rotation=Quaternion.Euler(new Vector3(cameraX,headTrans.eulerAngles.y,0f));
        headTrans.eulerAngles+=new Vector3(0f,x,0f);
        playerMoveRef.eulerAngles=new Vector3(0f,headTrans.eulerAngles.y,0f);
        bodyTrans.rotation=Quaternion.Lerp(bodyTrans.rotation,playerMoveRef.rotation,5f*Time.deltaTime);
           if(Chunk.GetChunk(Chunk.Vec3ToChunkPos(transform.position))==null||Chunk.GetChunk(Chunk.Vec3ToChunkPos(transform.position)).isChunkDataDownloaded==false||Chunk.GetChunk(Chunk.Vec3ToChunkPos(transform.position)).isChunkUpdated==true){
            return;
        }
        if(cc.isGrounded==true){
            playerY=0f;
        }else{
            playerY+=gravity*Time.deltaTime;
        }
        if(Input.GetButton("Jump")&&cc.isGrounded==true){
            playerY=jumpHeight;
        }
    //    Debug.Log(new Vector3(NetworkProgram.currentPlayer.posX,NetworkProgram.currentPlayer.posY,NetworkProgram.currentPlayer.posZ));
        cc.Move((Input.GetAxis("Horizontal")*playerMoveRef.right+Input.GetAxis("Vertical")*playerMoveRef.forward)*playerSpeed*Time.deltaTime);
        cc.Move(new Vector3(0f,playerY,0f)*Time.deltaTime);
  //      NetworkProgram.SendMessageToServer(new Message("UpdatesUer", MessagePackSerializer.Serialize(NetworkProgram.currentPlayer)));
            if(breakBlockCD>0f){
             breakBlockCD-=Time.deltaTime;   
            }
            
        if(Input.GetMouseButton(0)&&breakBlockCD<=0f){
            breakBlockCD=0.3f;
            BreakBlock();
        }
         if(Input.GetMouseButton(1)&&breakBlockCD<=0f){
            breakBlockCD=0.3f;
            PlaceBlock();
        }

    }
    void FixedUpdate(){
        if(!isCurrentPlayer){
            return;
        }
        UpdateWorld();
    }
    void InvokeStopAttack(){
        isPlayerAttacking=false;
    }
     void BreakBlock(){
        Ray ray=mainCam.ScreenPointToRay(new Vector3(Screen.width / 2, Screen.height / 2, 0));
        RaycastHit info;
        if(Physics.Raycast(ray,out info,10f)){
            Vector3 hitBlockPoint=info.point+cameraTrans.forward*0.01f;
            isPlayerAttacking=true;
            Invoke("InvokeStopAttack",0.3f);
            StartCoroutine(Chunk.SetBlock(hitBlockPoint.x,hitBlockPoint.y,hitBlockPoint.z,0));

        }
    }
     void PlaceBlock(){
        Ray ray=mainCam.ScreenPointToRay(new Vector3(Screen.width / 2, Screen.height / 2, 0));
        RaycastHit info;
        if(Physics.Raycast(ray,out info,10f)){
            Vector3 hitBlockPoint=info.point-cameraTrans.forward*0.01f;
            blockOutlineColliding.GetComponent<BlockOutlineBeh>().CheckIsCollidingWithPlayer();
            if(blockOutlineColliding.GetComponent<BlockOutlineBeh>().isCollidingWithPlayer!=true){
            isPlayerAttacking=true;
            Invoke("InvokeStopAttack",0.3f);
            StartCoroutine(Chunk.SetBlock(hitBlockPoint.x,hitBlockPoint.y,hitBlockPoint.z,1));
            }
         

        }
    }
    
}
