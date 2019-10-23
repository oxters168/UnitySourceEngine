using UnityEngine;

namespace UnitySourceEngine
{
    public struct StaticPropLump_t
    {
        // v4
        public Vector3 Origin; // origin
        public QAngle Angles; // orientation (pitch roll yaw)
        public ushort PropType; // index into model name dictionary
        public ushort FirstLeaf; // index into leaf array
        public ushort LeafCount;
        public byte Solid; // solidity type
        public byte Flags;
        public int Skin; // model skin numbers
        public float FadeMinDist;
        public float FadeMaxDist;
        public Vector3 LightingOrigin; // for lighting
        // since v5
        public float ForcedFadeScale; // fade distance scale
        // v6 and v7 only
        public ushort MinDXLevel; // minimum DirectX version to be visible
        public ushort MaxDXLevel; // maximum DirectX version to be visible
        // since v8
        public byte MinCPULevel;
        public byte MaxCPULevel;
        public byte MinGPULevel;
        public byte MaxGPULevel;
        // since v7
        public Color32 DiffuseModulation; // per instance color and alpha modulation
        // v9 and v10 only
        public bool DisableX360; // if true, don't show on XBox 360 (4-bytes long)
        // since v10
        public uint FlagsEx; // Further bitflags.
        // since v11
        public float UniformScale; // Prop scale

        public override string ToString()
        {
            string output = "StaticPropInfo";
            output += "\n_Origin(" + Origin + ")";
            output += "\n_Angle(" + Angles + ")";
            output += "\n_Prop (pe: " + PropType + ")";
            output += "\n_First (af: " + FirstLeaf + ")";
            output += "\n_Leaf (unt: " + LeafCount + ")";
            output += "\n_Solid(" + Solid + ")";
            output += "\n_Flags(" + Flags + ")";
            output += "\n_Skin(" + Skin + ")";
            output += "\n_FadeMinDist(" + FadeMinDist + ")";
            output += "\n_FadeMaxDist(" + FadeMaxDist + ")";
            output += "\n_LightingOrigin(" + LightingOrigin + ")";
            output += "\n_ForcedFadeScale(" + ForcedFadeScale + ")";
            output += "\n_MinDXLevel(" + MinDXLevel + ")";
            output += "\n_MaxDXLevel(" + MaxDXLevel + ")";
            output += "\n_MinCPULevel(" + MinCPULevel + ")";
            output += "\n_MaxCPULevel(" + MaxCPULevel + ")";
            output += "\n_MinGPULevel(" + MinGPULevel + ")";
            output += "\n_MaxGPULevel(" + MaxGPULevel + ")";
            output += "\n_DiffuseModulation(" + DiffuseModulation + ")";
            //output += " Unknown: " + unknown + ")";
            output += "\n_DisableX360(" + DisableX360 + ")";

            return output;
        }
    }
}