using System;

namespace UnitySourceEngine
{
    public class SourceVtxMesh
    {
        public int stripGroupCount;
        public int stripGroupOffset;
        public byte flags;

        public SourceVtxStripGroup[] theVtxStripGroups;

        public void Dispose()
        {
            if (theVtxStripGroups != null)
                foreach (SourceVtxStripGroup stripGroup in theVtxStripGroups)
                    stripGroup?.Dispose();
            theVtxStripGroups = null;
        }
    }

    [Flags]
    public enum MeshFlags_t
    {
        MESH_IS_TEETH = 0x01,
        MESH_IS_EYES = 0x02,
    }
}