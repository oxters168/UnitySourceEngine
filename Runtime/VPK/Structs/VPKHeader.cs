namespace UnitySourceEngine
{
    class VPKHeader
    {
        public const uint Signature = 0x55aa1234;
        public uint Version;

        // The size, in bytes, of the directory tree
        public uint TreeSize;

        // How many bytes of file content are stored in this VPK file (0 in CSGO)
        public uint FileDataSectionSize;

        // The size, in bytes, of the section containing MD5 checksums for external archive content
        public uint ArchiveMD5SectionSize;

        // The size, in bytes, of the section containing MD5 checksums for content in this file (should always be 48)
        public uint OtherMD5SectionSize;

        // The size, in bytes, of the section containing the public key and signature. This is either 0 (CSGO & The Ship) or 296 (HL2, HL2:DM, HL2:EP1, HL2:EP2, HL2:LC, TF2, DOD:S & CS:S)
        public uint SignatureSectionSize;
    }
}