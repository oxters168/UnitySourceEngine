namespace UnitySourceEngine
{
    public struct VTXData
    {
        public vtxheader_t header; //36 bytes
        public SourceVtxBodyPart[] bodyParts;

        public ulong CountBytes()
        {
            ulong totalBytes = (ulong)(36);
            if (bodyParts != null)
                foreach (var bodyPart in bodyParts)
                    totalBytes += bodyPart.CountBytes();
            return totalBytes;
        }
    }
}