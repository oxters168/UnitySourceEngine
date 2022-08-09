namespace UnitySourceEngine
{
    public struct SourceVtxModel
    {
        public int lodCount; //4
        public int lodOffset; //4

        public SourceVtxModelLod[] vtxModelLods;

        public ulong CountBytes()
        {
            ulong totalBytes = (ulong)(8);
            if (vtxModelLods != null)
                foreach (var vtxModelLod in vtxModelLods)
                    totalBytes += vtxModelLod.CountBytes();
            return totalBytes;
        }

        public void Dispose()
        {
            if (vtxModelLods != null)
                foreach (var modelLod in vtxModelLods)
                    modelLod.Dispose();
            vtxModelLods = null;
        }
    }
}