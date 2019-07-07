using UnityEngine;
using System.IO;
using System;

public class VVDParser : IDisposable
{
    public const int MAX_NUM_LODS = 7, MAX_NUM_BONES_PER_VERT = 3;
    //private byte[] data;
    public vertexFileHeader_t header;
    public vertexFileFixup_t[] fileFixup;
    public mstudiovertex_t[][] vertices;

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
            if (vertices != null)
                foreach (var subVertices in vertices)
                    if (subVertices != null)
                        foreach (var vertex in subVertices)
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
            ParseVertices(stream);
            ParseFixupTable(stream);
        //}
    }
    private vertexFileHeader_t ParseHeader(Stream stream)
    {
        header = new vertexFileHeader_t();

        header.id = DataParser.ReadInt(stream);
        header.version = DataParser.ReadInt(stream); // MODEL_VERTEX_FILE_VERSION
        header.checksum = DataParser.ReadLong(stream); // same as studiohdr_t, ensures sync
        header.numLODs = DataParser.ReadInt(stream); // num of valid lods

        header.numLODVertices = new int[MAX_NUM_LODS]; // num verts for desired root lod (size is MAX_NUM_LODS = 8)
        for(int i = 0; i < header.numLODVertices.Length; i++)
        {
            header.numLODVertices[i] = DataParser.ReadInt(stream);
        }

        header.numFixups = DataParser.ReadInt(stream); // num of vertexFileFixup_t
        header.fixupTableStart = DataParser.ReadInt(stream); // offset from base to fixup table
        header.vertexDataStart = DataParser.ReadInt(stream); // offset from base to vertex block
        header.tangentDataStart = DataParser.ReadInt(stream); // offset from base to tangent block

        return header;
    }
    private vertexFileFixup_t[] ParseFixupTable(Stream stream)
    {
        fileFixup = new vertexFileFixup_t[header.numFixups];

        stream.Position = fileOffsetPosition + header.fixupTableStart;
        for (int i = 0; i < fileFixup.Length; i++)
        {
            fileFixup[i].lod = DataParser.ReadInt(stream); // used to skip culled root lod
            fileFixup[i].sourceVertexID = DataParser.ReadInt(stream); // absolute index from start of vertex/tangent blocks
            fileFixup[i].numVertices = DataParser.ReadInt(stream);
        }

        return fileFixup;
    }
    private mstudiovertex_t[][] ParseVertices(Stream stream)
    {
        if(header.numLODs > 0)
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

                    vertices[i][j].m_vecPosition = new Vector3(DataParser.ReadFloat(stream), DataParser.ReadFloat(stream), DataParser.ReadFloat(stream));
                    vertices[i][j].m_vecPosition = new Vector3(vertices[i][j].m_vecPosition.x, vertices[i][j].m_vecPosition.z, vertices[i][j].m_vecPosition.y);
                    vertices[i][j].m_vecNormal = new Vector3(DataParser.ReadFloat(stream), DataParser.ReadFloat(stream), DataParser.ReadFloat(stream));
                    vertices[i][j].m_vecNormal = new Vector3(vertices[i][j].m_vecNormal.x, vertices[i][j].m_vecNormal.z, vertices[i][j].m_vecNormal.y);
                    vertices[i][j].m_vecTexCoord = new Vector2(DataParser.ReadFloat(stream), DataParser.ReadFloat(stream));
                    vertices[i][j].m_vecTexCoord = new Vector2(vertices[i][j].m_vecTexCoord.x, 1 - vertices[i][j].m_vecTexCoord.y);
                }
            }
        }

        return vertices;
    }
}
