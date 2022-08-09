using UnityEngine;

namespace UnitySourceEngine
{
    public struct mstudiobone_t //160+(2*n.length)+(2*tspn.length)+(4*bci.length) bytes
    {
        public string name; //2*name.length bytes
        public string theSurfacePropName; //2*theSurfacePropName.length bytes

        public int nameOffset; //4 bytes
        public int parentBoneIndex; //4 bytes
        public int[] boneControllerIndex; //4*boneControllerIndex.length bytes

        public Vector3 position; //12 bytes
        public Quaternion quat; //16 bytes

        public Vector3 rotation; //12 bytes
        public Vector3 positionScale; //12 bytes
        public Vector3 rotationScale; //12 bytes

        public Vector3 poseToBoneColumn0; //12 bytes
        public Vector3 poseToBoneColumn1; //12 bytes
        public Vector3 poseToBoneColumn2; //12 bytes
        public Vector3 poseToBoneColumn3; //12 bytes

        public Quaternion qAlignment; //16 bytes

        public int flags; //4 bytes

        public int proceduralRuleType; //4 bytes
        public int proceduralRuleOffset; //4 bytes
        public int physicsBoneIndex; //4 bytes
        public int surfacePropNameOffset; //4 bytes
        public int contents; //4 bytes

        public ulong CountBytes()
        {
            return (ulong)(160+(2*name.Length)+(2*theSurfacePropName.Length)+(4*boneControllerIndex.Length));
        }

        public void Dispose()
        {
            boneControllerIndex = null;
        }

        public override string ToString()
        {
            string output = string.Empty;
            output += "_name(" + name + ")";
            output += "\n_theSurfacePropName(" + theSurfacePropName + ")";
            output += "\n_nameOffset(" + nameOffset + ")";
            output += "\n_parentBoneIndex(" + parentBoneIndex + ")";
            output += "\n_boneControllerIndex[" + (boneControllerIndex != null ? boneControllerIndex.Length.ToString() : "null") + "]:";
            if (boneControllerIndex != null)
                for (int i = 0; i < boneControllerIndex.Length; i++)
                    output += "\n    [" + i + "]: " + boneControllerIndex[i];
            output += "\n_position(" + position + ")";
            output += "\n_quat(" + quat + ")";
            output += "\n_rotation(" + rotation + ")";
            output += "\n_positionScale(" + positionScale + ")";
            output += "\n_rotationScale(" + rotationScale + ")";
            output += "\n_poseToBoneColumn0(" + poseToBoneColumn0 + ")";
            output += "\n_poseToBoneColumn1(" + poseToBoneColumn1 + ")";
            output += "\n_poseToBoneColumn2(" + poseToBoneColumn2 + ")";
            output += "\n_poseToBoneColumn3(" + poseToBoneColumn3 + ")";
            output += "\n_qAlignment(" + qAlignment + ")";
            output += "\n_flags(" + flags + ")";
            output += "\n_proceduralRuleType(" + proceduralRuleType + ")";
            output += "\n_proceduralRuleOffset(" + proceduralRuleOffset + ")";
            output += "\n_physicsBoneIndex(" + physicsBoneIndex + ")";
            output += "\n_surfacePropNameOffset(" + surfacePropNameOffset + ")";
            output += "\n_contents(" + contents + ")";
            return output;
        }
    }
}