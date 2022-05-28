namespace UnitySourceEngine
{
    public class SourceVtxBodyPart
    {
        public int modelCount;
        public int modelOffset;

        public SourceVtxModel[] theVtxModels;

        public void Dispose()
        {
            if (theVtxModels != null)
                foreach (SourceVtxModel model in theVtxModels)
                    model?.Dispose();
            theVtxModels = null;
        }
    }
}