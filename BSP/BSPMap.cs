using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System;
using UnityHelpers;
using System.Threading;

namespace UnitySourceEngine
{
    public class BSPMap
    {
        public readonly string[] undesiredTextures = new string[] { "TOOLS/TOOLSAREAPORTAL", "TOOLS/TOOLSBLACK", "TOOLS/CLIMB", "TOOLS/CLIMB_ALPHA", "TOOLS/FOGVOLUME", "TOOLS/TOOLSAREAPORTAL-DX10", "TOOLS/TOOLSBLACK", "TOOLS/TOOLSBLOCK_LOS",
                "TOOLS/TOOLSBLOCK_LOS-DX10", "TOOLS/TOOLSBLOCKBOMB", "TOOLS/TOOLSBLOCKBULLETS", "TOOLS/TOOLSBLOCKBULLETS-DX10", "TOOLS/TOOLSBLOCKLIGHT", "TOOLS/TOOLSCLIP", "TOOLS/TOOLSCLIP-DX10", "TOOLS/TOOLSDOTTED", "TOOLS/TOOLSFOG", "TOOLS/TOOLSFOG-DX10",
                "TOOLS/TOOLSHINT", "TOOLS/TOOLSHINT-DX10", "TOOLS/TOOLSINVISIBLE", "TOOLS/TOOLSINVISIBLE-DX10", "TOOLS/TOOLSINVISIBLELADDER", "TOOLS/TOOLSNODRAW", "TOOLS/TOOLSNPCCLIP", "TOOLS/TOOLSOCCLUDER", "TOOLS/TOOLSOCCLUDER-DX10", "TOOLS/TOOLSORIGIN",
                "TOOLS/TOOLSPLAYERCLIP", "TOOLS/TOOLSPLAYERCLIP-DX10", "TOOLS/TOOLSSKIP", "TOOLS/TOOLSSKIP-DX10", "TOOLS/TOOLSSKYBOX2D", "TOOLS/TOOLSSKYFOG", "TOOLS/TOOLSTRIGGER", "TOOLS/TOOLSTRIGGER-DX10" };

        #region Map Variables
        public string mapName { get; private set; }
        public string mapDir { get; private set; }
        public static bool combineMeshesWithSameTexture;
        private static float modelLoadPercent = 1;
        public static float ModelLoadPercent { get { return Mathf.Clamp01(modelLoadPercent); } set { modelLoadPercent = value; } }
        private static float faceLoadPercent = 1;
        public static float FaceLoadPercent { get { return Mathf.Clamp01(faceLoadPercent); } set { faceLoadPercent = value; } }
        public static bool applyLightmaps;
        public static string vpkLoc;
        public GameObject gameObject { get; private set; }

        private List<FaceMesh> allFaces = new List<FaceMesh>();
        private StaticPropData[] staticProps;
        public Dictionary<int, SourceLightmap> lightmaps; //Maps from face lightofs to lightmap
        #endregion

        #region Feedback
        public bool isParsed { get; private set; }
        public bool isParsing { get; private set; }
        public bool isBuilt { get; private set; }
        public bool isBuilding { get; private set; }
        public string currentMessage { get; private set; }
        public float PercentLoaded { get { return (float)totalItemsLoaded / totalItemsToLoad; } }

        private int totalItemsLoaded = 0;
        private int totalItemsToLoad;
        #endregion

        public BSPMap(string _mapLocation)
        {
            mapDir = _mapLocation.Replace("\\", "/").ToLower();
            mapName = Path.GetFileNameWithoutExtension(mapDir);
            mapDir = Path.GetDirectoryName(mapDir);
        }

        public override bool Equals(object obj)
        {
            return obj != null && (obj is BSPMap) && mapName.Equals(((BSPMap)obj).mapName, StringComparison.OrdinalIgnoreCase);
        }
        public override int GetHashCode()
        {
            return -1521134295 + EqualityComparer<string>.Default.GetHashCode(mapName);
        }

        public void Unload()
        {
            isParsed = false;
            isParsing = false;
            isBuilt = false;
            isBuilding = false;
            currentMessage = string.Empty;

            if (staticProps != null)
                foreach (var prop in staticProps)
                    prop.model?.Dispose();
            staticProps = null;

            if (lightmaps != null)
                foreach (var lightmap in lightmaps)
                    lightmap.Value.Dispose();
            lightmaps = null;

            foreach (var face in allFaces)
                face?.Dispose();
            allFaces = new List<FaceMesh>();

            if (gameObject != null)
                UnityEngine.Object.Destroy(gameObject);
            gameObject = null;
        }

