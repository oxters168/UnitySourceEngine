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

        public override string ToString()
        {
            string output = string.Empty;
            output += "_srcbonetransform_count(" + srcbonetransform_count + ")";
            output += "\n_srcbonetransform_index(" + srcbonetransform_index + ")";
            output += "\n_illumpositionattachmentindex(" + illumpositionattachmentindex + ")";
            output += "\n_flMaxEyeDeflection(" + flMaxEyeDeflection + ")";//new string(name).Replace("\0", "") + ")";
            output += "\n_linearbone_index(" + linearbone_index + ")";
            output += "\n_sznameindex(" + sznameindex + ")";
            output += "\n_m_nBoneFlexDriverCount(" + m_nBoneFlexDriverCount + ")";
            output += "\n_m_nBoneFlexDriverIndex(" + m_nBoneFlexDriverIndex + ")";
            output += "\n_reserved[" + (reserved != null ? reserved.Length.ToString() : "null") + "]";
            if (reserved != null)
                for (int i = 0; i < reserved.Length; i++)
                    output += "\n   [" + i + "]: " + reserved[i];

            return output;
        }
    }
}