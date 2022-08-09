namespace UnitySourceEngine
{
    public struct VVDData
    {
        public const int MAX_NUM_LODS = 7, MAX_NUM_BONES_PER_VERT = 3;
        public vertexFileHeader_t header;
        public vertexFileFixup_t[] fileFixup;
        public mstudiovertex_t[] vertices;

        public ulong CountBytes()
        {
            ulong totalBytes = (ulong)((fileFixup != null ? 12*fileFixup.Length : 0)) + header.CountBytes();
            if (vertices != null)
                foreach (var vertex in vertices)
                    totalBytes += vertex.CountBytes();
            return totalBytes;
        }
    }
}