using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

public struct dbrush_t //lump 18 : 12 bytes
{
    public int firstside;	// first brushside
    public int numsides;	// number of brushsides
    public int contents;	// contents flags
}

[Flags]
public enum contentflags
{
    CONTENTS_EMPTY = 0,  //No contents
    CONTENTS_SOLID = 0x1,  //an eye is never valid in a solid
    CONTENTS_WINDOW = 0x2,  //translucent, but not watery (glass)
    CONTENTS_AUX = 0x4,
    CONTENTS_GRATE = 0x8,  //alpha-tested "grate" textures. Bullets/sight pass through, but solids don't
    CONTENTS_SLIME = 0x10,
    CONTENTS_WATER = 0x20,
    CONTENTS_MIST = 0x40,
    CONTENTS_OPAQUE = 0x80,  //block AI line of sight
    CONTENTS_TESTFOGVOLUME = 0x100,  //things that cannot be seen through (may be non-solid though)
    CONTENTS_UNUSED = 0x200,  //unused
    CONTENTS_UNUSED6 = 0x400,  //unused
    CONTENTS_TEAM1 = 0x800,  //per team contents used to differentiate collisions between players and objects on different teams
    CONTENTS_TEAM2 = 0x1000,
    CONTENTS_IGNORE_NODRAW_OPAQUE = 0x2000,  //ignore CONTENTS_OPAQUE on surfaces that have SURF_NODRAW
    CONTENTS_MOVEABLE = 0x4000,  //hits entities which are MOVETYPE_PUSH (doors, plats, etc.)
    CONTENTS_AREAPORTAL = 0x8000,  //remaining contents are non-visible, and don't eat brushes
    CONTENTS_PLAYERCLIP = 0x10000,
    CONTENTS_MONSTERCLIP = 0x20000,
    CONTENTS_CURRENT_0 = 0x40000,  //currents can be added to any other contents, and may be mixed
    CONTENTS_CURRENT_90 = 0x80000,
    CONTENTS_CURRENT_180 = 0x100000,
    CONTENTS_CURRENT_270 = 0x200000,
    CONTENTS_CURRENT_UP = 0x400000,
    CONTENTS_CURRENT_DOWN = 0x800000,
    CONTENTS_ORIGIN = 0x1000000,  //removed before bsping an entity
    CONTENTS_MONSTER = 0x2000000, //should never be on a brush, only in game
    CONTENTS_DEBRIS = 0x4000000,
    CONTENTS_DETAIL = 0x8000000,  //brushes to be added after vis leafs
    CONTENTS_TRANSLUCENT = 0x10000000,  //auto set if any surface has trans
    CONTENTS_LADDER = 0x20000000,
    CONTENTS_HITBOX = 0x40000000,  //use accurate hitboxes on trace
}