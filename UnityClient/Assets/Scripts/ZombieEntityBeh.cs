using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ZombieEntityBeh : MonoBehaviour
{
    public static AudioClip zombieHurtClip;
    public Transform headTrans;
    public Transform bodyTrans;
    public Transform entityMoveRef;
    public bool isEntityHurt;
    public bool prevIsEntityHurt;
    public Bounds entityBounds;
    public Animator am;
    void Start()
    {
        zombieHurtClip=Resources.Load<AudioClip>("Audios/Zombie_hurt1");
        headTrans=transform.GetChild(0);
        bodyTrans=transform.GetChild(1);
        entityMoveRef=transform.GetChild(2);
        am=GetComponent<Animator>();
        entityBounds=new Bounds(transform.position+new Vector3(0f,0.9f,0f),new Vector3(0.6f,1.8f,0.6f));
    }

    Vector2 curpos;
    Vector2 lastpos;
    float Speed()
	{
		curpos = new Vector2(transform.position.x,transform.position.z);//当前点
		float _speed = (Vector3.Magnitude(curpos - lastpos) / Time.deltaTime);//与上一个点做计算除去当前帧花的时间。
		lastpos = curpos;//把当前点保存下一次用
		return _speed;
	}
    
    void Update()
    {
        if(NetworkProgram.isGamePaused==true){
            am.Play("zombiewalk",0,0f);
            return;
        }
        entityBounds.center=transform.position+new Vector3(0f,0.9f,0f);
        am.SetFloat("speed",Speed()/3f);
        entityMoveRef.rotation=Quaternion.Euler(0f,headTrans.eulerAngles.y,0f);
        bodyTrans.rotation=Quaternion.Lerp(bodyTrans.rotation,entityMoveRef.rotation,Time.deltaTime*10f);
        if(prevIsEntityHurt!=isEntityHurt){

        if(isEntityHurt==true){
            AudioSource.PlayClipAtPoint(zombieHurtClip,transform.position,1f);
            transform.GetChild(0).GetChild(0).GetComponent<MeshRenderer>().material.color=Color.red;
            transform.GetChild(1).GetChild(0).GetComponent<MeshRenderer>().material.color=Color.red;
            transform.GetChild(1).GetChild(1).GetChild(0).GetComponent<MeshRenderer>().material.color=Color.red;
            transform.GetChild(1).GetChild(2).GetChild(0).GetComponent<MeshRenderer>().material.color=Color.red;
            transform.GetChild(1).GetChild(3).GetChild(0).GetComponent<MeshRenderer>().material.color=Color.red;
            transform.GetChild(1).GetChild(4).GetChild(0).GetComponent<MeshRenderer>().material.color=Color.red;
        }else{
            transform.GetChild(0).GetChild(0).GetComponent<MeshRenderer>().material.color=Color.white;
            transform.GetChild(1).GetChild(0).GetComponent<MeshRenderer>().material.color=Color.white;
            transform.GetChild(1).GetChild(1).GetChild(0).GetComponent<MeshRenderer>().material.color=Color.white;
            transform.GetChild(1).GetChild(2).GetChild(0).GetComponent<MeshRenderer>().material.color=Color.white;
            transform.GetChild(1).GetChild(3).GetChild(0).GetComponent<MeshRenderer>().material.color=Color.white;
            transform.GetChild(1).GetChild(4).GetChild(0).GetComponent<MeshRenderer>().material.color=Color.white;
        }
        
        }
       prevIsEntityHurt=isEntityHurt;
    }
    
}
