namespace UnitySourceEngine
{
    public class SourceVtxModel
    {
        public int lodCount;
        public int lodOffset;

        public SourceVtxModelLod[] theVtxModelLods;

        public void Dispose()
        {
            if (theVtxModelLods != null)
                foreach (SourceVtxModelLod modelLod in theVtxModelLods)
                    modelLod?.Dispose();
            theVtxModelLods = null;
        }
    }
}