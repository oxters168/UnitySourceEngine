using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

public struct dbrushside_t //lump 19 : 8 bytes
{
    public ushort planenum; // facing out of the leaf
    public short texinfo; // texture info
    public short dispinfo; // displacement info
    public short bevel; // is the side a bevel plane?
}
