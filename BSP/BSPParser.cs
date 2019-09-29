using UnityEngine;
using System.IO;
using System;
using System.Collections.Generic;
using System.Threading;

namespace UnitySourceEngine
{
    public class BSPParser : IDisposable
    {
        public const string TEXTURE_STRING_DATA_SPLITTER = ":";

        public string fileLocation { get; private set; }

        public int identifier { get; private set; }
        public int version { get; private set; }
        public int mapRevision { get; private set; }

        private lump_t[] lumps;
        private object[] lumpData;
        public dgamelumpheader_t gameLumpHeader { get; private set; }

        #region Geometry
        public Vector3[] vertices;
        public dedge_t[] edges;
        public dface_t[] faces;
        public int[] surfedges;
        public dplane_t[] planes;

        public ddispinfo_t[] dispInfo;
        public dDispVert[] dispVerts;

        public texinfo_t[] texInfo;
        public dtexdata_t[] texData;
        public int[] texStringTable;
        public List<string> textureStringData;

        public StaticProps_t staticProps;
        #endregion

        public BSPParser(string fileLocation)
        {
            this.fileLocation = fileLocation;
            lumps = new lump_t[64];
            lumpData = new object[64];
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
        ~BSPParser()
        {
            // Finalizer calls Dispose(false)
            Dispose(false);
        }
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                lumps = null;
                lumpData = null;
                vertices = null;
                if (edges != null)
                    foreach (var edge in edges)
                        edge.Dispose();
                edges = null;
                if (faces != null)
                    foreach (var face in faces)
                        face.Dispose();
                faces = null;
                surfedges = null;
                planes = null;
                if (dispInfo != null)
                    foreach (var di in dispInfo)
                        di.Dispose();
                dispInfo = null;
                dispVerts = null;
                if (texInfo != null)
                    foreach (var ti in texInfo)
                        ti.Dispose();
                texInfo = null;
                texData = null;
                texStringTable = null;
                textureStringData = null;
                staticProps?.Dispose();
                staticProps = null;
            }
        }

        private void LoadLumps(Stream stream, CancellationTokenSource cancelSource = null)
        {
            for (int i = 0; i < lumps.Length; i++)
            {
                if (cancelSource != null && cancelSource.IsCancellationRequested)
                    return;

                lump_t lump = new lump_t();
                lump.fileofs = DataParser.ReadInt(stream);
                lump.filelen = DataParser.ReadInt(stream);
                lump.version = DataParser.ReadInt(stream);
                lump.fourCC = DataParser.ReadInt(stream);
                lumps[i] = lump;
            }
        }
        private void LoadGameLumps(Stream stream, CancellationTokenSource cancelSource = null)
        {
            lump_t lump = lumps[35];
            stream.Position = lump.fileofs;

            //gameLumpHeader = new dgamelumpheader_t();
            dgamelumpheader_t gameLumpHeader = new dgamelumpheader_t();
            gameLumpHeader.lumpCount = DataParser.ReadInt(stream);
            gameLumpHeader.gamelump = new dgamelump_t[gameLumpHeader.lumpCount];

            for (int i = 0; i < gameLumpHeader.gamelump.Length; i++)
            {
                if (cancelSource != null && cancelSource.IsCancellationRequested)
                    return;

                gameLumpHeader.gamelump[i] = new dgamelump_t();
                gameLumpHeader.gamelump[i].id = DataParser.ReadInt(stream);
                gameLumpHeader.gamelump[i].flags = DataParser.ReadUShort(stream);
                gameLumpHeader.gamelump[i].version = DataParser.ReadUShort(stream);
                gameLumpHeader.gamelump[i].fileofs = DataParser.ReadInt(stream);
                gameLumpHeader.gamelump[i].filelen = DataParser.ReadInt(stream);
            }

            this.gameLumpHeader = gameLumpHeader;
            lumpData[35] = gameLumpHeader.gamelump;
        }

