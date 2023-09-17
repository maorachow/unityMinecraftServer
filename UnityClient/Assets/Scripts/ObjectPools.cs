using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;


public class ObjectPools : MonoBehaviour
{
      
        public static GameObject particlePrefab;
      
        public static ObjectPool<GameObject> particleEffectPool;
     //   public static MyChunkObjectPool chunkPool=new MyChunkObjectPool();
     //   public static ObjectPool<GameObject> creeperEntityPool;
     //   public static ObjectPool<GameObject> zombieEntityPool;
       // public static ObjectPool<GameObject> itemEntityPool;
      //  public static MyItemObjectPool itemEntityPool=new MyItemObjectPool();
        public void Start(){
        particlePrefab=Resources.Load<GameObject>("Prefabs/blockbreakingparticle");
      //  itemPrefab=Resources.Load<GameObject>("Prefabs/itementity");
        particleEffectPool=new ObjectPool<GameObject>(CreateEffect, GetEffect, ReleaseEffect, DestroyEffect, true, 10, 300);
      //  creeperEntityPool=new ObjectPool<GameObject>(CreateCreeper,GetCreeper,ReleaseCreeper,DestroyCreeper,true,10,300);
     //   zombieEntityPool=new ObjectPool<GameObject>(CreateZombie,GetZombie,ReleaseZombie,DestroyZombie,true,10,300);
     //   itemEntityPool=new ObjectPool<GameObject>(CreateItem,GetItem,ReleaseItem,DestroyItem,true,10,300);
     //   chunkPrefab=Resources.Load<GameObject>("Prefabs/chunk");
     ///   chunkPool.Object=chunkPrefab;
     //   chunkPool.maxCount=100;
     //   chunkPool.Init();
     //   itemEntityPool.Object=itemPrefab;
      //  itemEntityPool.maxCount=300;
     //   itemEntityPool.Init();
    }
  
    public GameObject CreateEffect()
    {
        GameObject gameObject = Instantiate(particlePrefab, transform.position, Quaternion.identity);
 
        return gameObject;
    }
    
    void GetEffect(GameObject gameObject)
    {
 
        gameObject.SetActive(true);
   
    }
    void ReleaseEffect(GameObject gameObject)
    {
        gameObject.SetActive(false);
  
    }
    void DestroyEffect(GameObject gameObject)
    {
    
        Destroy(gameObject);
    }
    


   
}
