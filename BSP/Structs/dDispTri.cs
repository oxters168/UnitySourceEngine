using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

public struct dDispTri //lump 48
{
    public ushort Tags; //Displacement triangle tags.
}

[Flags]
public enum DispTriangleFlags
{
    DISPTRI_TAG_SURFACE = 0x1,
    DISPTRI_TAG_WALKABLE = 0x2,
    DISPTRI_TAG_BUILDABLE = 0x4,
    DISPTRI_FLAG_SURFPROP1 = 0x8,
    DISPTRI_FLAG_SURFPROP2 = 0x10,
}
