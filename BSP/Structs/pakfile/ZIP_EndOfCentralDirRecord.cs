public struct ZIP_EndOfCentralDirRecord
{
    public const int BYTE_LENGTH = 22;

    public uint signature; // 4 bytes PK56
    public ushort numberOfThisDisk;  // 2 bytes
    public ushort numberOfTheDiskWithStartOfCentralDirectory; // 2 bytes
    public ushort nCentralDirectoryEntries_ThisDisk;   // 2 bytes
    public ushort nCentralDirectoryEntries_Total;  // 2 bytes
    public uint centralDirectorySize; // 4 bytes
    public uint startOfCentralDirOffset; // 4 bytes
    public ushort commentLength; // 2 bytes
    // zip file comment follows
}
