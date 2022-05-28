using UnityEngine;

namespace UnitySourceEngine
{
    public struct ddispinfo_t //lump 26 : 176 bytes : 86 bytes
    {
        public Vector3 startPosition; //Start position used for orientation.
        public int DispVertStart; //Index into LUMP_DISP_VERTS.
        public int DispTriStart; //Index into LUMP_DISP_TRIS.
        public int power; //power - Indicates size of surface (2^power   1).
        public int minTess; //Minimum tesselation allowed.
        public float smoothingAngle; //Lighting smoothing angle.
        public int contents; //Surface contents.
        public ushort MapFace; //which map face this displacement comes from.
        public int LightmapAlphaStart; //Index into ddisplightmapalpha.
        public int LightmapSamplePositionStart; //Index into LUMP_DISP_LIGHTMAP_POSITIONS.
                                                //public CDispNeighbor[] EdgeNeighbors; //Indexed by NEIGHBOREDGE_ defines.
                                                //public CDispCornerNeighbors[] CornerNeighbors; //Indexed by CORNER_ defines.
        public uint[] AllowedVerts; //Active vertices.

        public void Dispose()
        {
            AllowedVerts = null;
        }
    }
}