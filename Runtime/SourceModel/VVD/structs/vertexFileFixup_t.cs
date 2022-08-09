namespace UnitySourceEngine
{
    // apply sequentially to lod sorted vertex and tangent pools to re-establish mesh order
    public struct vertexFileFixup_t //12
    {
        public int lod; // used to skip culled root lod //4
        public int sourceVertexID; // absolute index from start of vertex/tangent blocks //4
        public int numVertices; //4
    }
}