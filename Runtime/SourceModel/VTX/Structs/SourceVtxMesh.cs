using System;

namespace UnitySourceEngine
{
    public struct SourceVtxMesh
    {
        public int stripGroupCount; //4
        public int stripGroupOffset; //4
        public byte flags; //1

        public SourceVtxStripGroup[] vtxStripGroups;

        public ulong CountBytes()
        {
            ulong totalBytes = (ulong)(9);
            if (vtxStripGroups != null)
                foreach (var vtxStripGroup in vtxStripGroups)
                    totalBytes += vtxStripGroup.CountBytes();
            return totalBytes;
        }

        public void Dispose()
        {
            if (vtxStripGroups != null)
                foreach (var stripGroup in vtxStripGroups)
                    stripGroup.Dispose();
            vtxStripGroups = null;
        }
    }

    [Flags]
    public enum MeshFlags_t
    {
        MESH_IS_TEETH = 0x01,
        MESH_IS_EYES = 0x02,
    }
}