namespace UnitySourceEngine
{
    public struct dedge_t
    {
        public ushort[] v;  // vertex indices

        public void Dispose()
        {
            v = null;
        }
    }
}