public struct ZIP_FileHeader
{
    public uint signature; //  4 bytes PK12 
    public ushort versionMadeBy; // version made by 2 bytes 
    public ushort versionNeededToExtract; // version needed to extract 2 bytes 
    public ushort flags; // general purpose bit flag 2 bytes 
    public ushort compressionMethod; // compression method 2 bytes 
    public ushort lastModifiedTime; // last mod file time 2 bytes 
    public ushort lastModifiedDate; // last mod file date 2 bytes 
    public uint crc32; // crc-32 4 bytes 
    public uint compressedSize; // compressed size 4 bytes 
    public uint uncompressedSize; // uncompressed size 4 bytes 
    public ushort fileNameLength; // file name length 2 bytes 
    public ushort extraFieldLength; // extra field length 2 bytes 
    public ushort fileCommentLength; // file comment length 2 bytes 
    public ushort diskNumberStart; // disk number start 2 bytes 
    public ushort internalFileAttribs; // internal file attributes 2 bytes 
    public uint externalFileAttribs; // external file attributes 4 bytes 
    public uint relativeOffsetOfLocalHeader; // relative offset of local header 4 bytes
    // file name (variable size) 
    // extra field (variable size) 
    // file comment (variable size)
}
