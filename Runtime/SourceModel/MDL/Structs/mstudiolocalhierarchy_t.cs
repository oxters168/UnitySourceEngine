namespace UnitySourceEngine
{
    public class mstudiolocalhierarchy_t
    {
        public int boneIndex;
        public int boneNewParentIndex;

        public float startInfluence;
        public float peakInfluence;
        public float tailInfluence;
        public float endInfluence;

        public int startFrameIndex;

        public int localAnimOffset;

        public int[] unused; //SizeOf 4

        public mstudiocompressedikerror_t[] theLocalAnims;

        public void Dispose()
        {
            unused = null;
            if (theLocalAnims != null)
                foreach (var localAnim in theLocalAnims)
                    localAnim?.Dispose();
            theLocalAnims = null;
        }
    }
}