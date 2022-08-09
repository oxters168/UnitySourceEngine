namespace UnitySourceEngine
{
    public struct mstudio_meshvertexdata_t //4+(4*lvc.length) bytes
    {
        public int modelVertexDataP; //4 bytes
        public int[] lodVertexCount; //4*lodVertexCount.length

        public ulong CountBytes()
        {
            return (ulong)((lodVertexCount != null ? 4*lodVertexCount.Length : 0) + 4);
        }

        public void Dispose()
        {
            lodVertexCount = null;
        }
    }
}