using System;

namespace UnitySourceEngine
{
    public class SourceVtxStrip
    {
        public int indexCount;
        public int indexMeshIndex;

        public int vertexCount;
        public int vertexMeshIndex;

        public short boneCount;

        public byte flags;

        public int boneStateChangeCount;
        public int boneStateChangeOffset;

        public SourceVtxBoneStateChange[] theVtxBoneStateChanges;

        public void Dispose()
        {
            theVtxBoneStateChanges = null;
        }
    }

    [Flags]
    public enum StripHeaderFlags_t
    {
        STRIP_IS_TRILIST = 0x01,
        STRIP_IS_TRISTRIP = 0x02,
    }
}