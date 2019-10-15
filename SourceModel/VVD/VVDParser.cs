using UnityEngine;
using System.IO;
using System;
using System.Collections.Generic;

namespace UnitySourceEngine
{
    public class VVDParser : IDisposable
    {
        public const int MAX_NUM_LODS = 7, MAX_NUM_BONES_PER_VERT = 3;
        //private byte[] data;
        public vertexFileHeader_t header;
        public vertexFileFixup_t[] fileFixup;
        //public mstudiovertex_t[][] vertices;
        public mstudiovertex_t[] vertices;

        private long fileOffsetPosition;

        public VVDParser()
        {
            //data = _data;
            //this.stream = stream;
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
                //data = null;
                header.Dispose();
                fileFixup = null;
                //if (vertices != null)
                //    foreach (var subVertices in vertices)
                //        if (subVertices != null)
                //            foreach (var vertex in subVertices)
                //                vertex.Dispose();
                if (vertices != null)
                    foreach (var vertex in vertices)
                        vertex.Dispose();

                vertices = null;
            }
        }

        public void Parse(Stream stream, long fileOffset = 0)
        {
            //if (data == null || data.Length <= 0)
            //    throw new System.ArgumentNullException("No data provided");

            fileOffsetPosition = fileOffset;
            //using (var stream = new MemoryStream(data))
            //{
            ParseHeader(stream);
            ParseFixupTable(stream);
            ParseVertices(stream);
            //}
        }
        private void ParseHeader(Stream stream)
        {
            header = new vertexFileHeader_t();

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
        /*private mstudiovertex_t[][] ParseVertices(Stream stream)
        {
            if (header.numLODs > 0)
            {
                stream.Position = fileOffsetPosition + header.vertexDataStart;

                vertices = new mstudiovertex_t[header.numLODVertices.Length][];
                for (int i = 0; i < vertices.Length; i++)
                {
                    vertices[i] = new mstudiovertex_t[header.numLODVertices[i]];
                    for (int j = 0; j < vertices[i].Length; j++)
                    {
                        vertices[i][j].m_BoneWeights.weight = new float[MAX_NUM_BONES_PER_VERT];
                        for (int k = 0; k < vertices[i][j].m_BoneWeights.weight.Length; k++)
                        {
                            vertices[i][j].m_BoneWeights.weight[k] = DataParser.ReadFloat(stream);
                        }
                        vertices[i][j].m_BoneWeights.bone = new char[MAX_NUM_BONES_PER_VERT];
                        for (int k = 0; k < vertices[i][j].m_BoneWeights.bone.Length; k++)
                        {
                            vertices[i][j].m_BoneWeights.bone[k] = DataParser.ReadChar(stream);
                        }
                        vertices[i][j].m_BoneWeights.numbones = DataParser.ReadByte(stream);

                        float ex = DataParser.ReadFloat(stream);
                        float zee = DataParser.ReadFloat(stream);
                        float why = DataParser.ReadFloat(stream);
                        vertices[i][j].m_vecPosition = new Vector3(ex, why, zee);

                        ex = DataParser.ReadFloat(stream);
                        zee = DataParser.ReadFloat(stream);
                        why = DataParser.ReadFloat(stream);
                        vertices[i][j].m_vecNormal = new Vector3(ex, why, zee);

                        ex = DataParser.ReadFloat(stream);
                        why = DataParser.ReadFloat(stream);
                        vertices[i][j].m_vecTexCoord = new Vector2(ex, 1 - why);
                    }
                }
            }

            return vertices;
        }*/
        private void ParseVertices(Stream stream)
        {
            //for (int i = 0; i < rootLOD; i++)
            //    header.numLODVertices[i] = header.numLODVertices[rootLOD];

            int lodIndex = 0;

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

                //if (header.numFixups <= 0)
                //{
                stream.Position = fileOffsetPosition + header.vertexDataStart;

                int vertexCount = 0;
                for (int i = 0; i < header.numLODVertices.Length; i++)
                    vertexCount += header.numLODVertices[i];

                vertices = new mstudiovertex_t[vertexCount];
                for (int i = 0; i < vertices.Length; i++)
                    vertices[i] = ReadVertexFromStream(stream);
                //}
                if (header.numFixups > 0)
                {
                    vertexCount = 0;
                    for (int fixupIndex = 0; fixupIndex < header.numFixups; fixupIndex++)
                    {
                        if (fileFixup[fixupIndex].lod < lodIndex)
                            continue;

                        vertexCount += fileFixup[fixupIndex].numVertices;
                    }

                    mstudiovertex_t[] oldVertices = vertices;
                    vertices = new mstudiovertex_t[vertexCount];

                    int currentIndex = 0;
                    for (int fixupIndex = 0; fixupIndex < header.numFixups; fixupIndex++)
                    {
                        if (fileFixup[fixupIndex].lod < lodIndex)
                            continue;

                        Array.Copy(oldVertices, fileFixup[fixupIndex].sourceVertexID, vertices, currentIndex, fileFixup[fixupIndex].numVertices);
                        currentIndex += fileFixup[fixupIndex].numVertices;
                        //stream.Position = fileOffsetPosition + header.vertexDataStart + fileFixup[fixupIndex].sourceVertexID;

                        //for (int i = 0; i < fileFixup[fixupIndex].numVertices; i++)
                        //    vertices[currentIndex++] = ReadVertexFromStream(stream);
                    }
                }
                /*else
                {
                    int vertexCount = 0;
                    for (int fixupIndex = 0; fixupIndex < header.numFixups; fixupIndex++)
                    {
                        if (fileFixup[fixupIndex].lod < lodIndex)
                            continue;

                        vertexCount += fileFixup[fixupIndex].numVertices;
                    }

                    vertices = new mstudiovertex_t[vertexCount];
                    int currentIndex = 0;
                    for (int fixupIndex = 0; fixupIndex < header.numFixups; fixupIndex++)
                    {
                        if (fileFixup[fixupIndex].lod < lodIndex)
                            continue;

                        stream.Position = fileOffsetPosition + header.vertexDataStart + fileFixup[fixupIndex].sourceVertexID;

                        for (int i = 0; i < fileFixup[fixupIndex].numVertices; i++)
                            vertices[currentIndex++] = ReadVertexFromStream(stream);
                    }
                }*/
            }
            else
                Debug.LogError("VVDParser: Header's numLODs less than or equal to zero");
        }
    }
}