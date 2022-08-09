namespace UnitySourceEngine
{
    public struct mstudiolocalhierarchy_t
    {
        public int boneIndex; //4
        public int boneNewParentIndex; //4

        public float startInfluence; //4
        public float peakInfluence; //4
        public float tailInfluence; //4
        public float endInfluence; //4

        public int startFrameIndex; //4

        public int localAnimOffset; //4

        public int[] unused; //SizeOf 4

        public mstudiocompressedikerror_t[] theLocalAnims;

        public ulong CountBytes()
        {
            ulong totalBytes = (ulong)((unused != null ? 4*unused.Length : 0) + 36);
            if (theLocalAnims != null)
                foreach(var localAnim in theLocalAnims)
                    totalBytes += localAnim.CountBytes();
            return totalBytes;
        }

        public void Dispose()
        {
            unused = null;
            if (theLocalAnims != null)
                foreach (var localAnim in theLocalAnims)
                    localAnim.Dispose();
            theLocalAnims = null;
        }
    }
}