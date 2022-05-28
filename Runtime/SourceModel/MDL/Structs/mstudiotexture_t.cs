namespace UnitySourceEngine
{
    public class mstudiotexture_t
    {
        public string name;
        public int nameOffset;
        public int flags;
        public int used;
        public int unused1;
        public int materialP;
        public int clientMaterialP;
        public int[] unused; //SizeOf 10

        public void Dispose()
        {
            unused = null;
        }
    }
}