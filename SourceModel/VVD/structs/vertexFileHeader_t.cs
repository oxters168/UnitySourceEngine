namespace UnitySourceEngine
{
    // these structures can be found in <mod folder>/src/public/studio.h
    public struct vertexFileHeader_t
    {
        public int id;             // MODEL_VERTEX_FILE_ID
        public int version;            // MODEL_VERTEX_FILE_VERSION
        public long checksum;          // same as studiohdr_t, ensures sync
        public int numLODs;            // num of valid lods
        public int[] numLODVertices;   // num verts for desired root lod (size is MAX_NUM_LODS = 8)
        public int numFixups;          // num of vertexFileFixup_t
        public int fixupTableStart;        // offset from base to fixup table
        public int vertexDataStart;        // offset from base to vertex block
        public int tangentDataStart;        // offset from base to tangent block

        public void Dispose()
        {
            numLODVertices = null;
        }
    }
}