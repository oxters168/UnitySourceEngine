namespace UnitySourceEngine
{
    public struct mstudio_frame_anim_t
    {
        public int constantsOffset; //4
        public int frameOffset; //4
        public int frameLength; //4
        public int[] unused; //SizeOf 3

        public byte[] theBoneFlags;
        public BoneConstantInfo[] theBoneConstantInfos; //28*length
        public BoneFrameDataInfo[,] theBoneFrameDataInfo; //40*getlength(0)*getlength(1)

        public ulong CountBytes()
        {
            return (ulong)((unused != null ? 4*unused.Length : 0) + (theBoneFlags != null ? theBoneFlags.Length : 0) + (theBoneConstantInfos != null ? 28*theBoneConstantInfos.Length : 0) + (theBoneFrameDataInfo != null ? 40*theBoneFrameDataInfo.GetLength(0)*theBoneFrameDataInfo.GetLength(1) : 0) + 12);
        }

        public void Dispose()
        {
            unused = null;
            theBoneFlags = null;
            theBoneConstantInfos = null;
            theBoneFrameDataInfo = null;
        }
    }

    public enum mstudio_fram_anim_flags
    {
        STUDIO_FRAME_RAWPOS = 0x01,
        STUDIO_FRAME_RAWROT = 0x02,
        STUDIO_FRAME_ANIMPOS = 0x04,
        STUDIO_FRAME_ANIMROT = 0x08,
        STUDIO_FRAME_FULLANIMPOS = 0x10,
    }
}