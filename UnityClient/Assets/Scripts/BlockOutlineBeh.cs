using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlockOutlineBeh : MonoBehaviour
{
    public Bounds blockOutlineBounds;
    void Start(){
        blockOutlineBounds=new Bounds(transform.position,new Vector3(1f,1f,1f));
    }
  public bool isCollidingWithPlayer=false;
  public void OnTriggerEnter(Collider other){
    if(other.gameObject.tag=="Player"){
        isCollidingWithPlayer=true;
    }else{
        isCollidingWithPlayer=false;
    }
  }
    public void OnTriggerExit(Collider other){
    
        isCollidingWithPlayer=false;
    
  }
  void Update(){
    blockOutlineBounds.center=transform.position;
  }
  public void CheckIsCollidingWithPlayer(){
        foreach(KeyValuePair<string,GameObject> g in AllPlayersManager.playerPrefabsInClient){
            if(blockOutlineBounds.Intersects(g.Value.GetComponent<PlayerMove>().playerBoundingBox)){
                isCollidingWithPlayer=true;
                return;
            }
        }
        isCollidingWithPlayer=false;
  }
}
