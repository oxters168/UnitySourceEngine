using UnityEngine;
using System;

namespace UnitySourceEngine
{
    public class mstudioattachment_t
    {
        public char[] builtName;
        public int type;
        public int bone;
        public Vector3 attachmentPoint;
        public Vector3[] vectors; //SizeOf 3

        public string name { get { return builtName != null ? new String(builtName) : name != null ? name : ""; } set { } }
        public int nameOffset;
        public int flags;
        public int localBoneIndex;
        public float localM11;
        public float localM12;
        public float localM13;
        public float localM14;
        public float localM21;
        public float localM22;
        public float localM23;
        public float localM24;
        public float localM31;
        public float localM32;
        public float localM33;
        public float localM34;
        public int[] unused; //SizeOf 8

        public void Dispose()
        {
            builtName = null;
            vectors = null;
            unused = null;
        }
    }
}