        public void ParseData(CancellationTokenSource cancelSource = null)
        {
            using (FileStream stream = new FileStream(fileLocation, FileMode.Open, FileAccess.Read))
            {
                identifier = DataParser.ReadInt(stream);
                version = DataParser.ReadInt(stream);
                LoadLumps(stream, cancelSource);
                LoadGameLumps(stream, cancelSource);
                mapRevision = DataParser.ReadInt(stream);

                vertices = GetVertices(stream, cancelSource);

                edges = GetEdges(stream, cancelSource);
                faces = GetFaces(stream, cancelSource);
                surfedges = GetSurfedges(stream, cancelSource);
                planes = GetPlanes(stream, cancelSource);

                dispInfo = GetDispInfo(stream, cancelSource);
                dispVerts = GetDispVerts(stream, cancelSource);

                texInfo = GetTextureInfo(stream, cancelSource);
                texData = GetTextureData(stream, cancelSource);
                texStringTable = GetTextureStringTable(stream, cancelSource);
                textureStringData = GetTextureStringData(stream, cancelSource);

                staticProps = GetStaticProps(stream, cancelSource);
            }
        }

        private string GetEntities(Stream stream, CancellationTokenSource cancelSource = null)
        {
            lump_t lump = lumps[0];
            string allEntities = "";
            stream.Position = lump.fileofs;

            for (int i = 0; i < lump.filelen; i++)
            {
                if (cancelSource != null && cancelSource.IsCancellationRequested)
                    return null;

                char nextChar = DataParser.ReadChar(stream);
                allEntities += nextChar;
            }

            return allEntities;
        }

        private dbrush_t[] GetBrushes(Stream stream, CancellationTokenSource cancelSource = null)
        {
            lump_t lump = lumps[18];
            dbrush_t[] brushes = new dbrush_t[lump.filelen / 12];
            stream.Position = lump.fileofs;

            for (int i = 0; i < brushes.Length; i++)
            {
                if (cancelSource != null && cancelSource.IsCancellationRequested)
                    return null;

                brushes[i].firstside = DataParser.ReadInt(stream);
                brushes[i].numsides = DataParser.ReadInt(stream);
                brushes[i].contents = DataParser.ReadInt(stream);
            }

            lumpData[18] = brushes;
            return brushes;
        }

        private dbrushside_t[] GetBrushSides(Stream stream, CancellationTokenSource cancelSource = null)
        {
            lump_t lump = lumps[19];
            dbrushside_t[] brushSides = new dbrushside_t[lump.filelen / 8];
            stream.Position = lump.fileofs;

            for (int i = 0; i < brushSides.Length; i++)
            {
                if (cancelSource != null && cancelSource.IsCancellationRequested)
                    return null;

                brushSides[i].planenum = DataParser.ReadUShort(stream);
                brushSides[i].texinfo = DataParser.ReadShort(stream);
                brushSides[i].dispinfo = DataParser.ReadShort(stream);
                brushSides[i].bevel = DataParser.ReadShort(stream);
            }

            lumpData[19] = brushSides;
            return brushSides;
        }

        private ddispinfo_t[] GetDispInfo(Stream stream, CancellationTokenSource cancelSource = null)
        {
            lump_t lump = lumps[26];
            ddispinfo_t[] displacementInfo = new ddispinfo_t[lump.filelen / 86];
            stream.Position = lump.fileofs;

            for (int i = 0; i < displacementInfo.Length; i++)
            {
                if (cancelSource != null && cancelSource.IsCancellationRequested)
                    return null;

                displacementInfo[i].startPosition = new Vector3(DataParser.ReadFloat(stream), DataParser.ReadFloat(stream), DataParser.ReadFloat(stream));
                displacementInfo[i].DispVertStart = DataParser.ReadInt(stream);
                displacementInfo[i].DispTriStart = DataParser.ReadInt(stream);
                displacementInfo[i].power = DataParser.ReadInt(stream);
                displacementInfo[i].minTess = DataParser.ReadInt(stream);
                displacementInfo[i].smoothingAngle = DataParser.ReadFloat(stream);
                displacementInfo[i].contents = DataParser.ReadInt(stream);
                displacementInfo[i].MapFace = DataParser.ReadUShort(stream);
                displacementInfo[i].LightmapAlphaStart = DataParser.ReadInt(stream);
                displacementInfo[i].LightmapSamplePositionStart = DataParser.ReadInt(stream);
                stream.Position += 90;
                displacementInfo[i].AllowedVerts = new uint[10] { DataParser.ReadUInt(stream), DataParser.ReadUInt(stream), DataParser.ReadUInt(stream), DataParser.ReadUInt(stream), DataParser.ReadUInt(stream), DataParser.ReadUInt(stream), DataParser.ReadUInt(stream), DataParser.ReadUInt(stream), DataParser.ReadUInt(stream), DataParser.ReadUInt(stream) };
            }

            lumpData[26] = displacementInfo;
            return displacementInfo;
        }

