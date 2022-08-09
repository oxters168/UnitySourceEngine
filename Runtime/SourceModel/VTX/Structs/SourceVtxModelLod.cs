namespace UnitySourceEngine
{
    public struct SourceVtxModelLod
    {
        public int meshCount; //4
        public int meshOffset; //4
        public float switchPoint; //4

        public SourceVtxMesh[] vtxMeshes;

        public ulong CountBytes()
        {
            ulong totalBytes = (ulong)(12);
            if (vtxMeshes != null)
                foreach (var vtxMesh in vtxMeshes)
                    totalBytes += vtxMesh.CountBytes();
            return totalBytes;
        }
        public void Dispose()
        {
            if (vtxMeshes != null)
                foreach (var mesh in vtxMeshes)
                    mesh.Dispose();
            vtxMeshes = null;
        }
    }
}