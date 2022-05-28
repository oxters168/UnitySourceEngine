using UnityEngine;

namespace UnitySourceEngine
{
    public class mstudioeyeball_t
    {
        public string name;

        public int nameOffset;
        public int boneIndex;
        public Vector3 org;
        public double zOffset;
        public double radius;
        public Vector3 up;
        public Vector3 forward;
        public int texture;

        public int unused1;
        public double irisScale;
        public int unused2;

        public int[] upperFlexDesc;
        public int[] lowerFlexDesc;
        public double[] upperTarget;
        public double[] lowerTarget;

        public int upperLidFlexDesc;
        public int lowerLidFlexDesc;
        public int[] unused;
        public byte eyeballIsNonFacs;
        public char[] unused3;
        public int[] unused4;

        public int theTextureIndex;

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