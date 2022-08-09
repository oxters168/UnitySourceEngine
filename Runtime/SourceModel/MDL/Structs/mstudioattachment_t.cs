using UnityEngine;
using System;

namespace UnitySourceEngine
{
    public struct mstudioattachment_t
    {
        public int type; //4
        public int bone; //4
        public Vector3 attachmentPoint; //12
        public Vector3[] vectors; //SizeOf 3
        public string name;
        public int nameOffset; //4
        public int flags; //4
        public int localBoneIndex; //4
        public float localM11; //4
        public float localM12; //4
        public float localM13; //4
        public float localM14; //4
        public float localM21; //4
        public float localM22; //4
        public float localM23; //4
        public float localM24; //4
        public float localM31; //4
        public float localM32; //4
        public float localM33; //4
        public float localM34; //4
        public int[] unused; //SizeOf 8

        public ulong CountBytes()
        {
            return (ulong)((vectors != null ? 12*vectors.Length : 0) + (!string.IsNullOrEmpty(name) ? 2*name.Length : 0) + (unused != null ? 4*unused.Length : 0) + 80);
        }

        public void Dispose()
        {
            name = null;
            vectors = null;
            unused = null;
        }
    }
}