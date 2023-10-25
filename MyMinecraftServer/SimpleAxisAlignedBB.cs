using System.Collections;
using System.Collections.Generic;
using System.Numerics;

public class SimpleAxisAlignedBB : AxisAlignedBB {

    public float minX;
    public float minY;
    public float minZ;
    public float maxX;
    public float maxY;
    public float maxZ;

    public SimpleAxisAlignedBB(Vector3 pos1, Vector3 pos2) {
        this.minX = MathF.Min(pos1.X, pos2.X);
        this.minY = MathF.Min(pos1.Y, pos2.Y);
        this.minZ = MathF.Min(pos1.Z, pos2.Z);
        this.maxX =  MathF.Max(pos1.X, pos2.X);
        this.maxY = MathF.Max(pos1.Y, pos2.Y);
        this.maxZ = MathF.Max(pos1.Z, pos2.Z);
    }

    public SimpleAxisAlignedBB(float minX, float minY, float minZ, float maxX, float maxY, float maxZ) {
        this.minX = minX;
        this.minY = minY;
        this.minZ = minZ;
        this.maxX = maxX;
        this.maxY = maxY;
        this.maxZ = maxZ;
    }

     
    public string toString() {
        return "AxisAlignedBB(" + this.getMinX() + ", " + this.getMinY() + ", " + this.getMinZ() + ", " + this.getMaxX() + ", " + this.getMaxY() + ", " + this.getMaxZ() + ")";
    }

     
    public float getMinX() {
        return minX;
    }

     
    public void setMinX(float minX) {
        this.minX = minX;
    }

     
    public float getMinY() {
        return minY;
    }

     
    public void setMinY(float minY) {
        this.minY = minY;
    }

     
    public float getMinZ() {
        return minZ;
    }

     
    public void setMinZ(float minZ) {
        this.minZ = minZ;
    }

     
    public float getMaxX() {
        return maxX;
    }

     
    public void setMaxX(float maxX) {
        this.maxX = maxX;
    }

     
    public float getMaxY() {
        return maxY;
    }

     
    public void setMaxY(float maxY) {
        this.maxY = maxY;
    }

     
    public float getMaxZ() {
        return maxZ;
    }

     
    public void setMaxZ(float maxZ) {
        this.maxZ = maxZ;
    }
    
    public SimpleAxisAlignedBB offset(float x, float y, float z) {
        this.setMinX(this.getMinX() + x);
        this.setMinY(this.getMinY() + y);
        this.setMinZ(this.getMinZ() + z);
        this.setMaxX(this.getMaxX() + x);
        this.setMaxY(this.getMaxY() + y);
        this.setMaxZ(this.getMaxZ() + z);
      
        return this;
    }
    

      public float calculateXOffset(SimpleAxisAlignedBB bb, float x) {
        if (bb.getMaxY() <= this.getMinY() || bb.getMinY() >= this.getMaxY()) {
            return x;
        }
        if (bb.getMaxZ() <= this.getMinZ() || bb.getMinZ() >= this.getMaxZ()) {
            return x;
        }
        if (x > 0 && bb.getMaxX() <= this.getMinX()) {
            float x1 = this.getMinX() - bb.getMaxX();
            if (x1 < x) {
                x = x1;
            }
        }
        if (x < 0 && bb.getMinX() >= this.getMaxX()) {
            float x2 = this.getMaxX() - bb.getMinX();
            if (x2 > x) {
                x = x2;
            }
        }

        return x;
    }

    public float calculateYOffset(SimpleAxisAlignedBB bb, float y) {
        if (bb.getMaxX() <= this.getMinX() || bb.getMinX() >= this.getMaxX()) {
            return y;
        }
        if (bb.getMaxZ() <= this.getMinZ() || bb.getMinZ() >= this.getMaxZ()) {
            return y;
        }
        if (y > 0 && bb.getMaxY() <= this.getMinY()) {
            float y1 = this.getMinY() - bb.getMaxY();
            if (y1 < y) {
                y = y1;
            }
        }
        if (y < 0 && bb.getMinY() >= this.getMaxY()) {
            float y2 = this.getMaxY() - bb.getMinY();
            if (y2 > y) {
                y = y2;
            }
        }

        return y;
    }

    public float calculateZOffset(SimpleAxisAlignedBB bb, float z) {
        if (bb.getMaxX() <= this.getMinX() || bb.getMinX() >= this.getMaxX()) {
            return z;
        }
        if (bb.getMaxY() <= this.getMinY() || bb.getMinY() >= this.getMaxY()) {
            return z;
        }
        if (z > 0 && bb.getMaxZ() <= this.getMinZ()) {
            float z1 = this.getMinZ() - bb.getMaxZ();
            if (z1 < z) {
                z = z1;
            }
        }
        if (z < 0 && bb.getMinZ() >= this.getMaxZ()) {
            float z2 = this.getMaxZ() - bb.getMinZ();
            if (z2 > z) {
                z = z2;
            }
        }

        return z;
    }
    public AxisAlignedBB clone() {
        return new SimpleAxisAlignedBB(minX, minY, minZ, maxX, maxY, maxZ);
    }
   
}