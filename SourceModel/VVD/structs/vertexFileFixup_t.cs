// apply sequentially to lod sorted vertex and tangent pools to re-establish mesh order
public struct vertexFileFixup_t
{
    public int lod; // used to skip culled root lod
    public int sourceVertexID; // absolute index from start of vertex/tangent blocks
    public int numVertices;
}