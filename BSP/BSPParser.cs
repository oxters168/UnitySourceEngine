using UnityEngine;
using System.IO;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Linq;

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

        private Stream bspStream;

        #region Geometry
        public Vector3[] vertices;
        public dedge_t[] edges;
        public dface_t[] faces;
        public int[] surfedges;
        public dplane_t[] planes;
        public dnode_t[] nodes;
        public dleaf_t[] leaves;
        public ushort[] leaffaces;

        public ddispinfo_t[] dispInfo;
        public dDispVert[] dispVerts;

        public texinfo_t[] texInfo;
        public dtexdata_t[] texData;
        public int[] texStringTable;
        public List<string> textureStringData;

        public StaticProps_t staticProps;

        public ZIP_EndOfCentralDirRecord pakfileDirRecord;
        public Dictionary<string, ZIP_FileHeader> pakfiles;
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
                bspStream?.Dispose();

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

                pakfiles = null;
            }
        }

        private void LoadLumps(Stream stream, CancellationToken cancelToken)
        {
            for (int i = 0; i < lumps.Length; i++)
            {
                if (cancelToken.IsCancellationRequested)
                    return;

                lump_t lump = new lump_t();
                lump.fileofs = DataParser.ReadInt(stream);
                lump.filelen = DataParser.ReadInt(stream);
                lump.version = DataParser.ReadInt(stream);
                lump.fourCC = DataParser.ReadInt(stream);
                lumps[i] = lump;
            }
        }
        private void LoadGameLumps(Stream stream, CancellationToken cancelToken)
        {
            lump_t lump = lumps[35];
            stream.Position = lump.fileofs;

            //gameLumpHeader = new dgamelumpheader_t();
            dgamelumpheader_t gameLumpHeader = new dgamelumpheader_t();
            gameLumpHeader.lumpCount = DataParser.ReadInt(stream);
            gameLumpHeader.gamelump = new dgamelump_t[gameLumpHeader.lumpCount];

            for (int i = 0; i < gameLumpHeader.gamelump.Length; i++)
            {
                if (cancelToken.IsCancellationRequested)
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

        public void ParseData(CancellationToken cancelToken)
        {
            bspStream = new FileStream(fileLocation, FileMode.Open, FileAccess.Read);

            identifier = DataParser.ReadInt(bspStream);
            version = DataParser.ReadInt(bspStream);
            if (!cancelToken.IsCancellationRequested)
                LoadLumps(bspStream, cancelToken);
            if (!cancelToken.IsCancellationRequested)
                LoadGameLumps(bspStream, cancelToken);
            mapRevision = DataParser.ReadInt(bspStream);

            if (!cancelToken.IsCancellationRequested)
                vertices = GetVertices(bspStream, cancelToken);

            if (!cancelToken.IsCancellationRequested)
                edges = GetEdges(bspStream, cancelToken);
            if (!cancelToken.IsCancellationRequested)
                faces = GetFaces(bspStream, cancelToken);
            if (!cancelToken.IsCancellationRequested)
                surfedges = GetSurfedges(bspStream, cancelToken);
            if (!cancelToken.IsCancellationRequested)
                planes = GetPlanes(bspStream, cancelToken);

            if (!cancelToken.IsCancellationRequested)
                leaffaces = GetLeafFaces(bspStream, cancelToken);
            if (!cancelToken.IsCancellationRequested)
                nodes = GetNodes(bspStream, cancelToken);
            if (!cancelToken.IsCancellationRequested)
                leaves = GetLeaves(bspStream, cancelToken);

            if (!cancelToken.IsCancellationRequested)
                dispInfo = GetDispInfo(bspStream, cancelToken);
            if (!cancelToken.IsCancellationRequested)
                dispVerts = GetDispVerts(bspStream, cancelToken);

            if (!cancelToken.IsCancellationRequested)
                texInfo = GetTextureInfo(bspStream, cancelToken);
            if (!cancelToken.IsCancellationRequested)
                texData = GetTextureData(bspStream, cancelToken);
            if (!cancelToken.IsCancellationRequested)
                texStringTable = GetTextureStringTable(bspStream, cancelToken);
            if (!cancelToken.IsCancellationRequested)
                textureStringData = GetTextureStringData(bspStream, cancelToken);

            if (!cancelToken.IsCancellationRequested)
                staticProps = GetStaticProps(bspStream, cancelToken);

            if (!cancelToken.IsCancellationRequested)
                pakfileDirRecord = ReadPakfileEndOfCentralDirRecord(bspStream, cancelToken);
            if (!cancelToken.IsCancellationRequested)
                pakfiles = ReadPakFileHeaders(bspStream, cancelToken);
        }

        public dnode_t GetNodeContainingLeaf(ushort leafIndex)
        {
            return nodes.First(currentNode => (-currentNode.children[0] - 1) == leafIndex || (-currentNode.children[1] - 1) == leafIndex);
        }
        public Vector3 CombineCenters(dnode_t startNode, ushort traverseAmount)
        {
            Vector3 center = GetNodeCenter(startNode);

            var childNodes = GetTraversedNodesStartingFrom(startNode, traverseAmount);
            foreach (var childNode in childNodes)
                center += GetNodeCenter(childNode);

            center /= childNodes.Count + 1;

            return center;
        }
        public int GetChildNodeIndex(dnode_t node)
        {
            int childNodeIndex = -1;
            if (node.children[0] >= 0)
                childNodeIndex = 0;
            else if (node.children[1] >= 0)
                childNodeIndex = 1;

            return childNodeIndex;
        }
        public bool HasChildNode(dnode_t node)
        {
            return GetChildNodeIndex(node) >= 0;
        }
        public dnode_t GetNextNodeFrom(dnode_t fromNode)
        {
            dnode_t nextNode;
            int childNodeIndex = GetChildNodeIndex(fromNode);
            if (childNodeIndex >= 0)
                nextNode = nodes[fromNode.children[childNodeIndex]];
            else
                throw new InvalidOperationException("There is no next node");

            return nextNode;
        }
        public List<dnode_t> GetTraversedNodesStartingFrom(dnode_t startNode, ushort amount)
        {
            List<dnode_t> childNodes = new List<dnode_t>();
            if (amount > 0 && HasChildNode(startNode))
            {
                dnode_t nextNode = GetNextNodeFrom(startNode);
                childNodes.Add(nextNode);
                childNodes.AddRange(GetTraversedNodesStartingFrom(nextNode, (ushort)(amount - 1)));
            }
            return childNodes;
        }
        public dnode_t TraverseNodesFrom(dnode_t startNode, ushort amount)
        {
            dnode_t nextNode = startNode;
            if (amount > 0 && HasChildNode(startNode))
            {
                nextNode = GetNextNodeFrom(startNode);
                amount = (ushort)(amount - 1);
                if (amount > 0)
                    nextNode = TraverseNodesFrom(nextNode, amount);
            }
            return nextNode;
        }
        public Vector3 GetNodeCenter(dnode_t node)
        {
            Vector3 min = GetNodeMin(node);
            Vector3 max = GetNodeMax(node);
            return min + max / 2f;
        }
        public Vector3 GetNodeMin(dnode_t node)
        {
            return new Vector3(node.mins[0], node.mins[2], node.mins[1]);
        }
        public Vector3 GetNodeMax(dnode_t node)
        {
            return new Vector3(node.maxs[0], node.maxs[2], node.maxs[1]);
        }

        private string GetEntities(Stream stream, CancellationToken cancelToken)
        {
            lump_t lump = lumps[0];
            string allEntities = "";
            stream.Position = lump.fileofs;

            for (int i = 0; i < lump.filelen; i++)
            {
                if (cancelToken.IsCancellationRequested)
                    return null;

                char nextChar = DataParser.ReadChar(stream);
                allEntities += nextChar;
            }

            return allEntities;
        }

        private dbrush_t[] GetBrushes(Stream stream, CancellationToken cancelToken)
        {
            lump_t lump = lumps[18];
            dbrush_t[] brushes = new dbrush_t[lump.filelen / 12];
            stream.Position = lump.fileofs;

            for (int i = 0; i < brushes.Length; i++)
            {
                if (cancelToken.IsCancellationRequested)
                    return null;

                brushes[i].firstside = DataParser.ReadInt(stream);
                brushes[i].numsides = DataParser.ReadInt(stream);
                brushes[i].contents = DataParser.ReadInt(stream);
            }

            lumpData[18] = brushes;
            return brushes;
        }

        private dbrushside_t[] GetBrushSides(Stream stream, CancellationToken cancelToken)
        {
            lump_t lump = lumps[19];
            dbrushside_t[] brushSides = new dbrushside_t[lump.filelen / 8];
            stream.Position = lump.fileofs;

            for (int i = 0; i < brushSides.Length; i++)
            {
                if (cancelToken.IsCancellationRequested)
                    return null;

                brushSides[i].planenum = DataParser.ReadUShort(stream);
                brushSides[i].texinfo = DataParser.ReadShort(stream);
                brushSides[i].dispinfo = DataParser.ReadShort(stream);
                brushSides[i].bevel = DataParser.ReadShort(stream);
            }

            lumpData[19] = brushSides;
            return brushSides;
        }

        private ddispinfo_t[] GetDispInfo(Stream stream, CancellationToken cancelToken)
        {
            lump_t lump = lumps[26];
            ddispinfo_t[] displacementInfo = new ddispinfo_t[lump.filelen / 86];
            stream.Position = lump.fileofs;

            for (int i = 0; i < displacementInfo.Length; i++)
            {
                if (cancelToken.IsCancellationRequested)
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

        private dDispVert[] GetDispVerts(Stream stream, CancellationToken cancelToken)
        {
            lump_t lump = lumps[33];
            dDispVert[] displacementVertices = new dDispVert[lump.filelen / 20];
            stream.Position = lump.fileofs;

            for (int i = 0; i < displacementVertices.Length; i++)
            {
                if (cancelToken.IsCancellationRequested)
                    return null;

                displacementVertices[i].vec = new Vector3(DataParser.ReadFloat(stream), DataParser.ReadFloat(stream), DataParser.ReadFloat(stream));
                displacementVertices[i].dist = DataParser.ReadFloat(stream);
                displacementVertices[i].alpha = DataParser.ReadFloat(stream);
            }

            lumpData[33] = displacementVertices;
            return displacementVertices;
        }

        private dedge_t[] GetEdges(Stream stream, CancellationToken cancelToken)
        {
            lump_t lump = lumps[12];
            dedge_t[] edges = new dedge_t[lump.filelen / 4];
            stream.Position = lump.fileofs;

            for (int i = 0; i < edges.Length; i++)
            {
                if (cancelToken.IsCancellationRequested)
                    return null;

                edges[i].v = new ushort[2];
                edges[i].v[0] = DataParser.ReadUShort(stream);
                edges[i].v[1] = DataParser.ReadUShort(stream);
            }

            lumpData[12] = edges;
            return edges;
        }

        private Vector3[] GetVertices(Stream stream, CancellationToken cancelToken)
        {
            lump_t lump = lumps[3];
            Vector3[] vertices = new Vector3[lump.filelen / 12];
            stream.Position = lump.fileofs;

            for (int i = 0; i < vertices.Length; i++)
            {
                if (cancelToken.IsCancellationRequested)
                    return null;

                vertices[i] = new Vector3(DataParser.ReadFloat(stream), DataParser.ReadFloat(stream), DataParser.ReadFloat(stream));
            }

            lumpData[3] = vertices;
            return vertices;
        }

        private dface_t[] GetOriginalFaces(Stream stream, CancellationToken cancelToken)
        {
            lump_t lump = lumps[27];
            dface_t[] faces = new dface_t[lump.filelen / 56];
            stream.Position = lump.fileofs;

            for (int i = 0; i < faces.Length; i++)
            {
                if (cancelToken.IsCancellationRequested)
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

        private dface_t[] GetFaces(Stream stream, CancellationToken cancelToken)
        {
            lump_t lump = lumps[7];
            dface_t[] faces = new dface_t[lump.filelen / 56];
            stream.Position = lump.fileofs;

            for (int i = 0; i < faces.Length; i++)
            {
                if (cancelToken.IsCancellationRequested)
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

        public Dictionary<int, SourceLightmap> GetLightmaps(CancellationToken cancelToken)
        {
            lump_t lump;
            if (version < 20)
                lump = lumps[8];
            else
                lump = lumps[53];

            Dictionary<int, SourceLightmap> lightmaps = new Dictionary<int, SourceLightmap>();

            /*int samples = lump.filelen / 4;
            int textureSize = (int)Mathf.Sqrt(samples);
            Debug.Log("BSPParser: Lightmap Lump Length = " + lump.filelen + " Total Samples = " + samples + " Lightmap Dimension Size = " + textureSize);

            SourceLightmap currentLightmap = new SourceLightmap();
            currentLightmap.width = textureSize;
            currentLightmap.height = textureSize;
            currentLightmap.lightmapColors = new Color[samples];
            for (int colorIndex = 0; colorIndex < currentLightmap.lightmapColors.Length; colorIndex++)
            {
                if (cancelToken.IsCancellationRequested)
                    return null;

                Color currentColor;

                if (version < 20)
                {
                    byte r = DataParser.ReadByte(bspStream);
                    byte g = DataParser.ReadByte(bspStream);
                    byte b = DataParser.ReadByte(bspStream);
                    sbyte exponent = DataParser.ReadSByte(bspStream);
                    currentColor = ColorRGBExp32.GetColor(r, g, b, exponent);
                }
                else
                {
                    byte r = DataParser.ReadByte(bspStream);
                    byte g = DataParser.ReadByte(bspStream);
                    byte b = DataParser.ReadByte(bspStream);
                    byte a = DataParser.ReadByte(bspStream);
                    currentColor = new Color(r / 255f, g / 255f, b / 255f, a / 255f);
                }

                currentLightmap.lightmapColors[colorIndex] = currentColor;
            }
            lightmaps[0] = currentLightmap;*/

            foreach (var face in faces)
            {
                if (cancelToken.IsCancellationRequested)
                    return null;

                int stylesCount = 0;
                for (int i = 0; i < face.styles.Length; i++)
                    if (face.styles[i] < byte.MaxValue)
                        stylesCount++;
                int luxelCount = (face.LightmapTextureSizeInLuxels[0] + 1) * (face.LightmapTextureSizeInLuxels[1] + 1);
                int numberOfSamples = luxelCount * stylesCount;
                int textureSize = Mathf.CeilToInt(Mathf.Sqrt(numberOfSamples));
                Debug.Log("BSPParser: Reading lightmap LuxelCount(" + luxelCount + ") Styles(" + stylesCount + ") Samples(" + numberOfSamples + ") Dimensions(" + textureSize + ", " + textureSize + ") Mins(" + face.LightmapTextureMinsInLuxels[0] + ", " + face.LightmapTextureMinsInLuxels[1] + ")");

                if (face.lightofs >= 0 && stylesCount > 0 && !lightmaps.ContainsKey(face.lightofs))
                {
                    bspStream.Position = lump.fileofs + face.lightofs;
                    //int width = face.LightmapTextureSizeInLuxels[0] + 1;
                    //int height = face.LightmapTextureSizeInLuxels[1] + 1;

                    //for (int styleIndex = 0; styleIndex < face.styles.Length; styleIndex++)
                    //{
                        //if (cancelToken.IsCancellationRequested)
                        //    return null;

                        //if (face.styles[0] < byte.MaxValue)
                        //{
                            SourceLightmap currentLightmap = new SourceLightmap();
                            currentLightmap.width = textureSize;
                            currentLightmap.height = textureSize;
                            currentLightmap.lightmapColors = new Color[numberOfSamples];
                            for (int colorIndex = 0; colorIndex < currentLightmap.lightmapColors.Length; colorIndex++)
                            {
                                if (cancelToken.IsCancellationRequested)
                                    return null;

                                Color currentColor;
                                if (version < 20)
                                {
                                    byte r = DataParser.ReadByte(bspStream);
                                    byte g = DataParser.ReadByte(bspStream);
                                    byte b = DataParser.ReadByte(bspStream);
                                    sbyte exponent = DataParser.ReadSByte(bspStream);
                                    currentColor = ColorRGBExp32.GetColor(r, g, b, exponent);
                                }
                                else
                                {
                                    byte r = DataParser.ReadByte(bspStream);
                                    byte g = DataParser.ReadByte(bspStream);
                                    byte b = DataParser.ReadByte(bspStream);
                                    byte a = DataParser.ReadByte(bspStream);
                                    currentColor = new Color(r / 255f, g / 255f, b / 255f);
                                }
                                currentLightmap.lightmapColors[colorIndex] = currentColor;
                            }
                            lightmaps[face.lightofs] = currentLightmap;
                        //}
                    //}
                }
            }
            return lightmaps;
        }

        private dplane_t[] GetPlanes(Stream stream, CancellationToken cancelToken)
        {
            lump_t lump = lumps[1];
            dplane_t[] planes = new dplane_t[lump.filelen / 20];
            stream.Position = lump.fileofs;

            for (int i = 0; i < planes.Length; i++)
            {
                if (cancelToken.IsCancellationRequested)
                    return null;

                planes[i].normal = new Vector3(DataParser.ReadFloat(stream), DataParser.ReadFloat(stream), DataParser.ReadFloat(stream));
                planes[i].dist = DataParser.ReadFloat(stream);
                planes[i].type = DataParser.ReadInt(stream);
            }

            lumpData[1] = planes;
            return planes;
        }

        private ushort[] GetLeafFaces(Stream stream, CancellationToken cancelToken)
        {
            lump_t lump = lumps[16];

            stream.Position = lump.fileofs;
            ushort[] leaffaces = new ushort[lump.filelen / 2];

            for (int i = 0; i < leaffaces.Length; i++)
            {
                if (cancelToken.IsCancellationRequested)
                    return null;

                leaffaces[i] = DataParser.ReadUShort(stream);
            }

            //Debug.Log("BSP Version: " + version + " Leaf Faces Lump Start: " + lump.fileofs + " Current Position: " + stream.Position + " Leaf Faces Lump length: " + lump.filelen);

            return leaffaces;
        }

        private dnode_t[] GetNodes(Stream stream, CancellationToken cancelToken)
        {
            lump_t lump = lumps[5];

            stream.Position = lump.fileofs;
            dnode_t[] nodes = new dnode_t[lump.filelen / 32];

            for (int i = 0; i < nodes.Length; i++)
            {
                if (cancelToken.IsCancellationRequested)
                    return null;

                nodes[i].planenum = DataParser.ReadInt(stream); //0 + 4 = 4
                nodes[i].children = new int[2];
                nodes[i].children[0] = DataParser.ReadInt(stream); //4 + 4 = 8
                nodes[i].children[1] = DataParser.ReadInt(stream); //8 + 4 = 12
                nodes[i].mins = new short[3];
                nodes[i].mins[0] = DataParser.ReadShort(stream); //12 + 2 = 14
                nodes[i].mins[1] = DataParser.ReadShort(stream); //14 + 2 = 16
                nodes[i].mins[2] = DataParser.ReadShort(stream); //16 + 2 = 18
                nodes[i].maxs = new short[3];
                nodes[i].maxs[0] = DataParser.ReadShort(stream); //18 + 2 = 20
                nodes[i].maxs[1] = DataParser.ReadShort(stream); //20 + 2 = 22
                nodes[i].maxs[2] = DataParser.ReadShort(stream); //22 + 2 = 24
                nodes[i].firstface = DataParser.ReadUShort(stream); //24 + 2 = 26
                nodes[i].numfaces = DataParser.ReadUShort(stream); //26 + 2 = 28
                nodes[i].area = DataParser.ReadShort(stream); //28 + 2 = 30
                stream.Position += 2;
            }

            //Debug.Log("BSP Version: " + version + " Nodes Lump Start: " + lump.fileofs + " Current Position: " + stream.Position + " Nodes Lump length: " + lump.filelen);

            return nodes;
        }

        private dleaf_t[] GetLeaves(Stream stream, CancellationToken cancelToken)
        {
            lump_t lump = lumps[10];

            int leafRawBytesAmount = version >= 17 ? 32 : 56;
            stream.Position = lump.fileofs;
            dleaf_t[] leaves = new dleaf_t[lump.filelen / leafRawBytesAmount];

            for (int i = 0; i < leaves.Length; i++)
            {
                if (cancelToken.IsCancellationRequested)
                    return null;

                leaves[i].contents = DataParser.ReadInt(stream); //0 + 4 = 4
                leaves[i].cluster = DataParser.ReadShort(stream); //4 + 2 = 6
                //short sharedAreaFlagsValue = DataParser.ReadShort(stream);
                //leaves[i].area = (short)(sharedAreaFlagsValue & 511); //511 = 0000 0001 1111 1111
                //leaves[i].flags = (short)((sharedAreaFlagsValue >> 9) & 127); //127 = 0000 0000 0111 1111
                leaves[i].area = DataParser.ReadShort(stream); //6 + 2 = 8
                leaves[i].flags = DataParser.ReadShort(stream); //8 + 2 = 10
                leaves[i].mins = new short[3];
                leaves[i].mins[0] = DataParser.ReadShort(stream); //10 + 2 = 12
                leaves[i].mins[1] = DataParser.ReadShort(stream); //12 + 2 = 14
                leaves[i].mins[2] = DataParser.ReadShort(stream); //14 + 2 = 16
                leaves[i].maxs = new short[3];
                leaves[i].maxs[0] = DataParser.ReadShort(stream); //16 + 2 = 18
                leaves[i].maxs[1] = DataParser.ReadShort(stream); //18 + 2 = 20
                leaves[i].maxs[2] = DataParser.ReadShort(stream); //20 + 2 = 22
                leaves[i].firstleafface = DataParser.ReadUShort(stream); //22 + 2 = 24
                leaves[i].numleaffaces = DataParser.ReadUShort(stream); //24 + 2 = 26
                leaves[i].firstleafbrush = DataParser.ReadUShort(stream); //26 + 2 = 28
                leaves[i].numleafbrushes = DataParser.ReadUShort(stream); //28 + 2 = 30
                leaves[i].leafWaterDataID = DataParser.ReadShort(stream); //30 + 2 = 32

                if (version < 17)
                    stream.Position += 24;
            }

            //Debug.Log("BSP Version: " + version + " Leaves Lump Start: " + lump.fileofs + " Current Position: " + stream.Position + " Leaves Lump length: " + lump.filelen);

            return leaves;
        }

        private int[] GetSurfedges(Stream stream, CancellationToken cancelToken)
        {

            lump_t lump = lumps[13];
            int[] surfedges = new int[lump.filelen / 4];
            stream.Position = lump.fileofs;

            for (int i = 0; i < lump.filelen / 4; i++)
            {
                if (cancelToken.IsCancellationRequested)
                    return null;

                surfedges[i] = DataParser.ReadInt(stream);
            }

            lumpData[13] = surfedges;
            return surfedges;
        }

        private texinfo_t[] GetTextureInfo(Stream stream, CancellationToken cancelToken)
        {
            lump_t lump = lumps[6];
            texinfo_t[] textureInfo = new texinfo_t[lump.filelen / 72];
            stream.Position = lump.fileofs;

            for (int i = 0; i < textureInfo.Length; i++)
            {
                if (cancelToken.IsCancellationRequested)
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

        private dtexdata_t[] GetTextureData(Stream stream, CancellationToken cancelToken)
        {
            lump_t lump = lumps[2];
            dtexdata_t[] textureData = new dtexdata_t[lump.filelen / 32];
            stream.Position = lump.fileofs;

            for (int i = 0; i < textureData.Length; i++)
            {
                if (cancelToken.IsCancellationRequested)
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

        private int[] GetTextureStringTable(Stream stream, CancellationToken cancelToken)
        {
            lump_t lump = lumps[44];
            int[] textureStringTable = new int[lump.filelen / 4];
            stream.Position = lump.fileofs;

            for (int i = 0; i < textureStringTable.Length; i++)
            {
                if (cancelToken.IsCancellationRequested)
                    return null;

                textureStringTable[i] = DataParser.ReadInt(stream);
            }

            return textureStringTable;
        }

        private List<string> GetTextureStringData(Stream stream, CancellationToken cancelToken)
        {
            lump_t lump = lumps[43];
            stream.Position = lump.fileofs;

            List<string> textureStringData = new List<string>();
            while (stream.Position < lump.fileofs + lump.filelen)
            {
                if (cancelToken.IsCancellationRequested)
                    return null;

                textureStringData.Add(DataParser.ReadNullTerminatedString(stream));
            }
            return textureStringData;
        }

        private StaticProps_t GetStaticProps(Stream stream, CancellationToken cancelToken)
        {
            dgamelump_t gameLump = null;

            int staticPropsGameLumpId = GetGameLumpIdFromString("sprp");
            //Debug.Log("# Game Lumps: " + gameLumpHeader.gamelump.Length);
            for (int i = 0; i < gameLumpHeader.gamelump.Length; i++)
            {
                if (cancelToken.IsCancellationRequested)
                    return null;

                //Debug.Log("Static Prop Dict Index: " + i + " id: " + gameLumpHeader.gamelump[i].id + " fileofs: " + gameLumpHeader.gamelump[i].fileofs + " filelen: " + gameLumpHeader.gamelump[i].filelen + " version: " + gameLumpHeader.gamelump[i].version);
                if (gameLumpHeader.gamelump[i].id == staticPropsGameLumpId)
                {
                    gameLump = gameLumpHeader.gamelump[i];
                }
            }

            StaticProps_t staticProps = new StaticProps_t();
            //staticProp.staticPropDict = new StaticPropDictLump_t();
            if (gameLump != null)
            {
                stream.Position = gameLump.fileofs;

                #region Dict Lump
                staticProps.staticPropDict.dictEntries = DataParser.ReadInt(stream);
                staticProps.staticPropDict.names = new string[staticProps.staticPropDict.dictEntries];

                for (int i = 0; i < staticProps.staticPropDict.names.Length; i++)
                {
                    if (cancelToken.IsCancellationRequested)
                        return null;

                    char[] nullPaddedName = new char[128];
                    for (int j = 0; j < nullPaddedName.Length; j++)
                    {
                        nullPaddedName[j] = DataParser.ReadChar(stream);
                    }
                    staticProps.staticPropDict.names[i] = new string(nullPaddedName).Replace("\0", "");
                    //Debug.Log(i + ": " + staticProps.staticPropDict.names[i]);
                }
                #endregion

                #region Leaf Lump
                staticProps.staticPropLeaf.leafEntries = DataParser.ReadInt(stream);
                staticProps.staticPropLeaf.leaf = new ushort[staticProps.staticPropLeaf.leafEntries];

                for (int i = 0; i < staticProps.staticPropLeaf.leaf.Length; i++)
                {
                    if (cancelToken.IsCancellationRequested)
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
                    if (cancelToken.IsCancellationRequested)
                        return null;

                    if (gameLump.version >= 4)
                    {
                        float posX = DataParser.ReadFloat(stream);
                        float posY = DataParser.ReadFloat(stream);
                        float posZ = DataParser.ReadFloat(stream);
                        staticProps.staticPropInfo[i].Origin = new Vector3(posX, posY, posZ); // origin

                        //Working: pitch roll yaw | yaw roll pitch
                        //Tested: pitch roll yaw | yaw roll pitch
                        //Combinations: pitch roll yaw | pitch yaw roll | roll pitch yaw | roll yaw pitch | yaw pitch roll | yaw roll pitch
                        float pitch = DataParser.ReadFloat(stream);
                        float roll = DataParser.ReadFloat(stream);
                        float yaw = DataParser.ReadFloat(stream);
                        staticProps.staticPropInfo[i].Angles = new QAngle(pitch, roll, yaw); // orientation

                        staticProps.staticPropInfo[i].PropType = DataParser.ReadUShort(stream); // index into model name dictionary
                        staticProps.staticPropInfo[i].FirstLeaf = DataParser.ReadUShort(stream); // index into leaf array
                        staticProps.staticPropInfo[i].LeafCount = DataParser.ReadUShort(stream);
                        staticProps.staticPropInfo[i].Solid = DataParser.ReadByte(stream); // solidity type
                        staticProps.staticPropInfo[i].Flags = DataParser.ReadByte(stream);
                        staticProps.staticPropInfo[i].Skin = DataParser.ReadInt(stream); // model skin numbers
                        staticProps.staticPropInfo[i].FadeMinDist = DataParser.ReadFloat(stream);
                        staticProps.staticPropInfo[i].FadeMaxDist = DataParser.ReadFloat(stream);
                        staticProps.staticPropInfo[i].LightingOrigin = new Vector3(DataParser.ReadFloat(stream), DataParser.ReadFloat(stream), DataParser.ReadFloat(stream));  // for lighting
                        if (gameLump.version >= 5)
                        {
                            // since v5
                            staticProps.staticPropInfo[i].ForcedFadeScale = DataParser.ReadFloat(stream); // fade distance scale
                            if (gameLump.version >= 6)
                            {
                                // v6 and v7 only
                                staticProps.staticPropInfo[i].MinDXLevel = DataParser.ReadUShort(stream); // minimum DirectX version to be visible
                                staticProps.staticPropInfo[i].MaxDXLevel = DataParser.ReadUShort(stream); // maximum DirectX version to be visible
                                if (gameLump.version >= 7)
                                {
                                    // since v7
                                    staticProps.staticPropInfo[i].DiffuseModulation = new Color32(DataParser.ReadByte(stream), DataParser.ReadByte(stream), DataParser.ReadByte(stream), DataParser.ReadByte(stream)); // per instance color and alpha modulation
                                    if (gameLump.version >= 8)
                                    {
                                        // since v8
                                        staticProps.staticPropInfo[i].MinCPULevel = DataParser.ReadByte(stream);
                                        staticProps.staticPropInfo[i].MaxCPULevel = DataParser.ReadByte(stream);
                                        staticProps.staticPropInfo[i].MinGPULevel = DataParser.ReadByte(stream);
                                        staticProps.staticPropInfo[i].MaxGPULevel = DataParser.ReadByte(stream);
                                        if (gameLump.version >= 9)
                                        {
                                            // v9 and v10 only
                                            //bool DisableX360;       // if true, don't show on XBox 360 (4-bytes long)
                                            //stream.Position += 4; //This value does not seem to exist!
                                            if (gameLump.version >= 10)
                                            {
                                                // since v10
                                                staticProps.staticPropInfo[i].FlagsEx = DataParser.ReadUInt(stream); // Further bitflags.
                                                if (gameLump.version >= 11)
                                                {
                                                    // since v11
                                                    staticProps.staticPropInfo[i].UniformScale = DataParser.ReadFloat(stream); // Prop scale
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
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

            //Debug.Log("GameLump Version: " + gameLump.version + " GameLump Start: " + gameLump.fileofs + " Current Position: " + stream.Position + " GameLump length: " + gameLump.filelen);
            return staticProps;
        }

        public ZIP_EndOfCentralDirRecord ReadPakfileEndOfCentralDirRecord(Stream stream, CancellationToken cancelToken)
        {
            lump_t lump = lumps[40];
            //stream.Position = lump.fileofs + offset;

            ZIP_EndOfCentralDirRecord dirRecord = new ZIP_EndOfCentralDirRecord();

            int offset = lump.filelen - ZIP_EndOfCentralDirRecord.BYTE_LENGTH;
            bool foundSignature = false;
            for (; offset >= 0; offset--)
            {
                stream.Position = lump.fileofs + offset;
                uint signature = DataParser.ReadUInt(stream); //PK56
                foundSignature = signature == GetPKID(5, 6);
                if (foundSignature)
                {
                    dirRecord.signature = signature;
                    break;
                }
            }

            if (foundSignature)
            {
                dirRecord.numberOfThisDisk = DataParser.ReadUShort(stream);
                dirRecord.numberOfTheDiskWithStartOfCentralDirectory = DataParser.ReadUShort(stream);
                dirRecord.nCentralDirectoryEntries_ThisDisk = DataParser.ReadUShort(stream);
                dirRecord.nCentralDirectoryEntries_Total = DataParser.ReadUShort(stream);
                dirRecord.centralDirectorySize = DataParser.ReadUInt(stream);
                dirRecord.startOfCentralDirOffset = DataParser.ReadUInt(stream);
                dirRecord.commentLength = DataParser.ReadUShort(stream);

                byte[] commentBytes = new byte[dirRecord.commentLength];
                stream.Read(commentBytes, 0, commentBytes.Length);
                string comment = System.Text.Encoding.ASCII.GetString(commentBytes);
                //Debug.Log("Pakfile Comment: " + comment);
            }
            else
                Debug.LogWarning("BSPParser: Could not find pakfile");

            return dirRecord;
        }
        public Dictionary<string, ZIP_FileHeader> ReadPakFileHeaders(Stream stream, CancellationToken cancelToken)
        {
            lump_t lump = lumps[40];

            Dictionary<string, ZIP_FileHeader> zipFiles = new Dictionary<string, ZIP_FileHeader>();

            stream.Position = lump.fileofs + pakfileDirRecord.startOfCentralDirOffset;
            for (int i = 0; i < pakfileDirRecord.nCentralDirectoryEntries_Total; i++)
            {
                ZIP_FileHeader zipFile = new ZIP_FileHeader();
                zipFile.signature = DataParser.ReadUInt(stream);
                zipFile.versionMadeBy = DataParser.ReadUShort(stream);
                zipFile.versionNeededToExtract = DataParser.ReadUShort(stream);
                zipFile.flags = DataParser.ReadUShort(stream);
                zipFile.compressionMethod = DataParser.ReadUShort(stream);
                zipFile.lastModifiedTime = DataParser.ReadUShort(stream); 
                zipFile.lastModifiedDate = DataParser.ReadUShort(stream);
                zipFile.crc32 = DataParser.ReadUInt(stream);
                zipFile.compressedSize = DataParser.ReadUInt(stream);
                zipFile.uncompressedSize = DataParser.ReadUInt(stream);
                zipFile.fileNameLength = DataParser.ReadUShort(stream);
                zipFile.extraFieldLength = DataParser.ReadUShort(stream);
                zipFile.fileCommentLength = DataParser.ReadUShort(stream);
                zipFile.diskNumberStart = DataParser.ReadUShort(stream);
                zipFile.internalFileAttribs = DataParser.ReadUShort(stream);
                zipFile.externalFileAttribs = DataParser.ReadUInt(stream);
                zipFile.relativeOffsetOfLocalHeader = DataParser.ReadUInt(stream);

                byte[] fileNameBytes = new byte[zipFile.fileNameLength];
                stream.Read(fileNameBytes, 0, fileNameBytes.Length);
                string fileName = System.Text.Encoding.ASCII.GetString(fileNameBytes).Replace("\\", "/").ToLower();

                stream.Position += zipFile.extraFieldLength;

                byte[] fileCommentBytes = new byte[zipFile.fileCommentLength];
                stream.Read(fileCommentBytes, 0, fileCommentBytes.Length);
                string fileComment = System.Text.Encoding.ASCII.GetString(fileCommentBytes);

                Debug.Assert(zipFile.signature == GetPKID(1, 2), "BSPParser: Pakfile has incorrect signature expected PK12");
                Debug.Assert(zipFile.compressionMethod == 0, "BSPParser: Pakfile unknown compression method");
                //Debug.Log("Pakfile" + i + ": " + fileName + " " + fileComment);

                zipFiles[fileName] = zipFile;
            }
            return zipFiles;
        }
        public bool HasPakFile(string name)
        {
            return pakfiles.ContainsKey(name);
        }
        public ZIP_FileHeader GetPakFileHeader(string name)
        {
            return pakfiles[name];
        }
        public void LoadPakFileAsStream(string name, Action<Stream, long, uint> streamActions)
        {
            try
            {
                var fileHeader = GetPakFileHeader(name);

                if (!fileHeader.Equals(default(ZIP_FileHeader)))
                {
                    lump_t lump = lumps[40];

                    #region Manual Extraction (As long as there is no compression happening i.e. compressionMethod = 0)
                    bspStream.Position = lump.fileofs + fileHeader.relativeOffsetOfLocalHeader;

                    ZIP_LocalFileHeader localFileHeader = new ZIP_LocalFileHeader();
                    localFileHeader.signature = DataParser.ReadUInt(bspStream);
                    localFileHeader.versionNeededToExtract = DataParser.ReadUShort(bspStream);
                    localFileHeader.flags = DataParser.ReadUShort(bspStream);
                    localFileHeader.compressionMethod = DataParser.ReadUShort(bspStream);
                    localFileHeader.lastModifiedTime = DataParser.ReadUShort(bspStream);
                    localFileHeader.lastModifiedDate = DataParser.ReadUShort(bspStream);
                    localFileHeader.crc32 = DataParser.ReadUInt(bspStream);
                    localFileHeader.compressedSize = DataParser.ReadUInt(bspStream);
                    localFileHeader.uncompressedSize = DataParser.ReadUInt(bspStream);
                    localFileHeader.fileNameLength = DataParser.ReadUShort(bspStream);
                    localFileHeader.extraFieldLength = DataParser.ReadUShort(bspStream);

                    Debug.Assert(localFileHeader.compressionMethod == 0, "BSPParser: Pak file compression method is not 0");
                    Debug.Assert(localFileHeader.signature == GetPKID(3, 4), "BSPParser: Pakfile local header has incorrect signature expected PK34");

                    bspStream.Position += localFileHeader.fileNameLength;
                    //byte[] fileNameBytes = new byte[localFileHeader.fileNameLength];
                    //bspStream.Read(fileNameBytes, 0, fileNameBytes.Length);
                    //string fileName = System.Text.Encoding.ASCII.GetString(fileNameBytes);

                    bspStream.Position += localFileHeader.extraFieldLength;

                    streamActions(bspStream, bspStream.Position, localFileHeader.uncompressedSize);
                    #endregion
                }
                else
                    Debug.LogError("BSPParser: Could not find pak file " + name);
            }
            catch (Exception e)
            {
                Debug.LogError("BSPParser: " + e.ToString());
            }

        }
        public byte[] GetPakFile(string name)
        {
            byte[] pakfile = null;

            try
            {
                var fileHeader = GetPakFileHeader(name);

                if (!fileHeader.Equals(default(ZIP_FileHeader)))
                {
                    lump_t lump = lumps[40];

                    #region Through DotNetZip
                    /*stream.Position = lump.fileofs;
                    using (var copiedStream = new MemoryStream())
                    {
                        stream.CopyTo(copiedStream);
                        copiedStream.Position = 0;
                        using (var zipFile = Ionic.Zip.ZipFile.Read(copiedStream))
                        {
                            var zipEntry = zipFile[name];
                            using (var extractionStream = new MemoryStream())
                            {
                                zipEntry.Extract(extractionStream);
                                extractionStream.Position = 0;
                                pakfile = extractionStream.ToArray();
                            }
                        }
                    }*/
                    #endregion

                    #region Manual Extraction (As long as there is no compression happening i.e. compressionMethod = 0)
                    bspStream.Position = lump.fileofs + fileHeader.relativeOffsetOfLocalHeader;

                    ZIP_LocalFileHeader localFileHeader = new ZIP_LocalFileHeader();
                    localFileHeader.signature = DataParser.ReadUInt(bspStream);
                    localFileHeader.versionNeededToExtract = DataParser.ReadUShort(bspStream);
                    localFileHeader.flags = DataParser.ReadUShort(bspStream);
                    localFileHeader.compressionMethod = DataParser.ReadUShort(bspStream);
                    localFileHeader.lastModifiedTime = DataParser.ReadUShort(bspStream);
                    localFileHeader.lastModifiedDate = DataParser.ReadUShort(bspStream);
                    localFileHeader.crc32 = DataParser.ReadUInt(bspStream);
                    localFileHeader.compressedSize = DataParser.ReadUInt(bspStream);
                    localFileHeader.uncompressedSize = DataParser.ReadUInt(bspStream);
                    localFileHeader.fileNameLength = DataParser.ReadUShort(bspStream);
                    localFileHeader.extraFieldLength = DataParser.ReadUShort(bspStream);

                    Debug.Assert(localFileHeader.signature == GetPKID(3, 4), "BSPParser: Pakfile local header has incorrect signature expected PK34");

                    byte[] fileNameBytes = new byte[localFileHeader.fileNameLength];
                    bspStream.Read(fileNameBytes, 0, fileNameBytes.Length);
                    string fileName = System.Text.Encoding.ASCII.GetString(fileNameBytes);

                    bspStream.Position += localFileHeader.extraFieldLength;

                    pakfile = new byte[localFileHeader.uncompressedSize];
                    bspStream.Read(pakfile, 0, pakfile.Length);
                    #endregion
                }
                else
                    Debug.LogError("BSPParser: Could not find pak file " + name);
            }
            catch(Exception e)
            {
                Debug.LogError("BSPParser: " + e.ToString());
            }

            return pakfile;
        }
        public static uint GetPKID(uint a, uint b)
        {
            return ((b) << 24) | ((a) << 16) | ('K' << 8) | 'P';
        }
        public static int GetGameLumpIdFromString(string lumpName)
        {
            return BitConverter.ToInt32(System.Text.Encoding.ASCII.GetBytes(lumpName).Reverse().ToArray(), 0);
        }
    }
}