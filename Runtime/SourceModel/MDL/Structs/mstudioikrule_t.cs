using UnityEngine;

namespace UnitySourceEngine
{
    public struct mstudioikrule_t
    {
        public string name;

        public int index; //4
        public int type; //4
        public int chain; //4
        public int bone; //4

        public int slot; //4
        public double height; //8
        public double radius; //8
        public double floor; //8
        public Vector3 pos; //12
        public Quaternion q; //16

        public int compressedIkErrorOffset; //4
        public int unused2; //4
        public int ikErrorIndexStart; //4
        public int ikErrorOffset; //4

        public double influenceStart; //8
        public double influencePeak; //8
        public double influenceTail; //8
        public double influenceEnd; //8

        public double unused3; //8
        public double contact; //8
        public double drop; //8
        public double top; //8

        public int unused6; //4
        public int unused7; //4
        public int unused8; //4

        public int attachmentNameOffset; //4

        public int[] unused; //SizeOf 7

        public ulong CountBytes()
        {
            return (ulong)((!string.IsNullOrEmpty(name) ? 2*name.Length : 0) + (unused != null ? 4*unused.Length : 0) + 168);
        }

        public void Dispose()
        {
            unused = null;
        }
    }
}