        private dDispVert[] GetDispVerts(Stream stream, CancellationTokenSource cancelSource = null)
        {
            lump_t lump = lumps[33];
            dDispVert[] displacementVertices = new dDispVert[lump.filelen / 20];
            stream.Position = lump.fileofs;

            for (int i = 0; i < displacementVertices.Length; i++)
            {
                if (cancelSource != null && cancelSource.IsCancellationRequested)
                    return null;

                displacementVertices[i].vec = new Vector3(DataParser.ReadFloat(stream), DataParser.ReadFloat(stream), DataParser.ReadFloat(stream));
                displacementVertices[i].dist = DataParser.ReadFloat(stream);
                displacementVertices[i].alpha = DataParser.ReadFloat(stream);
            }

            lumpData[33] = displacementVertices;
            return displacementVertices;
        }

        private dedge_t[] GetEdges(Stream stream, CancellationTokenSource cancelSource = null)
        {
            lump_t lump = lumps[12];
            dedge_t[] edges = new dedge_t[lump.filelen / 4];
            stream.Position = lump.fileofs;

            for (int i = 0; i < edges.Length; i++)
            {
                if (cancelSource != null && cancelSource.IsCancellationRequested)
                    return null;

                edges[i].v = new ushort[2];
                edges[i].v[0] = DataParser.ReadUShort(stream);
                edges[i].v[1] = DataParser.ReadUShort(stream);
            }

            lumpData[12] = edges;
            return edges;
        }

        private Vector3[] GetVertices(Stream stream, CancellationTokenSource cancelSource = null)
        {
            lump_t lump = lumps[3];
            Vector3[] vertices = new Vector3[lump.filelen / 12];
            stream.Position = lump.fileofs;

            for (int i = 0; i < vertices.Length; i++)
            {
                if (cancelSource != null && cancelSource.IsCancellationRequested)
                    return null;

                vertices[i] = new Vector3(DataParser.ReadFloat(stream), DataParser.ReadFloat(stream), DataParser.ReadFloat(stream));
            }

            lumpData[3] = vertices;
            return vertices;
        }

        private dface_t[] GetOriginalFaces(Stream stream, CancellationTokenSource cancelSource = null)
        {
            lump_t lump = lumps[27];
            dface_t[] faces = new dface_t[lump.filelen / 56];
            stream.Position = lump.fileofs;

            for (int i = 0; i < faces.Length; i++)
            {
                if (cancelSource != null && cancelSource.IsCancellationRequested)
                    return null;

                faces[i].planenum = DataParser.ReadUShort(stream);
                faces[i].side = DataParser.ReadByte(stream);
                faces[i].onNode = DataParser.ReadByte(stream);
                faces[i].firstedge = DataParser.ReadInt(stream);
                faces[i].numedges = DataParser.ReadShort(stream);
                faces[i].texinfo = DataParser.ReadShort(stream);
                faces[i].dispinfo = DataParser.ReadShort(stream);
                faces[i].surfaceFogVolumeID = DataParser.ReadShort(stream);
                faces[i].styles = new byte[4] { DataParser.ReadByte(stream), DataParser.ReadByte(stream), DataParser.ReadByte(stream), DataParser.ReadByte(stream) };
                faces[i].lightofs = DataParser.ReadInt(stream);
                faces[i].area = DataParser.ReadFloat(stream);
                faces[i].LightmapTextureMinsInLuxels = new int[2] { DataParser.ReadInt(stream), DataParser.ReadInt(stream) };
                faces[i].LightmapTextureSizeInLuxels = new int[2] { DataParser.ReadInt(stream), DataParser.ReadInt(stream) };
                faces[i].origFace = DataParser.ReadInt(stream);
                faces[i].numPrims = DataParser.ReadUShort(stream);
                faces[i].firstPrimID = DataParser.ReadUShort(stream);
                faces[i].smoothingGroups = DataParser.ReadUInt(stream);
            }

            lumpData[27] = faces;
            return faces;
        }

