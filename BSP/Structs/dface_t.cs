namespace UnitySourceEngine
{
    public struct dface_t
    {
        public ushort planenum; // the plane number
        public byte side; // faces opposite to the node's plane direction
        public byte onNode; // 1 if on node, 0 if in leaf
        public int firstedge; // index into surfedges
        public short numedges; // number of surfedges
        public short texinfo; // texture info
        public short dispinfo; // displacement info
        public short surfaceFogVolumeID; // ?
        public byte[] styles; // switchable lighting info
        public int lightofs; // offset into lightmap lump
        public float area; // face area in units^2
        public int[] LightmapTextureMinsInLuxels; // texture lighting info
        public int[] LightmapTextureSizeInLuxels; // texture lighting info
        public int origFace; // original face this was split from
        public ushort numPrims; // primitives
        public ushort firstPrimID;
        public uint smoothingGroups; // lightmap smoothing group

        public void Dispose()
        {
            styles = null;
            LightmapTextureMinsInLuxels = null;
            LightmapTextureSizeInLuxels = null;
        }
    }
}