        public List<string> GetDependencies(CancellationToken cancelToken)
        {
            List<string> dependencies = new List<string>();
            using (VPKParser vpkParser = new VPKParser(vpkLoc))
            using (BSPParser bspParser = new BSPParser(Path.Combine(mapDir, mapName + ".bsp")))
            {
                bool validVPK = vpkParser.IsValid();
                if (!validVPK)
                    return null;

                bspParser.ParseData(cancelToken);

                if (cancelToken.IsCancellationRequested)
                    return null;

                //Note: If there are materials that point to textures in separate archives or there are textures used by the models whose vpk archive is not already
                //      added by other dependencies, those archives will not be added. That would require us to read the materials and models to get what textures they use.

                #region Map face textures dependencies
                if (FaceLoadPercent > 0)
                {
                    foreach (dface_t face in bspParser.faces)
                    {
                        if (cancelToken.IsCancellationRequested)
                            return null;

                        texflags currentTexFlags = GetFaceTextureFlags(face, bspParser);
                        string rawTextureLocation = GetFaceTextureLocation(face, bspParser);

                        if (!IsUndesiredTexture(rawTextureLocation, currentTexFlags))
                        {
                            string fixedLocation = VMTData.FixLocation(bspParser, vpkParser, rawTextureLocation);
                            if (!vpkParser.FileExists(fixedLocation))
                                fixedLocation = SourceTexture.FixLocation(bspParser, vpkParser, rawTextureLocation);

                            string dependency = vpkParser.LocateInArchive(fixedLocation);

                            if (!string.IsNullOrEmpty(dependency) && !dependencies.Contains(dependency))
                                dependencies.Add(dependency);
                        }
                    }
                }
                #endregion

                #region Model dependencies
                if (ModelLoadPercent > 0)
                {
                    for (int i = 0; i < bspParser.staticProps.staticPropInfo.Length; i++)
                    {
                        if (cancelToken.IsCancellationRequested)
                            return null;

                        var currentPropInfo = bspParser.staticProps.staticPropInfo[i];

                        ushort propType = currentPropInfo.PropType;
                        string modelFullPath = bspParser.staticProps.staticPropDict.names[propType];
                        modelFullPath = modelFullPath.Substring(0, modelFullPath.LastIndexOf("."));

                        string mdlPath = modelFullPath + ".mdl";
                        string vvdPath = modelFullPath + ".vvd";
                        string vtxPath = modelFullPath + ".vtx";

                        if (!vpkParser.FileExists(vtxPath))
                            vtxPath = modelFullPath + ".dx90.vtx";

                        string dependency = vpkParser.LocateInArchive(mdlPath);
                        if (!string.IsNullOrEmpty(dependency) && !dependencies.Contains(dependency))
                            dependencies.Add(dependency);
                        dependency = vpkParser.LocateInArchive(vvdPath);
                        if (!string.IsNullOrEmpty(dependency) && !dependencies.Contains(dependency))
                            dependencies.Add(dependency);
                        dependency = vpkParser.LocateInArchive(vtxPath);
                        if (!string.IsNullOrEmpty(dependency) && !dependencies.Contains(dependency))
                            dependencies.Add(dependency);
                    }
                }
                #endregion
            }
            return dependencies;
        }
        public void ParseFile(CancellationToken cancelToken, Action<float, string> onProgressChanged = null, Action onFinished = null)
        {
            isParsed = false;
            isParsing = true;

            //currentMessage = "Reading BSP Data";
            //onProgressChanged?.Invoke(PercentLoaded, currentMessage);

            using (VPKParser vpkParser = new VPKParser(vpkLoc))
            using (BSPParser bspParser = new BSPParser(Path.Combine(mapDir, mapName + ".bsp")))
            {
                bspParser.ParseData(cancelToken);
                //if (!cancelToken.IsCancellationRequested)
                //    lightmaps = bspParser.GetLightmaps(cancelToken);

                int facesCount = Mathf.RoundToInt(bspParser.faces.Length * FaceLoadPercent);
                int propsCount = Mathf.RoundToInt(bspParser.staticProps.staticPropInfo.Length * ModelLoadPercent);
                totalItemsToLoad = facesCount + propsCount;

                bool validVPK = vpkParser.IsValid();

                currentMessage = "Reading Map Faces";
                onProgressChanged?.Invoke(PercentLoaded, currentMessage);
                ReadFaces(bspParser, validVPK ? vpkParser : null, cancelToken, onProgressChanged);

                currentMessage = "Reading Map Models";
                onProgressChanged?.Invoke(PercentLoaded, currentMessage);
                if (validVPK)
                    ReadStaticProps(bspParser, vpkParser, cancelToken, onProgressChanged);
            }

            onFinished?.Invoke();

            isParsing = false;
            isParsed = true;
        }
        private texflags GetFaceTextureFlags(dface_t face, BSPParser bspParser)
        {
            texflags currentTexFlag = texflags.SURF_SKIP;
            if (bspParser.texInfo != null && face.texinfo < bspParser.texInfo.Length)
                currentTexFlag = (texflags)bspParser.texInfo[face.texinfo].flags;
            return currentTexFlag;
        }
        private string GetFaceTextureLocation(dface_t face, BSPParser bspParser)
        {
            int faceTexInfoIndex = face.texinfo;
            int texDataIndex = bspParser.texInfo[faceTexInfoIndex].texdata;
            int nameStringTableIndex = bspParser.texData[texDataIndex].nameStringTableID;
            return bspParser.textureStringData[nameStringTableIndex];
        }
        private bool IsUndesiredTexture(string textureLocation, texflags tf)
        {
            bool undesired = false;
            foreach (string undesiredTexture in undesiredTextures)
                if (textureLocation.Equals(undesiredTexture, StringComparison.OrdinalIgnoreCase))
                {
                    undesired = true;
                    break;
                }
            return undesired || (tf & texflags.SURF_SKY2D) == texflags.SURF_SKY2D || (tf & texflags.SURF_SKY) == texflags.SURF_SKY || (tf & texflags.SURF_NODRAW) == texflags.SURF_NODRAW || (tf & texflags.SURF_SKIP) == texflags.SURF_SKIP;
        }
        private void ReadFaces(BSPParser bspParser, VPKParser vpkParser, CancellationToken cancelToken, Action<float, string> onProgressChanged = null)
        {
            for (int i = 0; i < Mathf.RoundToInt(bspParser.faces.Length * FaceLoadPercent); i++)
            {
                if (cancelToken.IsCancellationRequested)
                    return;

                dface_t face = bspParser.faces[i];

                texflags currentTexFlags = GetFaceTextureFlags(face, bspParser);
                string textureLocation = GetFaceTextureLocation(face, bspParser);

                if (!IsUndesiredTexture(textureLocation, currentTexFlags))
                {
                    FaceMesh currentFace = new FaceMesh();
                    currentFace.textureFlag = currentTexFlags;
                    currentFace.lightmapKey = bspParser.faces[i].lightofs;

                    currentFace.faceName = textureLocation;
                    currentFace.material = VMTData.GrabVMT(bspParser, vpkParser, textureLocation);
                    currentFace.meshData = MakeFace(bspParser, face);
                    AddFaceMesh(currentFace, combineMeshesWithSameTexture);
                }

                totalItemsLoaded++;
                onProgressChanged?.Invoke(PercentLoaded, currentMessage);
            }
        }
        public MeshHelpers.MeshData MakeFace(BSPParser bspParser, dface_t face)
        {
            #region Get all vertices of face
            List<Vector3> surfaceVertices = new List<Vector3>();
            List<Vector3> originalVertices = new List<Vector3>();
            List<Vector3> normals = new List<Vector3>();
            for (int i = 0; i < face.numedges; i++)
            {
                ushort[] currentEdge = bspParser.edges[Mathf.Abs(bspParser.surfedges[face.firstedge + i])].v;
                Vector3 point1 = bspParser.vertices[currentEdge[0]], point2 = bspParser.vertices[currentEdge[1]];
                Vector3 planeNormal = bspParser.planes[face.planenum].normal;

                point1 = new Vector3(point1.x, point1.y, point1.z);
                point2 = new Vector3(point2.x, point2.y, point2.z);

                if (bspParser.surfedges[face.firstedge + i] >= 0)
                {
                    if (surfaceVertices.IndexOf(point1) < 0)
                    {
                        surfaceVertices.Add(point1);
                        normals.Add(planeNormal);
                    }
                    originalVertices.Add(point1);

                    if (surfaceVertices.IndexOf(point2) < 0)
                    {
                        surfaceVertices.Add(point2);
                        normals.Add(planeNormal);
                    }
                    originalVertices.Add(point2);
                }
                else
                {
                    if (surfaceVertices.IndexOf(point2) < 0)
                    {
                        surfaceVertices.Add(point2);
                        normals.Add(planeNormal);
                    }
                    originalVertices.Add(point2);

                    if (surfaceVertices.IndexOf(point1) < 0)
                    {
                        surfaceVertices.Add(point1);
                        normals.Add(planeNormal);
                    }
                    originalVertices.Add(point1);
                }
            }
            #endregion

            #region Apply Displacement
            if (face.dispinfo > -1)
            {
                ddispinfo_t disp = bspParser.dispInfo[face.dispinfo];
                int power = Mathf.RoundToInt(Mathf.Pow(2, disp.power));

                List<Vector3> dispVertices = new List<Vector3>();
                Vector3 startingPosition = surfaceVertices[0];
                Vector3 topCorner = surfaceVertices[1], topRightCorner = surfaceVertices[2], rightCorner = surfaceVertices[3];

                #region Setting Orientation
                Vector3 dispStartingVertex = disp.startPosition;
                if (Vector3.Distance(dispStartingVertex, topCorner) < 0.01f)
                {
                    Vector3 tempCorner = startingPosition;

                    startingPosition = topCorner;
                    topCorner = topRightCorner;
                    topRightCorner = rightCorner;
                    rightCorner = tempCorner;
                }
                else if (Vector3.Distance(dispStartingVertex, rightCorner) < 0.01f)
                {
                    Vector3 tempCorner = startingPosition;

                    startingPosition = rightCorner;
                    rightCorner = topRightCorner;
                    topRightCorner = topCorner;
                    topCorner = tempCorner;
                }
                else if (Vector3.Distance(dispStartingVertex, topRightCorner) < 0.01f)
                {
                    Vector3 tempCorner = startingPosition;

                    startingPosition = topRightCorner;
                    topRightCorner = tempCorner;
                    tempCorner = rightCorner;
                    rightCorner = topCorner;
                    topCorner = tempCorner;
                }
                #endregion

                int orderNum = 0;
                #region Method 13 (The one and only two)
                Vector3 leftSide = (topCorner - startingPosition), rightSide = (topRightCorner - rightCorner);
                float leftSideLineSegmentationDistance = leftSide.magnitude / power, rightSideLineSegmentationDistance = rightSide.magnitude / power;
                for (int line = 0; line < (power + 1); line++)
                {
                    for (int point = 0; point < (power + 1); point++)
                    {
                        Vector3 leftPoint = (leftSide.normalized * line * leftSideLineSegmentationDistance) + startingPosition;
                        Vector3 rightPoint = (rightSide.normalized * line * rightSideLineSegmentationDistance) + rightCorner;
                        Vector3 currentLine = rightPoint - leftPoint;
                        Vector3 pointDirection = currentLine.normalized;
                        float pointSideSegmentationDistance = currentLine.magnitude / power;

                        Vector3 pointA = leftPoint + (pointDirection * pointSideSegmentationDistance * point);

                        Vector3 dispDirectionA = bspParser.dispVerts[disp.DispVertStart + orderNum].vec;
                        dispVertices.Add(pointA + (dispDirectionA * bspParser.dispVerts[disp.DispVertStart + orderNum].dist));
                        orderNum++;
                    }
                }
                #endregion

                surfaceVertices = dispVertices;
            }
            #endregion

            #region Triangulate
            List<int> triangleIndices = new List<int>();

            if (face.dispinfo > -1)
            {
                ddispinfo_t disp = bspParser.dispInfo[face.dispinfo];
                int power = Mathf.RoundToInt(Mathf.Pow(2, disp.power));

                #region Method 12 Triangulation
                for (int row = 0; row < power; row++)
                {
                    for (int col = 0; col < power; col++)
                    {
                        int currentLine = row * (power + 1);
                        int nextLineStart = (row + 1) * (power + 1);

                        triangleIndices.Add(currentLine + col);
                        triangleIndices.Add(currentLine + col + 1);
                        triangleIndices.Add(nextLineStart + col);

                        triangleIndices.Add(currentLine + col + 1);
                        triangleIndices.Add(nextLineStart + col + 1);
                        triangleIndices.Add(nextLineStart + col);
                    }
                }
                #endregion
            }
            else
            {
                for (int i = 0; i < (originalVertices.Count / 2) - 0; i++)
                {
                    int firstOrigIndex = i * 2, secondOrigIndex = (i * 2) + 1, thirdOrigIndex = 0;
                    int firstIndex = surfaceVertices.IndexOf(originalVertices[firstOrigIndex]);
                    int secondIndex = surfaceVertices.IndexOf(originalVertices[secondOrigIndex]);
                    int thirdIndex = surfaceVertices.IndexOf(originalVertices[thirdOrigIndex]);

                    triangleIndices.Add(thirdIndex);
                    triangleIndices.Add(secondIndex);
                    triangleIndices.Add(firstIndex);
                }
            }
            #endregion

            #region Map UV Points
            Vector3 s = Vector3.zero, t = Vector3.zero;
            float xOffset = 0, yOffset = 0;

            try
            {
                s = new Vector3(bspParser.texInfo[face.texinfo].textureVecs[0][0], bspParser.texInfo[face.texinfo].textureVecs[0][1], bspParser.texInfo[face.texinfo].textureVecs[0][2]);
                t = new Vector3(bspParser.texInfo[face.texinfo].textureVecs[1][0], bspParser.texInfo[face.texinfo].textureVecs[1][1], bspParser.texInfo[face.texinfo].textureVecs[1][2]);
                xOffset = bspParser.texInfo[face.texinfo].textureVecs[0][3];
                yOffset = bspParser.texInfo[face.texinfo].textureVecs[1][3];
            }
            catch (Exception e)
            {
                Debug.LogError(e);
            }

            Vector2[] uvPoints = new Vector2[surfaceVertices.Count];
            int textureWidth = 0, textureHeight = 0;

            try
            {
                textureWidth = bspParser.texData[bspParser.texInfo[face.texinfo].texdata].width;
                textureHeight = bspParser.texData[bspParser.texInfo[face.texinfo].texdata].height;
            }
            catch (Exception e)
            {
                Debug.LogError(e);
            }

            for (int i = 0; i < uvPoints.Length; i++)
                uvPoints[i] = new Vector2((Vector3.Dot(surfaceVertices[i], s) + xOffset) / textureWidth, (textureHeight - (Vector3.Dot(surfaceVertices[i], t) + yOffset)) / textureHeight);
            #endregion

            #region Organize Mesh Data
            MeshHelpers.MeshData meshData = new MeshHelpers.MeshData();
            meshData.vertices = surfaceVertices.ToArray();
            meshData.triangles = triangleIndices.ToArray();
            meshData.normals = normals.ToArray();
            meshData.uv = uvPoints;
            #endregion

            #region Clear References
            surfaceVertices.Clear();
            surfaceVertices = null;
            originalVertices.Clear();
            originalVertices = null;
            normals.Clear();
            normals = null;
            triangleIndices.Clear();
            triangleIndices = null;
            uvPoints = null;
            #endregion

            return meshData;
        }
        private void AddFaceMesh(FaceMesh faceMesh, bool combine)
        {
            if (combine)
            {
                FaceMesh latestFace = null;// allFaces.Where(face => face.texture == faceMesh.texture).LastOrDefault();

                if (latestFace != null)
                {
                    if (!latestFace.Append(faceMesh))
                        allFaces.Add(faceMesh);
                }
                else
                    allFaces.Add(faceMesh);
            }
            else
                allFaces.Add(faceMesh);
        }

