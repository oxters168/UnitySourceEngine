namespace UnitySourceEngine
{
    public struct mstudioiklock_t
    {
        public int chainIndex;
        public double posWeight;
        public double localQWeight;
        public int flags;
        public int[] unused; //SizeOf 4

        public ulong CountBytes()
        {
            return (ulong)((unused != null ? 4*unused.Length : 0) + 24);
        }

        public void Dispose()
        {
            unused = null;
        }
    }
}