        private dface_t[] GetFaces(Stream stream, CancellationTokenSource cancelSource = null)
        {
            lump_t lump = lumps[7];
            dface_t[] faces = new dface_t[lump.filelen / 56];
            stream.Position = lump.fileofs;

            for (int i = 0; i < faces.Length; i++)
            {
                if (cancelSource != null && cancelSource.IsCancellationRequested)
                    return null;

                faces[i].planenum = DataParser.ReadUShort(stream);
                faces[i].side = DataParser.ReadByte(stream);
                faces[i].onNode = DataParser.ReadByte(stream);
                faces[i].firstedge = DataParser.ReadInt(stream);
                faces[i].numedges = DataParser.ReadShort(stream);
                faces[i].texinfo = DataParser.ReadShort(stream);
                faces[i].dispinfo = DataParser.ReadShort(stream);
                faces[i].surfaceFogVolumeID = DataParser.ReadShort(stream);
                faces[i].styles = new byte[4] { DataParser.ReadByte(stream), DataParser.ReadByte(stream), DataParser.ReadByte(stream), DataParser.ReadByte(stream) };
                faces[i].lightofs = DataParser.ReadInt(stream);
                faces[i].area = DataParser.ReadFloat(stream);
                faces[i].LightmapTextureMinsInLuxels = new int[2] { DataParser.ReadInt(stream), DataParser.ReadInt(stream) };
                faces[i].LightmapTextureSizeInLuxels = new int[2] { DataParser.ReadInt(stream), DataParser.ReadInt(stream) };
                faces[i].origFace = DataParser.ReadInt(stream);
                faces[i].numPrims = DataParser.ReadUShort(stream);
                faces[i].firstPrimID = DataParser.ReadUShort(stream);
                faces[i].smoothingGroups = DataParser.ReadUInt(stream);
            }

            lumpData[7] = faces;
            return faces;
        }

        private dplane_t[] GetPlanes(Stream stream, CancellationTokenSource cancelSource = null)
        {
            lump_t lump = lumps[1];
            dplane_t[] planes = new dplane_t[lump.filelen / 20];
            stream.Position = lump.fileofs;

            for (int i = 0; i < planes.Length; i++)
            {
                if (cancelSource != null && cancelSource.IsCancellationRequested)
                    return null;

                planes[i].normal = new Vector3(DataParser.ReadFloat(stream), DataParser.ReadFloat(stream), DataParser.ReadFloat(stream));
                planes[i].dist = DataParser.ReadFloat(stream);
                planes[i].type = DataParser.ReadInt(stream);
            }

            lumpData[1] = planes;
            return planes;
        }

        private int[] GetSurfedges(Stream stream, CancellationTokenSource cancelSource = null)
        {

            lump_t lump = lumps[13];
            int[] surfedges = new int[lump.filelen / 4];
            stream.Position = lump.fileofs;

            for (int i = 0; i < lump.filelen / 4; i++)
            {
                if (cancelSource != null && cancelSource.IsCancellationRequested)
                    return null;

                surfedges[i] = DataParser.ReadInt(stream);
            }

            lumpData[13] = surfedges;
            return surfedges;
        }

