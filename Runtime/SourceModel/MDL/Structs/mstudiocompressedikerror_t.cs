namespace UnitySourceEngine
{
    public class mstudiocompressedikerror_t
    {
        public float[] scale; //SizeOf 6
        public short[] offset; //SizeOf 6

        public mstudioanimvalue_t[] theAnimValues;

        public void Dispose()
        {
            scale = null;
            offset = null;
            theAnimValues = null;
        }
    }
}