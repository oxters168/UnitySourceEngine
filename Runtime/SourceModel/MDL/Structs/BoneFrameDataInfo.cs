using UnityEngine;

namespace UnitySourceEngine
{
    public struct BoneFrameDataInfo
    {
        public Vector3 theAnimPosition; //SourceVector48bits
        public Quaternion theAnimRotation; //SourceQuaternion48bits
        public Vector3 theFullAnimPosition; //SourceVector
    }
}