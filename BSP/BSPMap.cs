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
        public static bool combineMeshesWithSameTextures;
        public static bool excludeTextures;
        public static bool excludeModels;
        public static bool excludeMapFaces;
        public static string vpkLoc;
        public static Material mapMaterial;
        public GameObject gameObject { get; private set; }
        private List<Material> materialsCreated = new List<Material>();

        private List<FaceMesh> allFaces = new List<FaceMesh>();
        private SourceModel[] staticProps;
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

            foreach (Material mat in materialsCreated)
                UnityEngine.Object.Destroy(mat);
            materialsCreated.Clear();
            materialsCreated = new List<Material>();

            if (staticProps != null)
                foreach (SourceModel prop in staticProps)
                    prop?.Dispose();
            staticProps = null;

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

                foreach (dface_t face in bspParser.faces)
                {
                    if (cancelToken.IsCancellationRequested)
                        return null;

                    texflags currentTexFlags = GetFaceTextureFlags(face, bspParser);
                    string textureLocation = GetFaceTextureLocation(face, bspParser);

                    if (!IsUndesiredTexture(textureLocation, currentTexFlags))
                    {
                        string dependency;
                        if (!vpkParser.FileExists("/materials/" + textureLocation + ".vtf"))
                            dependency = vpkParser.LocateInArchive("/materials/" + textureLocation + ".vmt");
                        else
                            dependency = vpkParser.LocateInArchive("/materials/" + textureLocation + ".vtf");

                        if (!string.IsNullOrEmpty(dependency) && !dependencies.Contains(dependency))
                            dependencies.Add(dependency);
                    }
                }

                //Todo: Add vpk dependency check for static props
                //if (validVPK && !excludeModels)
                //    ReadStaticProps(bspParser, vpkParser, onProgressChanged);
            }
            return dependencies;
        }
        public void ParseFile(CancellationToken cancelToken, Action<float, string> onProgressChanged = null, Action onFinished = null)
        {
            isParsed = false;
            isParsing = true;

            currentMessage = "Reading BSP Data";
            onProgressChanged?.Invoke(PercentLoaded, currentMessage);

            using (VPKParser vpkParser = new VPKParser(vpkLoc))
            using (BSPParser bspParser = new BSPParser(Path.Combine(mapDir, mapName + ".bsp")))
            {
                bspParser.ParseData(cancelToken);

                int facesCount = excludeMapFaces ? 0 : bspParser.faces.Length;
                int propsCount = excludeModels ? 0 : bspParser.staticProps.staticPropInfo.Length;
                totalItemsToLoad = facesCount + propsCount;

                bool validVPK = vpkParser.IsValid();

                currentMessage = "Parsing Faces";
                onProgressChanged?.Invoke(PercentLoaded, currentMessage);
                if (!excludeMapFaces)
                    ReadFaces(bspParser, validVPK ? vpkParser : null, cancelToken, onProgressChanged);

                currentMessage = "Loading Static Props";
                onProgressChanged?.Invoke(PercentLoaded, currentMessage);
                if (validVPK && !excludeModels)
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
            foreach (dface_t face in bspParser.faces)
            {
                if (cancelToken.IsCancellationRequested)
                    return;

                texflags currentTexFlags = GetFaceTextureFlags(face, bspParser);
                string textureLocation = GetFaceTextureLocation(face, bspParser);

                if (!IsUndesiredTexture(textureLocation, currentTexFlags))
                {
                    FaceMesh currentFace = new FaceMesh();
                    currentFace.textureFlag = currentTexFlags;

                    currentFace.s = new Vector3(bspParser.texInfo[face.texinfo].textureVecs[0][0], bspParser.texInfo[face.texinfo].textureVecs[0][2], bspParser.texInfo[face.texinfo].textureVecs[0][1]);
                    currentFace.t = new Vector3(bspParser.texInfo[face.texinfo].textureVecs[1][0], bspParser.texInfo[face.texinfo].textureVecs[1][2], bspParser.texInfo[face.texinfo].textureVecs[1][1]);
                    currentFace.xOffset = bspParser.texInfo[face.texinfo].textureVecs[0][3];
                    currentFace.yOffset = bspParser.texInfo[face.texinfo].textureVecs[1][3];

                    currentFace.faceName = textureLocation;
                    if (vpkParser != null && !excludeTextures)
                        currentFace.texture = SourceTexture.GrabTexture(vpkParser, textureLocation);
                    currentFace.meshData = MakeFace(bspParser, face);
                    AddFaceMesh(currentFace, combineMeshesWithSameTextures);
                }

                totalItemsLoaded++;
                onProgressChanged?.Invoke(PercentLoaded, currentMessage);
            }
        }
        public MeshData MakeFace(BSPParser bspParser, dface_t face)
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

                point1 = new Vector3(point1.x, point1.z, point1.y);
                point2 = new Vector3(point2.x, point2.z, point2.y);

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
                dispStartingVertex = new Vector3(dispStartingVertex.x, dispStartingVertex.z, dispStartingVertex.y);
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
                        dispDirectionA = new Vector3(dispDirectionA.x, dispDirectionA.z, dispDirectionA.y);
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
                        triangleIndices.Add(nextLineStart + col);
                        triangleIndices.Add(currentLine + col + 1);

                        triangleIndices.Add(currentLine + col + 1);
                        triangleIndices.Add(nextLineStart + col);
                        triangleIndices.Add(nextLineStart + col + 1);
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

                    triangleIndices.Add(firstIndex);
                    triangleIndices.Add(secondIndex);
                    triangleIndices.Add(thirdIndex);
                }
            }
            #endregion

            #region Map UV Points
            Vector3 s = Vector3.zero, t = Vector3.zero;
            float xOffset = 0, yOffset = 0;

            try
            {
                s = new Vector3(bspParser.texInfo[face.texinfo].textureVecs[0][0], bspParser.texInfo[face.texinfo].textureVecs[0][2], bspParser.texInfo[face.texinfo].textureVecs[0][1]);
                t = new Vector3(bspParser.texInfo[face.texinfo].textureVecs[1][0], bspParser.texInfo[face.texinfo].textureVecs[1][2], bspParser.texInfo[face.texinfo].textureVecs[1][1]);
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
            MeshData meshData = new MeshData();
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
                FaceMesh latestFace = allFaces.Where(face => face.texture == faceMesh.texture).LastOrDefault();

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
            staticProps = new SourceModel[bspParser.staticProps.staticPropInfo.Length];
            for (int i = 0; i < bspParser.staticProps.staticPropInfo.Length; i++)
            {
                if (cancelToken.IsCancellationRequested)
                    return;

                //Debug.Log("Prop: Fixing Location");
                if (i >= bspParser.staticProps.staticPropInfo.Length)
                {
                    Debug.Log("Could not find model");
                    continue;
                }
                ushort propType = bspParser.staticProps.staticPropInfo[i].PropType;
                if (propType >= bspParser.staticProps.staticPropDict.names.Length)
                {
                    Debug.Log("Could not find model");
                    continue;
                }
                string modelFullPath = bspParser.staticProps.staticPropDict.names[propType];

                string modelName = "", modelLocation = "";
                modelName = modelFullPath.Substring(modelFullPath.LastIndexOf("/") + 1);
                modelName = modelName.Substring(0, modelName.LastIndexOf("."));
                modelLocation = modelFullPath.Substring(0, modelFullPath.LastIndexOf("/"));

                //Debug.Log("Prop: Grabbing Model (" + modelLocation + "/" + modelName + ".mdl" + ")");
                staticProps[i] = SourceModel.GrabModel(vpkParser, modelName, modelLocation);
                staticProps[i].origin = bspParser.staticProps.staticPropInfo[i].Origin;
                staticProps[i].angles = bspParser.staticProps.staticPropInfo[i].Angles;

                totalItemsLoaded++;
                onProgressChanged?.Invoke(PercentLoaded, currentMessage);
            }
        }

        public void MakeGameObject(Action<float, string> onProgressChanged = null, Action<GameObject> outputAction = null)
        {
            isBuilt = false;
            isBuilding = true;

            totalItemsLoaded = 0;

            gameObject = new GameObject(mapName);
            gameObject.SetActive(false);

            int staticPropsCount = staticProps != null ? staticProps.Length : 0;
            totalItemsToLoad = allFaces.Count + staticPropsCount;

            //int breathingInterval = 25;
            Material materialPrefab = mapMaterial;
            bool destroyMatAfterBuild = mapMaterial == null;
            if (destroyMatAfterBuild)
                materialPrefab = new Material(Shader.Find("Legacy Shaders/Diffuse"));
            foreach (FaceMesh face in allFaces)
            {
                string faceName = face.faceName;
                if (faceName == null || faceName.Length <= 0)
                    faceName = mapName + " Part";
                GameObject faceGO = new GameObject(faceName);
                faceGO.transform.parent = gameObject.transform;
                faceGO.transform.position = face.relativePosition;
                faceGO.transform.rotation = Quaternion.Euler(face.relativeRotation);
                faceGO.AddComponent<MeshFilter>().mesh = face.meshData.GetMesh();

                #region Set Material of GameObject
                Material faceMaterial = new Material(materialPrefab);
                materialsCreated.Add(faceMaterial);
                faceMaterial.mainTextureScale = new Vector2(1, 1);
                faceMaterial.mainTextureOffset = new Vector2(0, 0);
                Texture2D faceTexture = face.texture?.GetTexture();
                if (faceTexture != null)
                {
                    faceMaterial.mainTexture = faceTexture;
                    faceTexture = null;
                }
                faceGO.AddComponent<MeshRenderer>().material = faceMaterial;
                faceGO = null;
                faceMaterial = null;
                #endregion

                totalItemsLoaded++;
                onProgressChanged?.Invoke(PercentLoaded, currentMessage);
                //if (totalItemsLoaded % breathingInterval == 0)
                //    yield return null;
            }
            for (int i = 0; i < staticPropsCount; i++)
            {
                SourceModel currentStaticProp = staticProps[i];
                if (currentStaticProp != null)
                {
                    GameObject model = currentStaticProp.InstantiateGameObject();
                    model.transform.SetParent(gameObject.transform);
                    model.transform.localPosition = currentStaticProp.origin.FixNaN();
                    model.transform.localRotation = Quaternion.Euler(currentStaticProp.angles).FixNaN();
                }
                totalItemsLoaded++;
                onProgressChanged?.Invoke(PercentLoaded, currentMessage);
                //if (totalItemsLoaded % breathingInterval == 0)
                //    yield return null;
            }

            if (destroyMatAfterBuild)
                UnityEngine.Object.Destroy(materialPrefab);

#if UNITY_EDITOR
            //MakeAsset();
            //SaveUVValues("C:\\Users\\oxter\\Documents\\csgo\\csgoMapModels\\" + mapName + "_UV.txt");
#endif

            isBuilding = false;
            isBuilt = true;

            outputAction?.Invoke(gameObject);
        }
    }

    public class FaceMesh
    {
        public string faceName;
        public Vector3 relativePosition;
        public Vector3 relativeRotation;

        public MeshData meshData;
        public Vector3 s, t;
        public float xOffset, yOffset;
        public texflags textureFlag;
        public SourceTexture texture;

        public FaceMesh()
        {
            relativePosition = Vector3.zero;
            relativeRotation = Vector3.zero;
            meshData = new MeshData();
            textureFlag = texflags.SURF_NODRAW;
        }
        public void Dispose()
        {
            texture?.Dispose();
            meshData?.Dispose();
            //meshData = null;
        }
        /*public static FaceMesh Copy(FaceMesh original)
        {
            FaceMesh copy = null;
            if(original != null)
            {
                copy = new FaceMesh();
                copy.relativePosition = original.relativePosition;
                copy.relativeRotation = original.relativeRotation;
                copy.face = original.face;
                //copy.meshData = MeshData.Copy(original.meshData);
                copy.s = original.s;
                copy.t = original.t;
                copy.xOffset = original.xOffset;
                copy.yOffset = original.yOffset;
                copy.rawTexture = original.rawTexture;
                copy.textureLocation = original.textureLocation;
                copy.textureFlag = original.textureFlag;
            }
            return copy;
        }*/

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
    public class MeshData
    {
        public static readonly int MAX_VERTICES = 65534;

        private Mesh mesh;
        public Vector3[] vertices = new Vector3[0];
        public int[] triangles = new int[0];
        public Vector3[] normals = new Vector3[0];
        public Vector2[] uv = new Vector2[0];
        public Vector2[] uv2 = new Vector2[0];
        public Vector2[] uv3 = new Vector2[0];
        public Vector2[] uv4 = new Vector2[0];

        public void Dispose()
        {
            if (mesh != null)
                UnityEngine.Object.Destroy(mesh);
            mesh = null;

            vertices = null;
            triangles = null;
            normals = null;
            uv = null;
            uv2 = null;
            uv3 = null;
            uv4 = null;
        }

        public bool Append(MeshData other, Vector3 position, Quaternion rotation, Vector3 scale)
        {
            if (other != null && vertices.Length + other.vertices.Length < MAX_VERTICES)
            {
                Vector3[] manipulatedVertices = other.vertices.ManipulateVertices(position, rotation, scale);

                int[] correctedTriangles = new int[other.triangles.Length];
                for (int i = 0; i < correctedTriangles.Length; i++)
                    correctedTriangles[i] = other.triangles[i] + vertices.Length;

                vertices = vertices.ToArray().Concat(manipulatedVertices).ToArray();
                triangles = triangles.ToArray().Concat(correctedTriangles).ToArray();
                normals = normals.ToArray().Concat(other.normals).ToArray();
                uv = uv.ToArray().Concat(other.uv).ToArray();
                uv2 = uv2.ToArray().Concat(other.uv2).ToArray();
                uv3 = uv3.ToArray().Concat(other.uv3).ToArray();
                uv4 = uv4.ToArray().Concat(other.uv4).ToArray();
                //vertices = DataParser.Merge(vertices, manipulatedVertices);
                //triangles = DataParser.Merge(triangles, correctedTriangles);
                //normals = DataParser.Merge(normals, other.normals);
                //uv = DataParser.Merge(uv, other.uv);
                //uv2 = DataParser.Merge(uv2, other.uv2);
                //uv3 = DataParser.Merge(uv3, other.uv3);
                //uv4 = DataParser.Merge(uv4, other.uv4);
                return true;
            }
            return false;
        }

        public Mesh GetMesh()
        {
            if (mesh == null)
            {
                mesh = new Mesh();
                mesh.name = "Custom Mesh";
            }

            mesh.Clear();
            mesh.vertices = vertices;
            mesh.triangles = triangles;
            if (uv.Length > 0)
                mesh.uv = uv;
            if (uv2.Length > 0)
                mesh.uv2 = uv2;
            if (uv3.Length > 0)
                mesh.uv3 = uv3;
            if (uv4.Length > 0)
                mesh.uv4 = uv4;
            if (normals.Length == vertices.Length)
                mesh.normals = normals;
            else
                mesh.RecalculateNormals();

            return mesh;
        }
        public IEnumerator DebugNormals()
        {
            for (int i = 0; i < vertices.Length; i++)
            {
                Debug.DrawRay(vertices[i], normals[i], Color.blue, 10000);
                if (i % 100 == 0)
                    yield return null;
            }
        }
    }
}