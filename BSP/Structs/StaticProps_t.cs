namespace UnitySourceEngine
{
    public class StaticProps_t
    {
        public StaticPropDictLump_t staticPropDict;
        public StaticPropLeafLump_t staticPropLeaf;
        public StaticPropLump_t[] staticPropInfo;

        public void Dispose()
        {
            staticPropInfo = null;
        }
    }
}