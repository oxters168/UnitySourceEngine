using UnityEngine;

namespace UnitySourceEngine
{
    public struct mstudioeyeball_t
    {
        public string name;

        public int nameOffset; //4
        public int boneIndex; //4
        public Vector3 org; //12
        public double zOffset; //8
        public double radius; //8
        public Vector3 up; //12
        public Vector3 forward; //12
        public int texture; //4

        public int unused1; //4
        public double irisScale; //8
        public int unused2; //4

        public int[] upperFlexDesc;
        public int[] lowerFlexDesc;
        public double[] upperTarget;
        public double[] lowerTarget;

        public int upperLidFlexDesc; //4
        public int lowerLidFlexDesc; //4
        public int[] unused;
        public byte eyeballIsNonFacs; //1
        public char[] unused3;
        public int[] unused4;

        public int theTextureIndex; //4

        public ulong CountBytes()
        {
            return (ulong)((!string.IsNullOrEmpty(name) ? 2*name.Length : 0) + (upperFlexDesc != null ? 4*upperFlexDesc.Length : 0) + (lowerFlexDesc != null ? 4*lowerFlexDesc.Length : 0) + (upperTarget != null ? 8*upperTarget.Length : 0) + (lowerTarget != null ? 8*lowerTarget.Length : 0) + (unused != null ? 4*unused.Length : 0) + (unused3 != null ? 2*unused3.Length : 0) + (unused4 != null ? 4*unused4.Length : 0) + 93);
        }

        public void Dispose()
        {
            upperFlexDesc = null;
            lowerFlexDesc = null;
            upperTarget = null;
            lowerTarget = null;
            unused = null;
            unused3 = null;
            unused4 = null;
        }
    }
}