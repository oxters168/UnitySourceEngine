namespace UnitySourceEngine
{
    public class mstudioiklock_t
    {
        public int chainIndex;
        public double posWeight;
        public double localQWeight;
        public int flags;
        public int[] unused; //SizeOf 4

        public void Dispose()
        {
            unused = null;
        }
    }
}