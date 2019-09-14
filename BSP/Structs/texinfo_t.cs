using System;

namespace UnitySourceEngine
{
    public struct texinfo_t
    {
        public float[][] textureVecs; // [s/t][xyz offset]
        public float[][] lightmapVecs; // [s/t][xyz offset] - length is in units of texels/area
        public int flags; // miptex flags	overrides
        public int texdata; // Pointer to texture name, size, etc.

        public void Dispose()
        {
            textureVecs = null;
            lightmapVecs = null;
        }
    }

    [Flags]
    public enum texflags
    {
        SURF_LIGHT = 0x1,  //value will hold the light strength
        SURF_SKY2D = 0x2,  //don't draw, indicates we should skylight + draw 2d sky but not draw the 3D skybox
        SURF_SKY = 0x4,  //don't draw, but add to skybox
        SURF_WARP = 0x8,  //turbulent water warp
        SURF_TRANS = 0x10,
        SURF_NOPORTAL = 0x20,  //the surface can not have a portal placed on it
        SURF_TRIGGER = 0x40,  //FIXME: This is an xbox hack to work around elimination of trigger surfaces, which breaks occluders
        SURF_NODRAW = 0x80,  //don't bother referencing the texture
        SURF_HINT = 0x100,  //make a primary bsp splitter
        SURF_SKIP = 0x200,  //completely ignore, allowing non-closed brushes
        SURF_NOLIGHT = 0x400,  //Don't calculate light
        SURF_BUMPLIGHT = 0x800,  //calculate three lightmaps for the surface for bumpmapping
        SURF_NOSHADOWS = 0x1000,  //Don't receive shadows
        SURF_NODECALS = 0x2000,  //Don't receive decals
        SURF_NOCHOP = 0x4000,  //Don't subdivide patches on this surface
        SURF_HITBOX = 0x8000,  //surface is part of a hitbox
    }
}