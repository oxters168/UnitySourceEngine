namespace UnitySourceEngine
{
    public class SourceVtxModelLod
    {
        public int meshCount;
        public int meshOffset;
        public float switchPoint;

        public SourceVtxMesh[] theVtxMeshes;

        public void Dispose()
        {
            if (theVtxMeshes != null)
                foreach (SourceVtxMesh mesh in theVtxMeshes)
                    mesh?.Dispose();
            theVtxMeshes = null;
        }
    }
}