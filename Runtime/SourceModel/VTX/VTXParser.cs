using System.IO;
using System;

namespace UnitySourceEngine
{
    public class VTXParser : IDisposable
    {
        // public vtxheader_t header;
        // public SourceVtxBodyPart[] bodyParts = new SourceVtxBodyPart[0];

        private long fileBeginOffset;

        public VTXParser()
        {
        }

        // Dispose() calls Dispose(true)
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        // NOTE: Leave out the finalizer altogether if this class doesn't
        // own unmanaged resources, but leave the other methods
        // exactly as they are.
        ~VTXParser()
        {
            // Finalizer calls Dispose(false)
            Dispose(false);
        }
        protected virtual void Dispose(bool disposing)
        {
            // if (disposing)
            // {
            //     if (bodyParts != null)
            //         foreach (SourceVtxBodyPart bodyPart in bodyParts)
            //             bodyPart?.Dispose();
            //     bodyParts = null;
            // }
        }

        // public void ParseHeader(Stream stream, long fileOffset = 0)
        // {
        //     fileOffsetPosition = fileOffset;

        //     ReadSourceVtxHeader(stream);
        // }
        public VTXData Parse(Stream stream, long fileOffset = 0)
        {
            fileBeginOffset = fileOffset;

            var vtxData = new VTXData();

            vtxData.header = ReadSourceVtxHeader(stream, fileBeginOffset);
            vtxData.bodyParts = ReadSourceVtxBodyParts(stream, fileBeginOffset, vtxData.header);

            return vtxData;
        }
        private static vtxheader_t ReadSourceVtxHeader(Stream stream, long fileBeginOffset)
        {
            var header = new vtxheader_t();

            stream.Position = fileBeginOffset;

            header.version = DataParser.ReadInt(stream);
            header.vertCacheSize = DataParser.ReadInt(stream);
            header.maxBonesPerStrip = DataParser.ReadUShort(stream);
            header.maxBonesPerFace = DataParser.ReadUShort(stream);
            header.maxBonesPerVert = DataParser.ReadInt(stream);
            header.checkSum = DataParser.ReadInt(stream);
            header.numLODs = DataParser.ReadInt(stream);
            header.materialReplacementListOffset = DataParser.ReadInt(stream);
            header.numBodyParts = DataParser.ReadInt(stream);
            header.bodyPartOffset = DataParser.ReadInt(stream);

            return header;
        }

