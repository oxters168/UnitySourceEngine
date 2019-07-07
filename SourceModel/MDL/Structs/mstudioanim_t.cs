using UnityEngine;

public class mstudioanim_t
{
    public byte boneIndex;
    public byte flags;
    public short nextSourceMdlAnimationOffset;

    public mstudioanim_valueptr_t theRotV;
    public mstudioanim_valueptr_t thePosV;
    public Quaternion theRot48bits; //SourceQuaternion48bits
    public Quaternion theRot64bits; //SourceQuaternion64bits
    public Vector3 thePos; //SourceVector48bits
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