        private void ReadStaticProps(BSPParser bspParser, VPKParser vpkParser, CancellationToken cancelToken, Action<float, string> onProgressChanged = null)
        {
            int staticPropCount = Mathf.RoundToInt(bspParser.staticProps.staticPropInfo.Length * ModelLoadPercent);
            staticProps = new StaticPropData[staticPropCount];
            for (int i = 0; i < staticPropCount; i++)
            {
                if (cancelToken.IsCancellationRequested)
                    return;

                var currentPropInfo = bspParser.staticProps.staticPropInfo[i];

                ushort propType = currentPropInfo.PropType;
                string modelFullPath = bspParser.staticProps.staticPropDict.names[propType];
                modelFullPath = modelFullPath.Substring(0, modelFullPath.LastIndexOf("."));

                staticProps[i].model = SourceModel.GrabModel(bspParser, vpkParser, modelFullPath);
                staticProps[i].debug = currentPropInfo.ToString();

                staticProps[i].origin = currentPropInfo.Origin;
                staticProps[i].angles = currentPropInfo.Angles;

                totalItemsLoaded++;
                onProgressChanged?.Invoke(PercentLoaded, currentMessage);
            }
        }

        public void MakeGameObject(Action<float, string> onProgressChanged = null, Action<GameObject> outputAction = null)
        {
            isBuilt = false;
            isBuilding = true;

            totalItemsLoaded = 0;

            currentMessage = "Building GameObject";

            gameObject = new GameObject(mapName);
            gameObject.SetActive(false);

            int staticPropsCount = staticProps != null ? staticProps.Length : 0;
            totalItemsToLoad = allFaces.Count + staticPropsCount;

            /*List<LightmapData> lightmapDataList = new List<LightmapData>();
            foreach (var lightmap in lightmaps)
            {
                LightmapData ld = new LightmapData();
                ld.lightmapColor = lightmap.Value.GetTexture();
                lightmap.Value.lightmapIndex = lightmapDataList.Count;
                lightmapDataList.Add(ld);
            }
            Debug.Log("BSPMap: Added " + lightmapDataList.Count + " lightmap(s)");
            LightmapSettings.lightmaps = lightmapDataList.ToArray();*/

            foreach (FaceMesh face in allFaces)
            {
                string faceName = face.faceName;
                if (faceName == null || faceName.Length <= 0)
                    faceName = mapName + " Part";
                GameObject faceGO = new GameObject("Map_Face_" + faceName);
                faceGO.transform.parent = gameObject.transform;
                faceGO.transform.position = face.relativePosition;
                faceGO.transform.rotation = Quaternion.Euler(face.relativeRotation);
                faceGO.AddComponent<MeshFilter>().mesh = face.meshData.GetMesh();
                var faceRenderer = faceGO.AddComponent<MeshRenderer>();
                faceRenderer.material = face.material?.GetMaterial();
                /*if (lightmaps.ContainsKey(face.lightmapKey))
                {
                    //faceRenderer.material.mainTexture = lightmaps[face.lightmapKey].GetTexture();
                    faceRenderer.lightmapIndex = lightmaps[face.lightmapKey].lightmapIndex;
                }*/
                faceGO = null;

                totalItemsLoaded++;
                onProgressChanged?.Invoke(PercentLoaded, currentMessage);
                //if (totalItemsLoaded % breathingInterval == 0)
                //    yield return null;
            }
            for (int i = 0; i < staticPropsCount; i++)
            {
                if (staticProps[i].model != null)
                {
                    GameObject model = staticProps[i].model.InstantiateGameObject();
                    model.name += "_" + staticProps[i].debug;
                    model.transform.SetParent(gameObject.transform);
                    model.transform.localPosition = staticProps[i].origin.FixNaN();
                    model.transform.rotation = staticProps[i].angles.ToQuaternion();
                }
                totalItemsLoaded++;
                onProgressChanged?.Invoke(PercentLoaded, currentMessage);
                //if (totalItemsLoaded % breathingInterval == 0)
                //    yield return null;
            }

            gameObject.transform.localScale = new Vector3(1, -1, 1);
            gameObject.transform.rotation = Quaternion.Euler(-90, 0, 0);

            isBuilding = false;
            isBuilt = true;

            outputAction?.Invoke(gameObject);
        }
    }

    public struct StaticPropData
    {
        public SourceModel model;
        public Vector3 origin;
        public QAngle angles;
        public string debug;
    }
    public class FaceMesh
    {
        public string faceName;
        public Vector3 relativePosition;
        public Vector3 relativeRotation;

        public MeshHelpers.MeshData meshData;
        //public Vector3 s, t;
        //public float xOffset, yOffset;
        public texflags textureFlag;
        public VMTData material;
        public int lightmapKey = -1;
        //public SourceTexture texture;

        public FaceMesh()
        {
            relativePosition = Vector3.zero;
            relativeRotation = Vector3.zero;
            meshData = new MeshHelpers.MeshData();
            textureFlag = texflags.SURF_NODRAW;
        }
        public void Dispose()
        {
            //texture?.Dispose();
            material?.Dispose();
            meshData?.Dispose();
        }

        public bool Append(FaceMesh other, bool positionStays = true)
        {
            Vector3 position = relativePosition;
            Vector3 rotation = relativeRotation;
            if (positionStays)
            {
                position = other.relativePosition - position;
                rotation = other.relativeRotation - rotation;
            }

            return meshData.Append(other.meshData, position, Quaternion.Euler(rotation), Vector3.one);
        }
    }
}