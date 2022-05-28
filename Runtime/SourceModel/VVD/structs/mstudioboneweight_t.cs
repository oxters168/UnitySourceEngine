namespace UnitySourceEngine
{
    // 16 bytes
    public struct mstudioboneweight_t
    {
        public float[] weight; //(size of MAX_NUM_BONES_PER_VERT = 3)
        public char[] bone; //(size of MAX_NUM_BONES_PER_VERT = 3)
        public byte numbones;

        public void Dispose()
        {
            weight = null;
            bone = null;
        }
    }
}