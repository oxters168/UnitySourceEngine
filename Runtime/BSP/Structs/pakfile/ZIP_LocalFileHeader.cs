public struct ZIP_LocalFileHeader
{
    public const int BYTE_SIZE = 30;

    public uint signature; //local file header signature 4 bytes PK34 
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
    // file name (variable size) 
    // extra field (variable size)
}
