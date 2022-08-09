namespace UnitySourceEngine
{
    public struct mstudiotexture_t
    {
        public string name;
        public int nameOffset;
        public int flags;
        public int used;
        public int unused1;
        public int materialP;
        public int clientMaterialP;
        public int[] unused; //SizeOf 10

        public ulong CountBytes()
        {
            return (ulong)((!string.IsNullOrEmpty(name) ? 2*name.Length : 0) + (unused != null ? 4*unused.Length : 0) + 24);
        }

        public void Dispose()
        {
            unused = null;
        }
    }
}