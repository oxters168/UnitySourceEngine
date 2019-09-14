namespace UnitySourceEngine
{
    struct VTFHeader
    {
        public const int signature = 0x00465456;      // File signature ("VTF\0"). (or as little-endian integer, 0x00465456)
        public float version;        // (Sizeof 2) version[0].version[1].
        public uint headerSize;        // Size of the header struct (16 byte aligned; currently 80 bytes).
        public ushort width;           // Width of the largest mipmap in pixels. Must be a power of 2.
        public ushort height;          // Height of the largest mipmap in pixels. Must be a power of 2.
        public uint flags;         // VTF flags.
        public ushort frames;          // Number of frames, if animated (1 for no animation).
        public ushort firstFrame;      // First frame in animation (0 based).
        public byte[] padding0;      // (Sizeof 4) reflectivity padding (16 byte alignment).
        public float[] reflectivity;  // (Sizeof 3) reflectivity vector.
        public byte[] padding1;      // (Sizeof 4) reflectivity padding (8 byte packing).
        public float bumpmapScale;     // Bumpmap scale.
        public VTFImageFormat highResImageFormat;    // High resolution image format.
        public byte mipmapCount;      // Number of mipmaps.
        public VTFImageFormat lowResImageFormat; // Low resolution image format (always DXT1).
        public byte lowResImageWidth; // Low resolution image width.
        public byte lowResImageHeight;    // Low resolution image height.

        public ushort depth;           // Depth of the largest mipmap in pixels. Must be a power of 2. Can be 0 or 1 for a 2D texture (v7.2 only).

        public byte[] padding2; // (Sizeof 3)
        public uint resourceCount; // Number of image resources

        public byte[] padding3; // (Sizeof 8)
        public VTFResource[] resources; // (Sizeof VTF_RSRC_MAX_DICTIONARY_ENTRIES)
        #pragma warning disable CS0649
        public VTFResourceData[] data; // (Sizeof VT_RSRC_MAX_DICTIONARY_ENTRIES)
        #pragma warning restore CS0649
    }

    [System.Flags]
    public enum VTFImageFormat
    {
        IMAGE_FORMAT_RGBA8888 = 0,              //!<  = Red, Green, Blue, Alpha - 32 bpp
        IMAGE_FORMAT_ABGR8888,                  //!<  = Alpha, Blue, Green, Red - 32 bpp
        IMAGE_FORMAT_RGB888,                    //!<  = Red, Green, Blue - 24 bpp
        IMAGE_FORMAT_BGR888,                    //!<  = Blue, Green, Red - 24 bpp
        IMAGE_FORMAT_RGB565,                    //!<  = Red, Green, Blue - 16 bpp
        IMAGE_FORMAT_I8,                        //!<  = Luminance - 8 bpp
        IMAGE_FORMAT_IA88,                      //!<  = Luminance, Alpha - 16 bpp
        IMAGE_FORMAT_P8,                        //!<  = Paletted - 8 bpp
        IMAGE_FORMAT_A8,                        //!<  = Alpha- 8 bpp
        IMAGE_FORMAT_RGB888_BLUESCREEN,         //!<  = Red, Green, Blue, "BlueScreen" Alpha - 24 bpp
        IMAGE_FORMAT_BGR888_BLUESCREEN,         //!<  = Red, Green, Blue, "BlueScreen" Alpha - 24 bpp
        IMAGE_FORMAT_ARGB8888,                  //!<  = Alpha, Red, Green, Blue - 32 bpp
        IMAGE_FORMAT_BGRA8888,                  //!<  = Blue, Green, Red, Alpha - 32 bpp
        IMAGE_FORMAT_DXT1,                      //!<  = DXT1 compressed format - 4 bpp
        IMAGE_FORMAT_DXT3,                      //!<  = DXT3 compressed format - 8 bpp
        IMAGE_FORMAT_DXT5,                      //!<  = DXT5 compressed format - 8 bpp
        IMAGE_FORMAT_BGRX8888,                  //!<  = Blue, Green, Red, Unused - 32 bpp
        IMAGE_FORMAT_BGR565,                    //!<  = Blue, Green, Red - 16 bpp
        IMAGE_FORMAT_BGRX5551,                  //!<  = Blue, Green, Red, Unused - 16 bpp
        IMAGE_FORMAT_BGRA4444,                  //!<  = Red, Green, Blue, Alpha - 16 bpp
        IMAGE_FORMAT_DXT1_ONEBITALPHA,          //!<  = DXT1 compressed format with 1-bit alpha - 4 bpp
        IMAGE_FORMAT_BGRA5551,                  //!<  = Blue, Green, Red, Alpha - 16 bpp
        IMAGE_FORMAT_UV88,                      //!<  = 2 channel format for DuDv/Normal maps - 16 bpp
        IMAGE_FORMAT_UVWQ8888,                  //!<  = 4 channel format for DuDv/Normal maps - 32 bpp
        IMAGE_FORMAT_RGBA16161616F,             //!<  = Red, Green, Blue, Alpha - 64 bpp
        IMAGE_FORMAT_RGBA16161616,              //!<  = Red, Green, Blue, Alpha signed with mantissa - 64 bpp
        IMAGE_FORMAT_UVLX8888,                  //!<  = 4 channel format for DuDv/Normal maps - 32 bpp
        IMAGE_FORMAT_R32F,                      //!<  = Luminance - 32 bpp
        IMAGE_FORMAT_RGB323232F,                //!<  = Red, Green, Blue - 96 bpp
        IMAGE_FORMAT_RGBA32323232F,             //!<  = Red, Green, Blue, Alpha - 128 bpp
        IMAGE_FORMAT_NV_DST16,
        IMAGE_FORMAT_NV_DST24,
        IMAGE_FORMAT_NV_INTZ,
        IMAGE_FORMAT_NV_RAWZ,
        IMAGE_FORMAT_ATI_DST16,
        IMAGE_FORMAT_ATI_DST24,
        IMAGE_FORMAT_NV_NULL,
        IMAGE_FORMAT_ATI2N,
        IMAGE_FORMAT_ATI1N,
        /*
        XBox:
        IMAGE_FORMAT_X360_DST16,
        IMAGE_FORMAT_X360_DST24,
        IMAGE_FORMAT_X360_DST24F,
        IMAGE_FORMAT_LINEAR_BGRX8888,			//!<  = Blue, Green, Red, Unused - 32 bpp		
        IMAGE_FORMAT_LINEAR_RGBA8888,			//!<  = Red, Green, Blue, Alpha - 32 bpp
        IMAGE_FORMAT_LINEAR_ABGR8888,			//!<  = Alpha, Blue, Green, Red - 32 bpp
        IMAGE_FORMAT_LINEAR_ARGB8888,			//!<  = Alpha, Red, Green, Blue - 32 bpp
        IMAGE_FORMAT_LINEAR_BGRA8888,			//!<  = Blue, Green, Red, Alpha - 32 bpp
        IMAGE_FORMAT_LINEAR_RGB888,				//!<  = Red, Green, Blue - 24 bpp
        IMAGE_FORMAT_LINEAR_BGR888,				//!<  = Blue, Green, Red - 24 bpp
        IMAGE_FORMAT_LINEAR_BGRX5551,			//!<  = Blue, Green, Red, Unused - 16 bpp
        IMAGE_FORMAT_LINEAR_I8,					//!<  = Luminance - 8 bpp
        IMAGE_FORMAT_LINEAR_RGBA16161616,		//!<  = Red, Green, Blue, Alpha signed with mantissa - 64 bpp
        IMAGE_FORMAT_LE_BGRX8888,				//!<  = Blue, Green, Red, Unused - 32 bpp
        IMAGE_FORMAT_LE_BGRA8888,				//!<  = Blue, Green, Red, Alpha - 32 bpp
        */
        IMAGE_FORMAT_COUNT,
        IMAGE_FORMAT_NONE = -1
    }
}