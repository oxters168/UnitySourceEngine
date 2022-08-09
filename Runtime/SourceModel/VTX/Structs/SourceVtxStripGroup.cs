using System;

namespace UnitySourceEngine
{
    public struct SourceVtxStripGroup
    {
        public int vertexCount; //4
        public int vertexOffset; //4

        public int indexCount; //4
        public int indexOffset; //4

        public int stripCount; //4
        public int stripOffset; //4

        public byte flags; //1

        public int topologyIndexCount; //4
        public int topologyIndexOffset; //4

        public SourceVtxVertex[] vtxVertices;
        public ushort[] vtxIndices; //2*length
        public SourceVtxStrip[] vtxStrips;

        public ulong CountBytes()
        {
            ulong totalBytes = (ulong)((vtxIndices != null ? 2*vtxIndices.Length : 0) + 33);
            if (vtxVertices != null)
                foreach (var vtxIndex in vtxVertices)
                    totalBytes += vtxIndex.CountBytes();
            if (vtxStrips != null)
                foreach (var vtxStrip in vtxStrips)
                    totalBytes += vtxStrip.CountBytes();
            return totalBytes;
        }

        public void Dispose()
        {
            if (vtxVertices != null)
                foreach (var vtxVertex in vtxVertices)
                    vtxVertex.Dispose();
            vtxVertices = null;
            if (vtxStrips != null)
                foreach (var vtxStrip in vtxStrips)
                    vtxStrip.Dispose();
            vtxStrips = null;
        }
    }

    [Flags]
    public enum StripGroupFlags_t
    {
        STRIPGROUP_IS_FLEXED = 0x01,
        STRIPGROUP_IS_HWSKINNED = 0x02,
        STRIPGROUP_IS_DELTA_FIXED = 0x04,
        STRIPGROUP_SUPPRESS_HW_MORPH = 0x08,
    }
}