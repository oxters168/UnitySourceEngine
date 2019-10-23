using UnityEngine;
using System.IO;
using System;
//using System.Collections.Generic;

namespace UnitySourceEngine
{
    public class VVDParser : IDisposable
    {
        public const int MAX_NUM_LODS = 7, MAX_NUM_BONES_PER_VERT = 3;
        public vertexFileHeader_t header;
        public vertexFileFixup_t[] fileFixup;
        public mstudiovertex_t[] vertices;

        private long fileOffsetPosition;

        public VVDParser()
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
        ~VVDParser()
        {
            // Finalizer calls Dispose(false)
            Dispose(false);
        }
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                header.Dispose();
                fileFixup = null;

                if (vertices != null)
                    foreach (var vertex in vertices)
                        vertex.Dispose();

                vertices = null;
            }
        }

        public void ParseHeader(Stream stream, long fileOffset = 0)
        {
            fileOffsetPosition = fileOffset;

            ParseHeader(stream);
        }
        public void Parse(Stream stream, int rootLod = 0, long fileOffset = 0)
        {
            fileOffsetPosition = fileOffset;

            ParseFixupTable(stream);
            ParseVertices(stream, rootLod);
        }
        private void ParseHeader(Stream stream)
        {
            header = new vertexFileHeader_t();

            stream.Position = fileOffsetPosition;

            header.id = DataParser.ReadInt(stream);
            header.version = DataParser.ReadInt(stream); // MODEL_VERTEX_FILE_VERSION
            header.checksum = DataParser.ReadLong(stream); // same as studiohdr_t, ensures sync
            header.numLODs = DataParser.ReadInt(stream); // num of valid lods

            header.numLODVertices = new int[MAX_NUM_LODS]; // num verts for desired root lod (size is MAX_NUM_LODS = 8)
            for (int i = 0; i < header.numLODVertices.Length; i++)
            {
                header.numLODVertices[i] = DataParser.ReadInt(stream);
            }

            header.numFixups = DataParser.ReadInt(stream); // num of vertexFileFixup_t
            header.fixupTableStart = DataParser.ReadInt(stream); // offset from base to fixup table
            header.vertexDataStart = DataParser.ReadInt(stream); // offset from base to vertex block
            header.tangentDataStart = DataParser.ReadInt(stream); // offset from base to tangent block
        }
        private void ParseFixupTable(Stream stream)
        {
            fileFixup = new vertexFileFixup_t[header.numFixups];

            stream.Position = fileOffsetPosition + header.fixupTableStart;
            for (int i = 0; i < fileFixup.Length; i++)
            {
                fileFixup[i].lod = DataParser.ReadInt(stream); // used to skip culled root lod
                fileFixup[i].sourceVertexID = DataParser.ReadInt(stream); // absolute index from start of vertex/tangent blocks
                fileFixup[i].numVertices = DataParser.ReadInt(stream);
            }
        }
        private void ParseVertices(Stream stream, int rootLod)
        {
            for (int i = 0; i < rootLod; i++)
                header.numLODVertices[i] = header.numLODVertices[rootLod];

            //int lodIndex = 0;

            if (header.numLODs > 0)
            {
                Func<Stream, mstudiovertex_t> ReadVertexFromStream = (innerStream) =>
                {
                    mstudiovertex_t vertex = new mstudiovertex_t();

                    vertex.m_BoneWeights.weight = new float[MAX_NUM_BONES_PER_VERT]; //0 + 12 = 12
                    for (int k = 0; k < vertex.m_BoneWeights.weight.Length; k++)
                        vertex.m_BoneWeights.weight[k] = DataParser.ReadFloat(innerStream);

                    vertex.m_BoneWeights.bone = new char[MAX_NUM_BONES_PER_VERT]; //12 + 12 = 24
                    for (int k = 0; k < vertex.m_BoneWeights.bone.Length; k++)
                        vertex.m_BoneWeights.bone[k] = DataParser.ReadChar(innerStream);

                    vertex.m_BoneWeights.numbones = DataParser.ReadByte(innerStream); //24 + 1 = 25

                    float ex = DataParser.ReadFloat(innerStream); //25 + 4 = 29
                    float why = DataParser.ReadFloat(innerStream); //33 + 4 = 37
                    float zee = DataParser.ReadFloat(innerStream); //29 + 4 = 33
                    vertex.m_vecPosition = new Vector3(ex, why, zee);

                    ex = DataParser.ReadFloat(innerStream); //37 + 4 = 41
                    why = DataParser.ReadFloat(innerStream); //45 + 4 = 49
                    zee = DataParser.ReadFloat(innerStream); //41 + 4 = 45
                    vertex.m_vecNormal = new Vector3(ex, why, zee);

                    ex = DataParser.ReadFloat(innerStream); //49 + 4 = 53
                    why = DataParser.ReadFloat(innerStream); //53 + 4 = 57
                    vertex.m_vecTexCoord = new Vector2(ex, 1 - why);
                    return vertex;
                };

                stream.Position = fileOffsetPosition + header.vertexDataStart;

                //int vertexCount = header.numLODVertices[0];
                int vertexCount = 0;
                for (int i = 0; i < header.numLODVertices.Length; i++)
                    vertexCount += header.numLODVertices[i];

                vertices = new mstudiovertex_t[vertexCount];
                for (int i = 0; i < vertices.Length; i++)
                    vertices[i] = ReadVertexFromStream(stream);

                if (header.numFixups > 0)
                {
                    vertexCount = 0;
                    for (int fixupIndex = 0; fixupIndex < header.numFixups; fixupIndex++)
                    {
                        if (fileFixup[fixupIndex].lod < rootLod)
                            continue;

                        vertexCount += fileFixup[fixupIndex].numVertices;
                    }

                    mstudiovertex_t[] oldVertices = vertices;
                    vertices = new mstudiovertex_t[vertexCount];

                    int currentIndex = 0;
                    for (int fixupIndex = 0; fixupIndex < header.numFixups; fixupIndex++)
                    {
                        if (fileFixup[fixupIndex].lod < rootLod)
                            continue;

                        Array.Copy(oldVertices, fileFixup[fixupIndex].sourceVertexID, vertices, currentIndex, fileFixup[fixupIndex].numVertices);
                        currentIndex += fileFixup[fixupIndex].numVertices;
                    }
                }
            }
            else
                Debug.LogError("VVDParser: Header's numLODs less than or equal to zero");
        }
    }
}