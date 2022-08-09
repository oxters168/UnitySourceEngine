namespace UnitySourceEngine
{
    public struct SourceVtxVertex
    {
        public byte[] boneWeightIndex; //(MAX_NUM_BONES_PER_VERT - 1) //1*length
        public byte boneCount; //1

        public ushort originalMeshVertexIndex; //2

        public byte[] boneId; //(MAX_NUM_BONES_PER_VERT - 1) //1*length

        public ulong CountBytes()
        {
            return (ulong)((boneWeightIndex != null ? boneWeightIndex.Length : 0) + (boneId != null ? boneId.Length : 0) + 3);
        }
        public void Dispose()
        {
            boneWeightIndex = null;
            boneId = null;
        }
    }
}