        private static SourceVtxBodyPart[] ReadSourceVtxBodyParts(Stream stream, long fileBeginOffset, vtxheader_t header)
        {
            SourceVtxBodyPart[] bodyParts = null;
            if (header.numBodyParts >= 0)
            {
                bodyParts = new SourceVtxBodyPart[header.numBodyParts];
                if (header.version <= 7)
                {
                    long[] bodyPartOffsets = new long[header.numBodyParts];

                    stream.Position = fileBeginOffset + header.bodyPartOffset;
                    for (int i = 0; i < bodyParts.Length; i++)
                    {
                        bodyPartOffsets[i] = stream.Position;

                        bodyParts[i] = new SourceVtxBodyPart();
                        bodyParts[i].modelCount = DataParser.ReadInt(stream);
                        bodyParts[i].modelOffset = DataParser.ReadInt(stream);
                    }
                    for (int i = 0; i < bodyParts.Length; i++) //To avoid issues with positioning of stream
                        bodyParts[i].vtxModels = ReadSourceVtxModels(stream, bodyPartOffsets[i], bodyParts[i].modelOffset, bodyParts[i].modelCount);
                }
            }
            return bodyParts;
        }
        private static SourceVtxModel[] ReadSourceVtxModels(Stream stream, long bodyPartOffset, int modelOffset, int modelCount)
        {
            SourceVtxModel[] vtxModels = null;
            if (modelCount >= 0)
            {
                vtxModels = new SourceVtxModel[modelCount];
                if (modelOffset != 0)
                {
                    long[] modelOffsets = new long[modelCount];
                    stream.Position = bodyPartOffset + modelOffset;

                    for (int i = 0; i < vtxModels.Length; i++)
                    {
                        modelOffsets[i] = stream.Position;
                        vtxModels[i] = new SourceVtxModel();
                        vtxModels[i].lodCount = DataParser.ReadInt(stream);
                        vtxModels[i].lodOffset = DataParser.ReadInt(stream);
                    }
                    for (int i = 0; i < vtxModels.Length; i++) //To avoid issues with positioning of stream
                        vtxModels[i].vtxModelLods = ReadSourceVtxModelLods(stream, modelOffsets[i], vtxModels[i].lodOffset, vtxModels[i].lodCount);
                }
            }
            return vtxModels;
        }
        private static SourceVtxModelLod[] ReadSourceVtxModelLods(Stream stream, long modelOffset, int lodOffset, int lodCount)
        {
            SourceVtxModelLod[] vtxModelLods = null;
            if (lodCount >= 0)
            {
                vtxModelLods = new SourceVtxModelLod[lodCount];
                if (lodOffset != 0)
                {
                    long[] modelLodOffsets = new long[lodCount];
                    stream.Position = modelOffset + lodOffset;

                    for (int i = 0; i < vtxModelLods.Length; i++)
                    {
                        modelLodOffsets[i] = stream.Position;
                        vtxModelLods[i] = new SourceVtxModelLod();
                        vtxModelLods[i].meshCount = DataParser.ReadInt(stream);
                        vtxModelLods[i].meshOffset = DataParser.ReadInt(stream);
                        vtxModelLods[i].switchPoint = DataParser.ReadFloat(stream);
                    }
                    for (int i = 0; i < vtxModelLods.Length; i++) //To avoid issues with positioning of stream
                        vtxModelLods[i].vtxMeshes = ReadSourceVtxMeshes(stream, modelLodOffsets[i], vtxModelLods[i].meshOffset, vtxModelLods[i].meshCount);
                }
            }
            return vtxModelLods;
        }
        private static SourceVtxMesh[] ReadSourceVtxMeshes(Stream stream, long modelLodOffset, int meshOffset, int meshCount)
        {
            SourceVtxMesh[] vtxMeshes = null;
            if (meshCount >= 0)
            {
                vtxMeshes = new SourceVtxMesh[meshCount];
                if (meshOffset != 0)
                {
                    long[] meshOffsets = new long[meshCount];
                    stream.Position = modelLodOffset + meshOffset;
                    for (int i = 0; i < vtxMeshes.Length; i++)
                    {
                        meshOffsets[i] = stream.Position;
                        vtxMeshes[i] = new SourceVtxMesh();
                        vtxMeshes[i].stripGroupCount = DataParser.ReadInt(stream);
                        vtxMeshes[i].stripGroupOffset = DataParser.ReadInt(stream);
                        vtxMeshes[i].flags = DataParser.ReadByte(stream);
                    }
                    for (int i = 0; i < vtxMeshes.Length; i++) //To avoid issues with positioning of stream
                        vtxMeshes[i].vtxStripGroups = ReadSourceVtxStripGroups(stream, meshOffsets[i], vtxMeshes[i].stripGroupOffset, vtxMeshes[i].stripGroupCount);
                }
            }
            return vtxMeshes;
        }
        private static SourceVtxStripGroup[] ReadSourceVtxStripGroups(Stream stream, long meshOffset, int stripGroupOffset, int stripGroupCount)
        {
            SourceVtxStripGroup[] vtxStripGroups = null;
            if (stripGroupCount >= 0)
            {
                vtxStripGroups = new SourceVtxStripGroup[stripGroupCount];
                if (stripGroupOffset != 0)
                {
                    long[] stripGroupOffsets = new long[stripGroupCount];
                    stream.Position = meshOffset + stripGroupOffset;
                    for (int i = 0; i < vtxStripGroups.Length; i++)
                    {
                        stripGroupOffsets[i] = stream.Position;
                        vtxStripGroups[i] = new SourceVtxStripGroup();
                        vtxStripGroups[i].vertexCount = DataParser.ReadInt(stream);
                        vtxStripGroups[i].vertexOffset = DataParser.ReadInt(stream);
                        vtxStripGroups[i].indexCount = DataParser.ReadInt(stream);
                        vtxStripGroups[i].indexOffset = DataParser.ReadInt(stream);
                        vtxStripGroups[i].stripCount = DataParser.ReadInt(stream);
                        vtxStripGroups[i].stripOffset = DataParser.ReadInt(stream);
                        vtxStripGroups[i].flags = DataParser.ReadByte(stream);
                    }
                    for (int i = 0; i < vtxStripGroups.Length; i++) //To avoid issues with positioning of stream
                    {
                        vtxStripGroups[i].vtxVertices = ReadSourceVtxVertices(stream, stripGroupOffsets[i], vtxStripGroups[i].vertexOffset, vtxStripGroups[i].vertexCount);
                        vtxStripGroups[i].vtxIndices = ReadSourceVtxIndices(stream, stripGroupOffsets[i], vtxStripGroups[i].indexOffset, vtxStripGroups[i].indexCount);
                        vtxStripGroups[i].vtxStrips = ReadSourceVtxStrips(stream, stripGroupOffsets[i], vtxStripGroups[i].stripOffset, vtxStripGroups[i].stripCount);
                    }
                }
            }
            return vtxStripGroups;
        }
        private static SourceVtxVertex[] ReadSourceVtxVertices(Stream stream, long stripGroupOffset, int vertexOffset, int vertexCount)
        {
            SourceVtxVertex[] vtxVertices = null;
            if (vertexCount >= 0)
            {
                vtxVertices = new SourceVtxVertex[vertexCount];
                if (vertexOffset != 0)
                {
                    stream.Position = stripGroupOffset + vertexOffset;
                    for (int i = 0; i < vtxVertices.Length; i++)
                    {
                        vtxVertices[i] = new SourceVtxVertex();
                        vtxVertices[i].boneWeightIndex = new byte[VVDData.MAX_NUM_BONES_PER_VERT];
                        for (int j = 0; j < vtxVertices[i].boneWeightIndex.Length; j++)
                        {
                            vtxVertices[i].boneWeightIndex[j] = DataParser.ReadByte(stream);
                        }

                        vtxVertices[i].boneCount = DataParser.ReadByte(stream);
                        vtxVertices[i].originalMeshVertexIndex = DataParser.ReadUShort(stream);

                        vtxVertices[i].boneId = new byte[VVDData.MAX_NUM_BONES_PER_VERT];
                        for (int j = 0; j < vtxVertices[i].boneId.Length; j++)
                        {
                            vtxVertices[i].boneId[j] = DataParser.ReadByte(stream);
                        }
                    }
                }
            }
            return vtxVertices;
        }
        private static ushort[] ReadSourceVtxIndices(Stream stream, long stripGroupOffset, int indexOffset, int indexCount)
        {
            ushort[] vtxIndices = null;
            if (indexCount >= 0)
            {
                vtxIndices = new ushort[indexCount];
                if (indexOffset != 0)
                {
                    stream.Position = stripGroupOffset + indexOffset;
                    for (int i = 0; i < vtxIndices.Length; i++)
                        vtxIndices[i] = DataParser.ReadUShort(stream);
                }
            }
            return vtxIndices;
        }
        private static SourceVtxStrip[] ReadSourceVtxStrips(Stream stream, long stripGroupOffset, int stripOffset, int stripCount)
        {
            SourceVtxStrip[] vtxStrips = null;
            if (stripCount >= 0)
            {
                vtxStrips = new SourceVtxStrip[stripCount];
                if (stripOffset != 0)
                {
                    stream.Position = stripGroupOffset + stripOffset;
                    for (int i = 0; i < vtxStrips.Length; i++)
                    {
                        vtxStrips[i] = new SourceVtxStrip();
                        vtxStrips[i].indexCount = DataParser.ReadInt(stream);
                        vtxStrips[i].indexMeshIndex = DataParser.ReadInt(stream);
                        vtxStrips[i].vertexCount = DataParser.ReadInt(stream);
                        vtxStrips[i].vertexMeshIndex = DataParser.ReadInt(stream);
                        vtxStrips[i].boneCount = DataParser.ReadShort(stream);
                        vtxStrips[i].flags = DataParser.ReadByte(stream);
                        vtxStrips[i].boneStateChangeCount = DataParser.ReadInt(stream);
                        vtxStrips[i].boneStateChangeOffset = DataParser.ReadInt(stream);
                        //Missing parsing of bone state changes (not sure what they are yet though)
                    }
                }
            }
            return vtxStrips;
        }
    }
}