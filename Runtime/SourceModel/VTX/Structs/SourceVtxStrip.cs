using System;

namespace UnitySourceEngine
{
    public struct SourceVtxStrip
    {
        public int indexCount; //4
        public int indexMeshIndex; //4

        public int vertexCount; //4
        public int vertexMeshIndex; //4

        public short boneCount; //2

        public byte flags; //1

        public int boneStateChangeCount; //4
        public int boneStateChangeOffset; //4

        public SourceVtxBoneStateChange[] vtxBoneStateChanges;

        public ulong CountBytes()
        {
            return (ulong)((vtxBoneStateChanges != null ? 8*vtxBoneStateChanges.Length : 0) + 27);
        }
        public void Dispose()
        {
            vtxBoneStateChanges = null;
        }
    }

    [Flags]
    public enum StripHeaderFlags_t
    {
        STRIP_IS_TRILIST = 0x01,
        STRIP_IS_TRISTRIP = 0x02,
    }
}