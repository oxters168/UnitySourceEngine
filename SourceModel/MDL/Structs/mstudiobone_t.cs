using UnityEngine;

namespace UnitySourceEngine
{
    public class mstudiobone_t
    {
        public string name;
        public string theSurfacePropName;

        public int nameOffset;
        public int parentBoneIndex;
        public int[] boneControllerIndex;

        public Vector3 position;
        public Quaternion quat;

        public Vector3 rotation;
        public Vector3 positionScale;
        public Vector3 rotationScale;

        public Vector3 poseToBoneColumn0;
        public Vector3 poseToBoneColumn1;
        public Vector3 poseToBoneColumn2;
        public Vector3 poseToBoneColumn3;

        public Quaternion qAlignment;

        public int flags;

        public int proceduralRuleType;
        public int proceduralRuleOffset;
        public int physicsBoneIndex;
        public int surfacePropNameOffset;
        public int contents;

        public void Dispose()
        {
            boneControllerIndex = null;
        }
    }
}