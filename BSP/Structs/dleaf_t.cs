using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

public struct dleaf_t
{
    public int contents; // OR of all brushes (not needed?)
	public short cluster; // cluster this leaf is in
	public short area; // area this leaf is in
	public short flags; // flags
	public short[] mins; // for frustum culling
	public short[] maxs;
	public ushort firstleafface; // index into leaffaces
	public ushort numleaffaces;
	public ushort firstleafbrush; // index into leafbrushes
	public ushort numleafbrushes;
	public short leafWaterDataID; // -1 for not in water
 
	//!!! NOTE: for maps of version 19 or lower uncomment this block
	/*
	CompressedLightCube	ambientLighting;	// Precaculated light info for entities.
	short			padding;		// padding to 4-byte boundary
	*/
}
