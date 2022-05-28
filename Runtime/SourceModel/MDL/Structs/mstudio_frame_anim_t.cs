namespace UnitySourceEngine
{
    public class mstudio_frame_anim_t
    {
        public int constantsOffset;
        public int frameOffset;
        public int frameLength;
        public int[] unused; //SizeOf 3

        public byte[] theBoneFlags;
        public BoneConstantInfo[] theBoneConstantInfos;
        public BoneFrameDataInfo[,] theBoneFrameDataInfo;

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