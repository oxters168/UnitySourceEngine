using UnityEngine;

namespace UnitySourceEngine
{
    public class mstudioikrule_t
    {
        public string name;

        public int index;
        public int type;
        public int chain;
        public int bone;

        public int slot;
        public double height;
        public double radius;
        public double floor;
        public Vector3 pos;
        public Quaternion q;

        public int compressedIkErrorOffset;
        public int unused2;
        public int ikErrorIndexStart;
        public int ikErrorOffset;

        public double influenceStart;
        public double influencePeak;
        public double influenceTail;
        public double influenceEnd;

        public double unused3;
        public double contact;
        public double drop;
        public double top;

        public int unused6;
        public int unused7;
        public int unused8;

        public int attachmentNameOffset;

        public int[] unused; //SizeOf 7

        public void Dispose()
        {
            unused = null;
        }
    }
}