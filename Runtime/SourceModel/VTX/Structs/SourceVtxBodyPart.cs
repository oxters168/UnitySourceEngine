namespace UnitySourceEngine
{
    public struct SourceVtxBodyPart
    {
        public int modelCount; //4
        public int modelOffset; //4

        public SourceVtxModel[] vtxModels;

        public ulong CountBytes()
        {
            ulong totalBytes = (ulong)(8);
            if (vtxModels != null)
                foreach (var vtxModel in vtxModels)
                    totalBytes += vtxModel.CountBytes();
            return totalBytes;
        }

        public void Dispose()
        {
            if (vtxModels != null)
                foreach (var model in vtxModels)
                    model.Dispose();
            vtxModels = null;
        }
    }
}