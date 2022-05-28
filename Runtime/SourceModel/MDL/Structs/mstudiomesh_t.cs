using UnityEngine;

namespace UnitySourceEngine
{
    public class mstudiomesh_t
    {
        public int materialIndex;
        public int modelOffset;
        public int vertexCount;
        public int vertexIndexStart;
        public int flexCount;
        public int flexOffset;
        public int materialType;
        public int materialParam;
        public int id;
        public Vector3 center;
        public mstudio_meshvertexdata_t vertexData;
        public int[] unused;
        public mstudioflex_t[] theFlexes;

        public void Dispose()
        {
            vertexData?.Dispose();
            unused = null;
            if (theFlexes != null)
                foreach (var flex in theFlexes)
                    flex?.Dispose();
            theFlexes = null;
        }
    }
}