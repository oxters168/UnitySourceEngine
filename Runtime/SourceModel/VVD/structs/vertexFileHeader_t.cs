namespace UnitySourceEngine
{
    // these structures can be found in <mod folder>/src/public/studio.h
    public struct vertexFileHeader_t
    {
        public int id;             // MODEL_VERTEX_FILE_ID //4
        public int version;            // MODEL_VERTEX_FILE_VERSION //4
        public long checksum;          // same as studiohdr_t, ensures sync //8
        public int numLODs;            // num of valid lods //4
        public int[] numLODVertices;   // num verts for desired root lod (size is MAX_NUM_LODS = 8) //4*length
        public int numFixups;          // num of vertexFileFixup_t //4
        public int fixupTableStart;        // offset from base to fixup table //4
        public int vertexDataStart;        // offset from base to vertex block //4
        public int tangentDataStart;        // offset from base to tangent block //4

        public ulong CountBytes()
        {
            return (ulong)((numLODVertices != null ? 4*numLODVertices.Length : 0) + 28);
        }

        public void Dispose()
        {
            numLODVertices = null;
        }
    }
}