        private texinfo_t[] GetTextureInfo(Stream stream, CancellationTokenSource cancelSource = null)
        {
            lump_t lump = lumps[6];
            texinfo_t[] textureInfo = new texinfo_t[lump.filelen / 72];
            stream.Position = lump.fileofs;

            for (int i = 0; i < textureInfo.Length; i++)
            {
                if (cancelSource != null && cancelSource.IsCancellationRequested)
                    return null;

                textureInfo[i].textureVecs = new float[2][];
                textureInfo[i].textureVecs[0] = new float[4] { DataParser.ReadFloat(stream), DataParser.ReadFloat(stream), DataParser.ReadFloat(stream), DataParser.ReadFloat(stream) };
                textureInfo[i].textureVecs[1] = new float[4] { DataParser.ReadFloat(stream), DataParser.ReadFloat(stream), DataParser.ReadFloat(stream), DataParser.ReadFloat(stream) };
                textureInfo[i].lightmapVecs = new float[2][];
                textureInfo[i].lightmapVecs[0] = new float[4] { DataParser.ReadFloat(stream), DataParser.ReadFloat(stream), DataParser.ReadFloat(stream), DataParser.ReadFloat(stream) };
                textureInfo[i].lightmapVecs[1] = new float[4] { DataParser.ReadFloat(stream), DataParser.ReadFloat(stream), DataParser.ReadFloat(stream), DataParser.ReadFloat(stream) };
                textureInfo[i].flags = DataParser.ReadInt(stream);
                textureInfo[i].texdata = DataParser.ReadInt(stream);
            }

            lumpData[6] = textureInfo;
            return textureInfo;
        }

        private dtexdata_t[] GetTextureData(Stream stream, CancellationTokenSource cancelSource = null)
        {
            lump_t lump = lumps[2];
            dtexdata_t[] textureData = new dtexdata_t[lump.filelen / 32];
            stream.Position = lump.fileofs;

            for (int i = 0; i < textureData.Length; i++)
            {
                if (cancelSource != null && cancelSource.IsCancellationRequested)
                    return null;

                Vector3 reflectivity = new Vector3(DataParser.ReadFloat(stream), DataParser.ReadFloat(stream), DataParser.ReadFloat(stream));
                textureData[i].reflectivity = reflectivity;
                textureData[i].nameStringTableID = DataParser.ReadInt(stream);
                textureData[i].width = DataParser.ReadInt(stream);
                textureData[i].height = DataParser.ReadInt(stream);
                textureData[i].view_width = DataParser.ReadInt(stream);
                textureData[i].view_height = DataParser.ReadInt(stream);
            }

            lumpData[2] = textureData;
            return textureData;
        }

        private int[] GetTextureStringTable(Stream stream, CancellationTokenSource cancelSource = null)
        {
            lump_t lump = lumps[44];
            int[] textureStringTable = new int[lump.filelen / 4];
            stream.Position = lump.fileofs;

            for (int i = 0; i < textureStringTable.Length; i++)
            {
                if (cancelSource != null && cancelSource.IsCancellationRequested)
                    return null;

                textureStringTable[i] = DataParser.ReadInt(stream);
            }

            return textureStringTable;
        }

        private List<string> GetTextureStringData(Stream stream, CancellationTokenSource cancelSource = null)
        {
            lump_t lump = lumps[43];
            stream.Position = lump.fileofs;

            List<string> textureStringData = new List<string>();
            while (stream.Position < lump.fileofs + lump.filelen)
            {
                if (cancelSource != null && cancelSource.IsCancellationRequested)
                    return null;

                textureStringData.Add(DataParser.ReadNullTerminatedString(stream));
            }
            return textureStringData;
        }

