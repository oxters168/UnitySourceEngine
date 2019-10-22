namespace UnitySourceEngine
{
    public struct studiohdr2_t
    {
        // ??
        public int srcbonetransform_count;
        public int srcbonetransform_index;

        public int illumpositionattachmentindex;

        public float flMaxEyeDeflection; // If set to 0, then equivalent to cos(30)

        // mstudiolinearbone_t
        public int linearbone_index;

        public int sznameindex;
        public int m_nBoneFlexDriverCount;
        public int m_nBoneFlexDriverIndex;

        public int[] reserved;
    }
}