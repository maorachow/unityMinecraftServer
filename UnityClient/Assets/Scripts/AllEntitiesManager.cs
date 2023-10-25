using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AllEntitiesManager : MonoBehaviour
{
    public static GameObject zombiePrefab;
   public static Dictionary<string,GameObject> entityPrefabsInClient=new Dictionary<string,GameObject>();
   public static List<EntityData> allEntityData=new List<EntityData>();

    void UpdateAllEntities(){
        foreach(EntityData e in allEntityData){
            if(!entityPrefabsInClient.ContainsKey(e.entityID)){
                if(e.typeid==0){
                  GameObject a=Instantiate(zombiePrefab,new Vector3(e.posX,e.posY,e.posZ),Quaternion.Euler(new Vector3(e.rotX,e.rotY,e.rotZ)));
                  entityPrefabsInClient.Add(e.entityID,a);

                }
                    
            }
        }
        foreach(EntityData et in allEntityData){
            if(entityPrefabsInClient.ContainsKey(et.entityID)){
                entityPrefabsInClient[et.entityID].transform.position=Vector3.Lerp(entityPrefabsInClient[et.entityID].transform.position,new Vector3(et.posX,et.posY,et.posZ),10f*Time.deltaTime);
                 entityPrefabsInClient[et.entityID].transform.rotation=Quaternion.Euler(new Vector3(et.rotX,et.rotY,et.rotZ));
            }
        }
    }
    void Awake(){
        zombiePrefab=Resources.Load<GameObject>("Prefabs/testentity");
    }
    void ClearEntities(){
              List<KeyValuePair<string,GameObject>> toDestroy=new List<KeyValuePair<string,GameObject>>();
         foreach(KeyValuePair<string,GameObject> e in entityPrefabsInClient){
            if(allEntityData.FindIndex(delegate (EntityData cl) { return cl.entityID ==e.Key; })==-1){
                toDestroy.Add(e);
            }
        }
        for(int i=0;i<toDestroy.Count;i++){
            Destroy(toDestroy[i].Value);
            entityPrefabsInClient.Remove(toDestroy[i].Key);
          //  return;
        }
    }
    // Update is called once per frame
    void FixedUpdate()
    {
        UpdateAllEntities();
        ClearEntities();
    }
}
