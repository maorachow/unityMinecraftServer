using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestClient
{

    public class Chunk
    {
        public Vector2Int chunkPos;
        public static int chunkWidth=30;
        public static int chunkHeight=20;
        public int[,,] map;

        public Chunk(Vector2Int chunkPos)
        {
            this.chunkPos = chunkPos;
        }
    }
}
