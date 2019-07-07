using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

public struct dnode_t
{
    public int planenum; // index into plane array
    public int[] children; // negative numbers are -(leafs + 1), not nodes
    public short[] mins; // for frustum culling
    public short[] maxs;
    public ushort firstface; // index into face array
    public ushort numfaces; // counting both sides
    public short area; // If all leaves below this node are in the same area, then this is the area index. If not, this is -1.
    public short padding;	// pad to 32 bytes length
}
