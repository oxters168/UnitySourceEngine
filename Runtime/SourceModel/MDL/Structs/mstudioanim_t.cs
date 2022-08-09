using UnityEngine;

namespace UnitySourceEngine
{
    public struct mstudioanim_t //84
    {
        public byte boneIndex; //1
        public byte flags; //1
        public short nextSourceMdlAnimationOffset; //2

        public mstudioanim_valueptr_t theRotV; //18
        public mstudioanim_valueptr_t thePosV; //18
        public Quaternion theRot48bits; //SourceQuaternion48bits //16
        public Quaternion theRot64bits; //SourceQuaternion64bits //16
        public Vector3 thePos; //SourceVector48bits //12
    }

    public enum mstudioanim_flags
    {
        STUDIO_ANIM_RAWPOS = 0x01,
        STUDIO_ANIM_RAWROT = 0x02,
        STUDIO_ANIM_ANIMPOS = 0x04,
        STUDIO_ANIM_ANIMROT = 0x08,
        STUDIO_ANIM_DELTA = 0x10,
        STUDIO_ANIM_RAWROT2 = 0x10,
    }
}