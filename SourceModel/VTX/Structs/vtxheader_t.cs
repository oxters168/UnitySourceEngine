// this structure is in <mod folder>/src/public/optimize.h
public struct vtxheader_t
{
    // file version as defined by OPTIMIZED_MODEL_FILE_VERSION (currently 7)
    public int version;

    // hardware params that affect how the model is to be optimized.
    public int vertCacheSize;
    public ushort maxBonesPerStrip;
    public ushort maxBonesPerFace;
    public int maxBonesPerVert;

    // must match checkSum in the .mdl
    public long checkSum;

    public int numLODs; // garymcthack - this is also specified in ModelHeader_t and should match

    // this is an offset to an array of 8 MaterialReplacementListHeader_t's, one of these for each LOD
    public int materialReplacementListOffset;

    public int numBodyParts;
    public int bodyPartOffset; // offset to an array of BodyPartHeader_t's
}
