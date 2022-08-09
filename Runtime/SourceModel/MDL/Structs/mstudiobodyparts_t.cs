namespace UnitySourceEngine
{
    public struct mstudiobodyparts_t
    {
        public string name; //2*name.length bytes
        public int nameOffset; //4 bytes
        public int modelCount; //4 bytes
        public int theBase; //4 bytes
        public int modelOffset; //4 bytes
        public mstudiomodel_t[] models;

        public ulong CountBytes()
        {
            ulong totalBytes = (ulong)((!string.IsNullOrEmpty(name) ? 2*name.Length : 0) + 16);
            if (models != null)
                foreach(var model in models)
                    totalBytes += model.CountBytes();
            return totalBytes;
        }

        public void Dispose()
        {
            if (models != null)
                foreach (var model in models)
                    model.Dispose();
            models = null;
        }

        // public override string ToString()
        // {
        //     string output = string.Empty;
        //     output += "_name(" + name + ")";
        //     output += "\n_theSurfacePropName(" + theSurfacePropName + ")";
        //     output += "\n_nameOffset(" + nameOffset + ")";
        //     output += "\n_parentBoneIndex(" + parentBoneIndex + ")";
        //     output += "\n_boneControllerIndex[" + (boneControllerIndex != null ? boneControllerIndex.Length.ToString() : "null") + "]:";
        //     if (boneControllerIndex != null)
        //         for (int i = 0; i < boneControllerIndex.Length; i++)
        //             output += "\n    [" + i + "]: " + boneControllerIndex[i];
        //     output += "\n_position(" + position + ")";
        //     output += "\n_quat(" + quat + ")";
        //     output += "\n_rotation(" + rotation + ")";
        //     output += "\n_positionScale(" + positionScale + ")";
        //     output += "\n_rotationScale(" + rotationScale + ")";
        //     output += "\n_poseToBoneColumn0(" + poseToBoneColumn0 + ")";
        //     output += "\n_poseToBoneColumn1(" + poseToBoneColumn1 + ")";
        //     output += "\n_poseToBoneColumn2(" + poseToBoneColumn2 + ")";
        //     output += "\n_poseToBoneColumn3(" + poseToBoneColumn3 + ")";
        //     output += "\n_qAlignment(" + qAlignment + ")";
        //     output += "\n_flags(" + flags + ")";
        //     output += "\n_proceduralRuleType(" + proceduralRuleType + ")";
        //     output += "\n_proceduralRuleOffset(" + proceduralRuleOffset + ")";
        //     output += "\n_physicsBoneIndex(" + physicsBoneIndex + ")";
        //     output += "\n_surfacePropNameOffset(" + surfacePropNameOffset + ")";
        //     output += "\n_contents(" + contents + ")";
        //     return output;
        // }
    }
}