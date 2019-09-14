namespace UnitySourceEngine
{
    class VPKDirectoryEntry
    {
        public uint CRC; // A 32bit CRC of the file's data.
        public ushort PreloadBytes; // The number of bytes contained in the index file.

        // A zero based index of the archive this file's data is contained in.
        // If 0x7fff, the data follows the directory.
        public ushort ArchiveIndex;

        // If ArchiveIndex is 0x7fff, the offset of the file data relative to the end of the directory (see the header for more details).
        // Otherwise, the offset of the data from the start of the specified archive.
        public uint EntryOffset;

        // If zero, the entire file is stored in the preload data.
        // Otherwise, the number of bytes stored starting at EntryOffset.
        public uint EntryLength;

        public const ushort Terminator = 0xffff;

        //public byte[] PreloadData;

        public void Dispose()
        {
            //PreloadData = null;
        }
    }
}