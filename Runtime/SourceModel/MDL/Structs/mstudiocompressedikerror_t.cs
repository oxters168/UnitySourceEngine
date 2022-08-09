namespace UnitySourceEngine
{
    public struct mstudiocompressedikerror_t
    {
        public float[] scale; //SizeOf 6
        public short[] offset; //SizeOf 6

        public mstudioanimvalue_t[] theAnimValues; //4*length

        public ulong CountBytes()
        {
            return (ulong)((scale != null ? 4*scale.Length : 0) + (offset != null ? 2*offset.Length : 0) + (theAnimValues != null ? 4*theAnimValues.Length : 0));
        }

        public void Dispose()
        {
            scale = null;
            offset = null;
            theAnimValues = null;
        }
    }
}