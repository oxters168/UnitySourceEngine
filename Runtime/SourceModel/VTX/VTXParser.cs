using System.IO;
using System;

namespace UnitySourceEngine
{
    public class VTXParser : IDisposable
    {
        public vtxheader_t header;
        public SourceVtxBodyPart[] bodyParts = new SourceVtxBodyPart[0];

        private long fileOffsetPosition;

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
            if (disposing)
            {
                if (bodyParts != null)
                    foreach (SourceVtxBodyPart bodyPart in bodyParts)
                        bodyPart?.Dispose();
                bodyParts = null;
            }
        }

        public void ParseHeader(Stream stream, long fileOffset = 0)
        {
            fileOffsetPosition = fileOffset;

            ReadSourceVtxHeader(stream);
        }
        public void Parse(Stream stream, long fileOffset = 0)
        {
            fileOffsetPosition = fileOffset;

            //try
            //{
                ReadSourceVtxBodyParts(stream);
            //}
            //catch (Exception e)
            //{
            //    UnityEngine.Debug.LogError("VTXParser: " + e.ToString());
            //}
        }
        private vtxheader_t ReadSourceVtxHeader(Stream stream)
        {
            header = new vtxheader_t();

            stream.Position = fileOffsetPosition;

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

        private void ReadSourceVtxBodyParts(Stream stream)
        {
            if (header.numBodyParts > 0 && header.version <= 7)
            {
                long[] bodyPartOffsets = new long[header.numBodyParts];

                stream.Position = fileOffsetPosition + header.bodyPartOffset;
                bodyParts = new SourceVtxBodyPart[header.numBodyParts];
                for (int i = 0; i < bodyParts.Length; i++)
                {
                    bodyPartOffsets[i] = stream.Position;

                    bodyParts[i] = new SourceVtxBodyPart();
                    bodyParts[i].modelCount = DataParser.ReadInt(stream);
                    bodyParts[i].modelOffset = DataParser.ReadInt(stream);
                }
                for (int i = 0; i < bodyParts.Length; i++)
                    ReadSourceVtxModels(stream, bodyPartOffsets[i], bodyParts[i]);
            }
        }
        private void ReadSourceVtxModels(Stream stream, long bodyPartOffset, SourceVtxBodyPart bodyPart)
        {
            if (bodyPart.modelCount > 0 && bodyPart.modelOffset != 0)
            {
                long[] modelOffsets = new long[bodyPart.modelCount];
                stream.Position = bodyPartOffset + bodyPart.modelOffset;
                bodyPart.theVtxModels = new SourceVtxModel[bodyPart.modelCount];

                for (int i = 0; i < bodyPart.theVtxModels.Length; i++)
                {
                    modelOffsets[i] = stream.Position;
                    bodyPart.theVtxModels[i] = new SourceVtxModel();
                    bodyPart.theVtxModels[i].lodCount = DataParser.ReadInt(stream);
                    bodyPart.theVtxModels[i].lodOffset = DataParser.ReadInt(stream);
                }
                for (int i = 0; i < bodyPart.theVtxModels.Length; i++)
                    ReadSourceVtxModelLods(stream, modelOffsets[i], bodyPart.theVtxModels[i]);
            }
        }
        private void ReadSourceVtxModelLods(Stream stream, long modelOffset, SourceVtxModel model)
        {
            if (model.lodCount > 0 && model.lodOffset != 0)
            {
                long[] modelLodOffsets = new long[model.lodCount];
                stream.Position = modelOffset + model.lodOffset;
                model.theVtxModelLods = new SourceVtxModelLod[model.lodCount];

                for (int i = 0; i < model.theVtxModelLods.Length; i++)
                {
                    modelLodOffsets[i] = stream.Position;
                    model.theVtxModelLods[i] = new SourceVtxModelLod();
                    model.theVtxModelLods[i].meshCount = DataParser.ReadInt(stream);
                    model.theVtxModelLods[i].meshOffset = DataParser.ReadInt(stream);
                    model.theVtxModelLods[i].switchPoint = DataParser.ReadFloat(stream);
                }
                for (int i = 0; i < model.theVtxModelLods.Length; i++)
                    ReadSourceVtxMeshes(stream, modelLodOffsets[i], model.theVtxModelLods[i]);
            }
        }
        private void ReadSourceVtxMeshes(Stream stream, long modelLodOffset, SourceVtxModelLod modelLod)
        {
            if (modelLod.meshCount > 0 && modelLod.meshOffset != 0)
            {
                long[] meshOffsets = new long[modelLod.meshCount];
                stream.Position = modelLodOffset + modelLod.meshOffset;
                modelLod.theVtxMeshes = new SourceVtxMesh[modelLod.meshCount];
                for (int i = 0; i < modelLod.theVtxMeshes.Length; i++)
                {
                    meshOffsets[i] = stream.Position;
                    modelLod.theVtxMeshes[i] = new SourceVtxMesh();
                    modelLod.theVtxMeshes[i].stripGroupCount = DataParser.ReadInt(stream);
                    modelLod.theVtxMeshes[i].stripGroupOffset = DataParser.ReadInt(stream);
                    modelLod.theVtxMeshes[i].flags = DataParser.ReadByte(stream);
                }
                for (int i = 0; i < modelLod.theVtxMeshes.Length; i++)
                    ReadSourceVtxStripGroups(stream, meshOffsets[i], modelLod.theVtxMeshes[i]);
            }
        }
        private void ReadSourceVtxStripGroups(Stream stream, long meshOffset, SourceVtxMesh mesh)
        {
            if (mesh.stripGroupCount > 0 && mesh.stripGroupOffset != 0)
            {
                long[] stripGroupOffsets = new long[mesh.stripGroupCount];
                stream.Position = meshOffset + mesh.stripGroupOffset;
                mesh.theVtxStripGroups = new SourceVtxStripGroup[mesh.stripGroupCount];
                for (int i = 0; i < mesh.theVtxStripGroups.Length; i++)
                {
                    stripGroupOffsets[i] = stream.Position;
                    mesh.theVtxStripGroups[i] = new SourceVtxStripGroup();
                    mesh.theVtxStripGroups[i].vertexCount = DataParser.ReadInt(stream);
                    mesh.theVtxStripGroups[i].vertexOffset = DataParser.ReadInt(stream);
                    mesh.theVtxStripGroups[i].indexCount = DataParser.ReadInt(stream);
                    mesh.theVtxStripGroups[i].indexOffset = DataParser.ReadInt(stream);
                    mesh.theVtxStripGroups[i].stripCount = DataParser.ReadInt(stream);
                    mesh.theVtxStripGroups[i].stripOffset = DataParser.ReadInt(stream);
                    mesh.theVtxStripGroups[i].flags = DataParser.ReadByte(stream);
                }
                for (int i = 0; i < mesh.theVtxStripGroups.Length; i++)
                {
                    ReadSourceVtxVertices(stream, stripGroupOffsets[i], mesh.theVtxStripGroups[i]);
                    ReadSourceVtxIndices(stream, stripGroupOffsets[i], mesh.theVtxStripGroups[i]);
                    ReadSourceVtxStrips(stream, stripGroupOffsets[i], mesh.theVtxStripGroups[i]);
                }
            }
        }
        private void ReadSourceVtxVertices(Stream stream, long stripGroupOffset, SourceVtxStripGroup stripGroup)
        {
            if (stripGroup.vertexCount > 0 && stripGroup.vertexOffset != 0)
            {
                stream.Position = stripGroupOffset + stripGroup.vertexOffset;
                stripGroup.theVtxVertices = new SourceVtxVertex[stripGroup.vertexCount];
                for (int i = 0; i < stripGroup.theVtxVertices.Length; i++)
                {
                    stripGroup.theVtxVertices[i] = new SourceVtxVertex();
                    stripGroup.theVtxVertices[i].boneWeightIndex = new byte[VVDParser.MAX_NUM_BONES_PER_VERT];
                    for (int j = 0; j < stripGroup.theVtxVertices[i].boneWeightIndex.Length; j++)
                    {
                        stripGroup.theVtxVertices[i].boneWeightIndex[j] = DataParser.ReadByte(stream);
                    }

                    stripGroup.theVtxVertices[i].boneCount = DataParser.ReadByte(stream);
                    stripGroup.theVtxVertices[i].originalMeshVertexIndex = DataParser.ReadUShort(stream);

                    stripGroup.theVtxVertices[i].boneId = new byte[VVDParser.MAX_NUM_BONES_PER_VERT];
                    for (int j = 0; j < stripGroup.theVtxVertices[i].boneId.Length; j++)
                    {
                        stripGroup.theVtxVertices[i].boneId[j] = DataParser.ReadByte(stream);
                    }
                }
            }
        }
        private void ReadSourceVtxIndices(Stream stream, long stripGroupOffset, SourceVtxStripGroup stripGroup)
        {
            if (stripGroup.indexCount > 0 && stripGroup.indexOffset != 0)
            {
                stream.Position = stripGroupOffset + stripGroup.indexOffset;
                stripGroup.theVtxIndices = new ushort[stripGroup.indexCount];
                for (int i = 0; i < stripGroup.theVtxIndices.Length; i++)
                {
                    stripGroup.theVtxIndices[i] = DataParser.ReadUShort(stream);
                }
            }
        }
        private void ReadSourceVtxStrips(Stream stream, long stripGroupOffset, SourceVtxStripGroup stripGroup)
        {
            if (stripGroup.stripCount > 0 && stripGroup.stripOffset != 0)
            {
                stream.Position = stripGroupOffset + stripGroup.stripOffset;
                stripGroup.theVtxStrips = new SourceVtxStrip[stripGroup.stripCount];
                for (int i = 0; i < stripGroup.theVtxStrips.Length; i++)
                {
                    stripGroup.theVtxStrips[i] = new SourceVtxStrip();
                    stripGroup.theVtxStrips[i].indexCount = DataParser.ReadInt(stream);
                    stripGroup.theVtxStrips[i].indexMeshIndex = DataParser.ReadInt(stream);
                    stripGroup.theVtxStrips[i].vertexCount = DataParser.ReadInt(stream);
                    stripGroup.theVtxStrips[i].vertexMeshIndex = DataParser.ReadInt(stream);
                    stripGroup.theVtxStrips[i].boneCount = DataParser.ReadShort(stream);
                    stripGroup.theVtxStrips[i].flags = DataParser.ReadByte(stream);
                    stripGroup.theVtxStrips[i].boneStateChangeCount = DataParser.ReadInt(stream);
                    stripGroup.theVtxStrips[i].boneStateChangeOffset = DataParser.ReadInt(stream);
                }
            }
        }
    }
}