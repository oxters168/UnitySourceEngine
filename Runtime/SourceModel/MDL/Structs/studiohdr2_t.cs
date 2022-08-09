namespace UnitySourceEngine
{
    public struct studiohdr2_t //32+(4*r.length) bytes
    {
        // ??
        public int srcbonetransform_count; //4 bytes
        public int srcbonetransform_index; //4 bytes

        public int illumpositionattachmentindex; //4 bytes

        public float flMaxEyeDeflection; // If set to 0, then equivalent to cos(30) //4 bytes

        // mstudiolinearbone_t
        public int linearbone_index; //4 bytes

        public int sznameindex; //4 bytes
        public int m_nBoneFlexDriverCount; //4 bytes
        public int m_nBoneFlexDriverIndex; //4 bytes

        public int[] reserved; //4 * length bytes

        public ulong CountBytes()
        {
            return (ulong)(32 + (reserved != null ? 4*reserved.Length : 0));
        }

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
            output += "\n_reserved[" + (reserved != null ? reserved.Length.ToString() : "null") + "]:";
            if (reserved != null)
                for (int i = 0; i < reserved.Length; i++)
                    output += "\n    [" + i + "]: " + reserved[i];
            return output;
        }
    }
}