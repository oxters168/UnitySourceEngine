﻿namespace UnitySourceEngine
{
    public struct lump_t
    {
        public int fileofs; // offset into file (bytes)
        public int filelen; // length of lump (bytes)
        public int version; // lump format version
        public int fourCC;  // lump ident code
    }
}