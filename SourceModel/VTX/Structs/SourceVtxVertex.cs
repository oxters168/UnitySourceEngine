namespace UnitySourceEngine
{
    public class SourceVtxVertex
    {
        public byte[] boneWeightIndex; //(MAX_NUM_BONES_PER_VERT - 1)
        public byte boneCount;

        public ushort originalMeshVertexIndex;

        public byte[] boneId; //(MAX_NUM_BONES_PER_VERT - 1)

        public void Dispose()
        {
            boneWeightIndex = null;
            boneId = null;
        }
    }
}