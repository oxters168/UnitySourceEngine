namespace UnitySourceEngine
{
    // this structure is in <mod folder>/src/public/optimize.h
    public struct vtxheader_t //36 bytes
    {
        // file version as defined by OPTIMIZED_MODEL_FILE_VERSION (currently 7)
        public int version; //4

        // hardware params that affect how the model is to be optimized.
        public int vertCacheSize; //4
        public ushort maxBonesPerStrip; //2
        public ushort maxBonesPerFace; //2
        public int maxBonesPerVert; //4

        // must match checkSum in the .mdl
        public int checkSum; //4

        public int numLODs; // garymcthack - this is also specified in ModelHeader_t and should match //4

        // this is an offset to an array of 8 MaterialReplacementListHeader_t's, one of these for each LOD
        public int materialReplacementListOffset; //4

        public int numBodyParts; //4
        public int bodyPartOffset; // offset to an array of BodyPartHeader_t's //4
    }
}