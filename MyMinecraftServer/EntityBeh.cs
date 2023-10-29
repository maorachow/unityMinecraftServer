using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Devices.Pwm;
using System.Numerics;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Windows.Forms;
using System.Diagnostics;
using Windows.System;
using MessagePack;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace MyMinecraftServer
{
    [MessagePackObject]
    public class EntityData
    {
        [Key(0)]
        public int typeid;
        [Key(1)]
        public float posX;
        [Key(2)]
        public float posY;
        [Key(3)]
        public float posZ;
        [Key(4)]
        public float rotX;
        [Key(5)]
        public float rotY;
        [Key(6)]
        public float rotZ;
        [Key(7)]
        public string entityID;
        [Key(8)]
        public float entityHealth;
        [Key(9)]
        public bool isEntityHurt;

        public EntityData(int typeid, float posX, float posY, float posZ, float rotX, float rotY, float rotZ, string entityID, float entityHealth, bool isEntityHurt)
        {
            this.typeid = typeid;
            this.posX = posX;
            this.posY = posY;
            this.posZ = posZ;
            this.rotX = rotX;
            this.rotY = rotY;
            this.rotZ = rotZ;
            this.entityID = entityID;
            this.entityHealth = entityHealth;
            this.isEntityHurt = isEntityHurt;
        }
    }

   
    public class EntityBeh
    {
        public static List<EntityBeh> worldEntities= new List<EntityBeh>();
        public Vector3 position;
        public float rotationX;
        public float rotationY;
        public float rotationZ;
        public int typeID;
        public string entityID;
        public SimpleAxisAlignedBB entityBounds;
        public Dictionary<Vector3Int,SimpleAxisAlignedBB> blocksAround;
        public static float gravity = -9.8f;
        public Vector3 entityVec;
        public Vector3 entitySize;
        public bool isGround = false;
        public float entityHealth;
        public bool isEntityHurt;
        public float entityHurtCD;
        public Vector3 entityMotionVec;
        public EntityBeh(Vector3 position, float rotationX, float rotationY, float rotationZ, int typeID, string entityID,float entityHealth,bool isEntityHurt)
        {
            this.position = position;
            this.rotationX = rotationX;
            this.rotationY = rotationY;
            this.rotationZ = rotationZ;
            this.typeID = typeID;
            this.entityID = entityID;
            this.entityHealth = entityHealth;
            this.isEntityHurt = isEntityHurt;
        }

        public static void SpawnNewEntity(Vector3 position, float rotationX, float rotationY, float rotationZ, int typeID)
        {
            EntityBeh tmp=new EntityBeh(position, rotationX, rotationY, rotationZ, typeID,System.Guid.NewGuid().ToString("N"),20f,false);
            tmp.entitySize = new Vector3(0.6f,1.8f,0.6f);
            tmp.InitBounds();
            worldEntities.Add(tmp);
        }
        public EntityData ToEntityData()
        {
            return new EntityData(this.typeID, this.position.X, this.position.Y, this.position.Z, this.rotationX, this.rotationY, this.rotationZ,this.entityID,entityHealth,isEntityHurt);
        }
        public void InitBounds()
        {
            entityBounds = new SimpleAxisAlignedBB(position-entitySize/2f, position + entitySize / 2f);
        }
        public bool CheckIsGround()
        {
            Vector3 pos = new Vector3((entityBounds.minX + entityBounds.maxX) / 2f, entityBounds.minY-0.1f, (entityBounds.minZ + entityBounds.maxZ) / 2f);
          
            int blockID = Chunk.GetBlock(pos);

            if (blockID > 0 && blockID < 100)
            {
                return true;
            }
            else
            {
                return false;
            }
         
        }
        Vector3 lastPos;
        public float Vec3Magnitude(Vector3 pos)
        {
            return (float)Math.Sqrt(pos.X * pos.X + pos.Y * pos.Y + pos.Z * pos.Z);
        }
        public void OnUpdate()
        {


            entityMotionVec = Vector3.Lerp(entityMotionVec, Vector3.Zero, 5f / 20f);

            float curSpeed = Vec3Magnitude((position - lastPos)/(1f/20f));
            lastPos = position;
            GetBlocksAround(entityBounds);
            bool isGround = CheckIsGround();
           // Bounds checkBounds = new Bounds(new Vector3(position.X,position.Y-1f,position.Z), entitySize);
        //    List<Bounds> blocks = GetBlocksAround(checkBounds);
         //   Debug.WriteLine(blocks.Count);
           // isGround = CheckIsGround();
          // Random random = new Random();
          //  entityVec.X =1f;
            //entityVec.Z = 1f;
          if(Program.allUserData.Count > 0)
            {
                Vector3 movePos = new Vector3(Program.allUserData[0].posX - position.X, 0, Program.allUserData[0].posZ - position.Z);
                Vector3 lookPos = new Vector3(Program.allUserData[0].posX - position.X, Program.allUserData[0].posY - position.Y-1f, Program.allUserData[0].posZ - position.Z);
                Vector3 movePosN=Vector3.Normalize(movePos)*0.3f;
                entityVec = movePosN;
                Vector3 entityRot = LookRotation(lookPos);
                rotationX=entityRot.X; rotationY=entityRot.Y; rotationZ=entityRot.Z;
                //   Debug.WriteLine(rotationX);
                //  Debug.WriteLine(rotationY);
           //     Debug.WriteLine("XRot:" + rotationX + " " + "YRot:" + rotationY);
            }
            
       ////     if (isGround==true)
       //     {
        //        Debug.WriteLine("isground");
        //        entityVec.Y = 0f;
       //     }
       //     else
    //        {
      //        
        //    EntityMove(entityVec.X,entityVec.Y,entityVec.Z);
            if(isGround)
            {
          //      Debug.WriteLine("ground");
         //   entityVec.Y =0f;
            }
            else
            {
                entityVec.Y += -9.8f / 20f;
            }

            //  Debug.WriteLine(curSpeed);

            //     }

            //   EntityMove(entityVec.X, entityVec.Y, entityVec.Z);

            //     Debug.WriteLine(position.X + " " + position.Y + " " + position.Z);
            if (entityHealth < 0f)
            {
                worldEntities.Remove(this);
              
            }
            switch (typeID)
            {
                case 0:
                    if (entityHurtCD >= 0f)
                    {
                    entityHurtCD -= (1f / 20f);
                        isEntityHurt = true;
                    }
                    else
                    {
                        isEntityHurt=false;
                    }
                    
                    
               
                    if (Program.allUserData.Count > 0) {
                    Vector3 movePos = new Vector3(Program.allUserData[0].posX - position.X, 0, Program.allUserData[0].posZ - position.Z);
                    if (isGround&&curSpeed<=0.1f&& Vec3Magnitude(movePos) > 2f)
                    {
                      //  Debug.WriteLine("jump");
                      //  entityBounds = entityBounds.offset(0f, 0.1f, 0f);
                        entityVec.Y = 2f;
                    }

                        
                        if (Vec3Magnitude(movePos) > 2f)
                        {
                        EntityMove(entityVec.X, entityVec.Y, entityVec.Z);
                        }
                       
                        EntityMove(entityMotionVec.X, entityVec.Y+entityMotionVec.Y, entityMotionVec.Z);
                        
                    }
                    
               
                    break;
            }
            
        }
        public static void HurtEntity(string entityID,float hurtValue,Vector3 sourcePos)
        {
            EntityBeh entityBeh;
            int index = worldEntities.FindIndex((EntityBeh e) => { return entityID == e.entityID; });
            if (index != -1)
            {
                entityBeh = worldEntities[index];
            }
            else
            {
                return;
            }
            if (entityBeh.isEntityHurt == true)
            {
                return;
            }
            entityBeh.entityHealth -= hurtValue;
            entityBeh.entityHurtCD = 0.2f;
            entityBeh.entityMotionVec =entityBeh.position-sourcePos;
        }
        public Vector3 LookRotation(Vector3 fromDir)
        {
            Vector3 eulerAngles = new Vector3();

            //AngleX = arc cos(sqrt((x^2 + z^2)/(x^2+y^2+z^2)))
            eulerAngles.X = (float)Math.Acos(Math.Sqrt((fromDir.X * fromDir.X + fromDir.Z * fromDir.Z) / (fromDir.X * fromDir.X + fromDir.Y * fromDir.Y + fromDir.Z * fromDir.Z)))* 360f / (MathF.PI * 2f);
            if (fromDir.Y > 0) eulerAngles.X = 360f - eulerAngles.X;

            //AngleY = arc tan(x/z)
            eulerAngles.Y = (float)Math.Atan2((float)fromDir.X, (float)fromDir.Z) * 360f / (MathF.PI * 2f);
            if (eulerAngles.Y < 0) eulerAngles.Y += 180f;
            if (fromDir.X < 0) eulerAngles.Y += 180f;
            //AngleZ = 0
            eulerAngles.Z = 0f;
            return eulerAngles;
        }


        void EntityMove(float dx, float dy, float dz)
        {
           

            float movX = dx;
            float movY = dy;
            float movZ = dz;
            if (blocksAround.Count == 0)
            {
                entityBounds = entityBounds.offset(0, dy, 0);
                entityBounds = entityBounds.offset(dx, 0, 0);
                entityBounds = entityBounds.offset(0, 0, dz);
            }





            foreach (var bb in blocksAround)
            {
                dy = bb.Value.calculateYOffset(entityBounds, dy);
            }

            entityBounds = entityBounds.offset(0, dy, 0);

            //      bool fallingFlag = (this.onGround || (dy != movY && movY < 0));

            foreach (var bb in blocksAround)
            {
                dx = bb.Value.calculateXOffset(entityBounds, dx);
            }

            entityBounds = entityBounds.offset(dx, 0, 0);

            foreach (var bb in blocksAround)
            {
                dz = bb.Value.calculateZOffset(entityBounds, dz);
            }

            entityBounds = entityBounds.offset(0, 0, dz);
            position = new Vector3((entityBounds.minX + entityBounds.maxX) / 2f, entityBounds.minY, (entityBounds.minZ + entityBounds.maxZ) / 2f);
        }
        public Dictionary<Vector3Int, SimpleAxisAlignedBB> GetBlocksAround(SimpleAxisAlignedBB aabb)
        {

            int minX = Program.FloorFloat(aabb.getMinX() - 0.1f);
            int minY = Program.FloorFloat(aabb.getMinY() - 0.1f);
            int minZ = Program.FloorFloat(aabb.getMinZ() - 0.1f);
            int maxX = Program.CeilFloat(aabb.getMaxX() + 0.1f);
            int maxY = Program.CeilFloat(aabb.getMaxY() + 0.1f);
            int maxZ = Program.FloorFloat(aabb.getMaxZ() + 0.1f);

            this.blocksAround = new Dictionary<Vector3Int, SimpleAxisAlignedBB>();

            for (int z = minZ - 1; z <= maxZ + 1; z++)
            {
                for (int x = minX - 1; x <= maxX + 1; x++)
                {
                    for (int y = minY - 1; y <= maxY + 1; y++)
                    {
                        int blockID =Chunk.GetBlock(new Vector3(x, y, z));
                        if (blockID > 0 && blockID < 100)
                        {
                            this.blocksAround.Add(new Vector3Int(x, y, z), new SimpleAxisAlignedBB(x, y, z, x + 1, y + 1, z + 1));
                        }
                    }
                }
            }


            return this.blocksAround;


        }
    }

}