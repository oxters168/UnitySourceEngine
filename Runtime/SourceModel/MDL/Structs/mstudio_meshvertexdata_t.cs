namespace UnitySourceEngine
{
    public class mstudio_meshvertexdata_t
    {
        public int modelVertexDataP;
        public int[] lodVertexCount;

        public void Dispose()
        {
            lodVertexCount = null;
        }
    }
}