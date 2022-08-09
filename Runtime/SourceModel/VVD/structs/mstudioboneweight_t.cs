namespace UnitySourceEngine
{
    // 16 bytes
    public struct mstudioboneweight_t
    {
        public float[] weight; //(size of MAX_NUM_BONES_PER_VERT = 3) //4*length
        public char[] bone; //(size of MAX_NUM_BONES_PER_VERT = 3) //2*length
        public byte numbones; //1

        public ulong CountBytes()
        {
            return (ulong)((weight != null ? 4*weight.Length : 0) + (bone != null ? 2*bone.Length : 0) + 1);
        }

        public void Dispose()
        {
            weight = null;
            bone = null;
        }
    }
}