        private StaticProps_t GetStaticProps(Stream stream, CancellationTokenSource cancelSource = null)
        {
            dgamelump_t lump = null;

            //Debug.Log("# Game Lumps: " + gameLumpHeader.gamelump.Length);
            for (int i = 0; i < gameLumpHeader.gamelump.Length; i++)
            {
                if (cancelSource != null && cancelSource.IsCancellationRequested)
                    return null;

                //Debug.Log("Static Prop Dict Index: " + i + " id: " + gameLumpHeader.gamelump[i].id + " fileofs: " + gameLumpHeader.gamelump[i].fileofs + " filelen: " + gameLumpHeader.gamelump[i].filelen + " version: " + gameLumpHeader.gamelump[i].version);
                if (gameLumpHeader.gamelump[i].id == 1936749168) { lump = gameLumpHeader.gamelump[i]; }
            }

            StaticProps_t staticProps = new StaticProps_t();
            //staticProp.staticPropDict = new StaticPropDictLump_t();
            if (lump != null)
            {
                stream.Position = lump.fileofs;

                #region Dict Lump
                staticProps.staticPropDict.dictEntries = DataParser.ReadInt(stream);
                staticProps.staticPropDict.names = new string[staticProps.staticPropDict.dictEntries];

                for (int i = 0; i < staticProps.staticPropDict.names.Length; i++)
                {
                    if (cancelSource != null && cancelSource.IsCancellationRequested)
                        return null;

                    char[] nullPaddedName = new char[128];
                    for (int j = 0; j < nullPaddedName.Length; j++)
                    {
                        nullPaddedName[j] = DataParser.ReadChar(stream);
                    }
                    staticProps.staticPropDict.names[i] = new string(nullPaddedName);
                    //Debug.Log(i + ": " + staticProps.staticPropDict.names[i]);
                }
                #endregion

                #region Leaf Lump
                staticProps.staticPropLeaf.leafEntries = DataParser.ReadInt(stream);
                staticProps.staticPropLeaf.leaf = new ushort[staticProps.staticPropLeaf.leafEntries];

                for (int i = 0; i < staticProps.staticPropLeaf.leaf.Length; i++)
                {
                    if (cancelSource != null && cancelSource.IsCancellationRequested)
                        return null;

                    staticProps.staticPropLeaf.leaf[i] = DataParser.ReadUShort(stream);
                }
                //Debug.Log("Leaf Entries: " + staticProps.staticPropLeaf.leaf.Length);
                #endregion

                #region Info Lump
                staticProps.staticPropInfo = new StaticPropLump_t[DataParser.ReadInt(stream)];
                //long currentSizeUsed = stream.Position - lump.fileofs;
                //Debug.Log("Used: " + currentSizeUsed + " Intended Length: " + lump.filelen + " BytesPerInfo: " + ((lump.filelen - currentSizeUsed) / staticProps.staticPropInfo.Length));
                //int largestIndex = -1;
                for (int i = 0; i < staticProps.staticPropInfo.Length; i++)
                {
                    if (cancelSource != null && cancelSource.IsCancellationRequested)
                        return null;

                    staticProps.staticPropInfo[i].Origin = new Vector3(DataParser.ReadFloat(stream), DataParser.ReadFloat(stream), DataParser.ReadFloat(stream));       // origin
                    staticProps.staticPropInfo[i].Origin = new Vector3(staticProps.staticPropInfo[i].Origin.x, staticProps.staticPropInfo[i].Origin.z, staticProps.staticPropInfo[i].Origin.y);
                    staticProps.staticPropInfo[i].Angles = new Vector3(DataParser.ReadFloat(stream), DataParser.ReadFloat(stream), DataParser.ReadFloat(stream));       // orientation (pitch roll yaw)
                                                                                                                                                                        //staticProps.staticPropInfo[i].Angles = new Vector3(staticProps.staticPropInfo[i].Angles.x, staticProps.staticPropInfo[i].Angles.z, staticProps.staticPropInfo[i].Angles.y);
                    staticProps.staticPropInfo[i].PropType = DataParser.ReadUShort(stream);     // index into model name dictionary
                    staticProps.staticPropInfo[i].FirstLeaf = DataParser.ReadUShort(stream);    // index into leaf array
                    staticProps.staticPropInfo[i].LeafCount = DataParser.ReadUShort(stream);
                    staticProps.staticPropInfo[i].Solid = DataParser.ReadByte(stream);         // solidity type
                    staticProps.staticPropInfo[i].Flags = DataParser.ReadByte(stream);
                    staticProps.staticPropInfo[i].Skin = DataParser.ReadInt(stream);        // model skin numbers
                    staticProps.staticPropInfo[i].FadeMinDist = DataParser.ReadFloat(stream);
                    staticProps.staticPropInfo[i].FadeMaxDist = DataParser.ReadFloat(stream);
                    staticProps.staticPropInfo[i].LightingOrigin = new Vector3(DataParser.ReadFloat(stream), DataParser.ReadFloat(stream), DataParser.ReadFloat(stream));  // for lighting
                                                                                                                                                                           // since v5
                    staticProps.staticPropInfo[i].ForcedFadeScale = DataParser.ReadFloat(stream); // fade distance scale
                                                                                                  // v6 and v7 only
                    staticProps.staticPropInfo[i].MinDXLevel = DataParser.ReadUShort(stream);      // minimum DirectX version to be visible
                    staticProps.staticPropInfo[i].MaxDXLevel = DataParser.ReadUShort(stream);      // maximum DirectX version to be visible
                                                                                                   // since v8
                    staticProps.staticPropInfo[i].MinCPULevel = DataParser.ReadByte(stream);
                    staticProps.staticPropInfo[i].MaxCPULevel = DataParser.ReadByte(stream);
                    staticProps.staticPropInfo[i].MinGPULevel = DataParser.ReadByte(stream);
                    staticProps.staticPropInfo[i].MaxGPULevel = DataParser.ReadByte(stream);
                    // since v7
                    staticProps.staticPropInfo[i].DiffuseModulation = new Color32(DataParser.ReadByte(stream), DataParser.ReadByte(stream), DataParser.ReadByte(stream), DataParser.ReadByte(stream)); // per instance color and alpha modulation
                                                                                                                                                                                                       // since v10
                    staticProps.staticPropInfo[i].unknown = DataParser.ReadFloat(stream);
                    // since v9
                    //staticProps.staticPropInfo[i].DisableX360 = Convert.ToBoolean(FileReader.readByte(stream));     // if true, don't show on XBox 360

                    //largestIndex = staticProps.staticPropInfo[i].PropType > largestIndex ? staticProps.staticPropInfo[i].PropType : largestIndex;

                    #region Full Debug
                    /*Debug.Log(i +
                        " Origin: " + staticProps.staticPropInfo[i].Origin +
                        " Angle: " + staticProps.staticPropInfo[i].Angles +
                        " Prop Type: " + staticProps.staticPropInfo[i].PropType +
                        " First Leaf: " + staticProps.staticPropInfo[i].FirstLeaf +
                        " Leaf Count: " + staticProps.staticPropInfo[i].LeafCount + 
                        " Solid: " + staticProps.staticPropInfo[i].Solid +
                        " Flags: " + staticProps.staticPropInfo[i].Flags +
                        " Skin: " + staticProps.staticPropInfo[i].Skin +
                        " FadeMinDist: " + staticProps.staticPropInfo[i].FadeMinDist +
                        " FadeMaxDist: " + staticProps.staticPropInfo[i].FadeMaxDist +
                        " LightingOrigin: " + staticProps.staticPropInfo[i].LightingOrigin +
                        " ForcedFadeScale: " + staticProps.staticPropInfo[i].ForcedFadeScale +
                        " MinDXLevel: " + staticProps.staticPropInfo[i].MinDXLevel +
                        " MaxDXLevel: " + staticProps.staticPropInfo[i].MaxDXLevel +
                        " MinCPULevel: " + staticProps.staticPropInfo[i].MinCPULevel +
                        " MaxCPULevel: " + staticProps.staticPropInfo[i].MaxCPULevel +
                        " MinGPULevel: " + staticProps.staticPropInfo[i].MinGPULevel +
                        " MaxGPULevel: " + staticProps.staticPropInfo[i].MaxGPULevel +
                        " DiffuseModulation: " + staticProps.staticPropInfo[i].DiffuseModulation +
                        " Unknown: " + staticProps.staticPropInfo[i].unknown +
                        " DisableX360: " + staticProps.staticPropInfo[i].DisableX360);*/
                    #endregion
                }
                //Debug.Log("Total Static Props: " + staticProps.staticPropInfo.Length + " Largest index into dict: " + largestIndex);
                #endregion
            }

            return staticProps;
        }
    }
}