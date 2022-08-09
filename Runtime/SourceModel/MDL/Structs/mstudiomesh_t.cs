using UnityEngine;

namespace UnitySourceEngine
{
    public struct mstudiomesh_t
    {
        public int materialIndex; //4 bytes
        public int modelOffset; //4 bytes
        public int vertexCount; //4 bytes
        public int vertexIndexStart; //4 bytes
        public int flexCount; //4 bytes
        public int flexOffset; //4 bytes
        public int materialType; //4 bytes
        public int materialParam; //4 bytes
        public int id; //4 bytes
        public Vector3 center; //12 bytes
        public mstudio_meshvertexdata_t vertexData; //4+(4*lvc.length) bytes
        public int[] unused; //4*unused.length bytes
        public mstudioflex_t[] flexes; //49+(2*uc.length)+(4*u.length)+(4*tva.length) bytes

        public ulong CountBytes()
        {
            ulong totalBytes = (ulong)((unused != null ? 4*unused.Length : 0) + 48) + vertexData.CountBytes();
            if (flexes != null)
                foreach(var flex in flexes)
                    totalBytes += flex.CountBytes();
            return totalBytes;
        }

        public void Dispose()
        {
            vertexData.Dispose();
            unused = null;
            if (flexes != null)
                foreach (var flex in flexes)
                    flex.Dispose();
            flexes = null;
        }
    }
}