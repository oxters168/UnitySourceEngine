using UnityEngine;
using System.Collections.Generic;
using System.IO;

namespace UnitySourceEngine
{
    public class SourceModel
    {
        //public static Material modelMaterial;
        private static Dictionary<string, SourceModel> loadedModels = new Dictionary<string, SourceModel>();
        private static GameObject staticPropLibrary;
        //public static bool excludeTextures;

        public string modelName { get; private set; }
        public string modelLocation { get; private set; }
        private string modelKey { get { return modelLocation + modelName; } }

        public int version { get; private set; }
        public int id { get; private set; }

        private GameObject modelPrefab;
        private List<FaceMesh> faces = new List<FaceMesh>();
        //private SourceTexture[] modelTextures;
        //private List<VMTData> materials = new List<VMTData>();
        //private List<Material> materialsCreated = new List<Material>();

        private SourceModel(string name, string location)
        {
            modelName = name;
            modelLocation = location;

            loadedModels.Add(modelKey, this);
        }

        public static void ClearCache()
        {
            foreach (var modPair in loadedModels)
                modPair.Value.Dispose();
            loadedModels.Clear();
            loadedModels = new Dictionary<string, SourceModel>();
        }
        public void Dispose()
        {
            if (loadedModels != null && loadedModels.ContainsKey(modelKey))
                loadedModels.Remove(modelKey);

            //if (materialsCreated != null)
            //    foreach (Material mat in materialsCreated)
            //        if (mat != null)
            //            Object.Destroy(mat);
            //materialsCreated = new List<Material>();

            if (faces != null)
                foreach (FaceMesh face in faces)
                    face?.Dispose();
            faces = null;

            //if (modelTextures != null)
            //    foreach (SourceTexture texture in modelTextures)
            //        texture?.Dispose();
            //modelTextures = null;

            if (modelPrefab != null)
                Object.Destroy(modelPrefab);
            modelPrefab = null;
        }

        public static SourceModel GrabModel(BSPParser bspParser, VPKParser vpkParser, string rawPath)
        {
            SourceModel model = null;

            string modelName = "";
            string modelLocation = rawPath.Replace("\\", "/").ToLower();

            if (modelLocation.IndexOf("/") > -1)
            {
                modelName = modelLocation.Substring(modelLocation.LastIndexOf("/") + 1);
                modelLocation = modelLocation.Substring(0, modelLocation.LastIndexOf("/") + 1);

                model = GrabModel(bspParser, vpkParser, modelName, modelLocation);
            }

            return model;
        }
        public static SourceModel GrabModel(BSPParser bspParser, VPKParser vpkParser, string name, string location)
        {
            SourceModel model = null;

            string fixedModelName = name.ToLower();
            string fixedModelLocation = location.Replace("\\", "/").ToLower();

            if (fixedModelName.LastIndexOf(".") > -1 && fixedModelName.LastIndexOf(".") == fixedModelName.Length - 4)
                fixedModelName = fixedModelName.Substring(0, fixedModelName.LastIndexOf("."));
            if (fixedModelLocation.LastIndexOf("/") != fixedModelLocation.Length - 1)
                fixedModelLocation = fixedModelLocation + "/";
            if (fixedModelLocation.IndexOf("models/") > -1)
                fixedModelLocation = fixedModelLocation.Substring(fixedModelLocation.IndexOf("models/") + "models/".Length);

            if (loadedModels.ContainsKey(fixedModelLocation + fixedModelName))
            {
                model = loadedModels[fixedModelLocation + fixedModelName];
            }
            else
            {
                model = new SourceModel(fixedModelName, fixedModelLocation);
                model.Parse(bspParser, vpkParser);
            }

            return model;
        }
        private void Parse(BSPParser bspParser, VPKParser vpkParser)
        {
            if (vpkParser != null)
            {
                string modelsVPKDir = ((modelLocation.IndexOf("/") == 0) ? "models" : "models/");
                string mdlPath = modelsVPKDir + modelLocation + modelName + ".mdl";
                string vvdPath = modelsVPKDir + modelLocation + modelName + ".vvd";
                string vtxPath = modelsVPKDir + modelLocation + modelName + ".vtx";

                if (bspParser.HasPakFile(mdlPath) || vpkParser.FileExists(mdlPath))
                {
                    if (bspParser.HasPakFile(vvdPath) || vpkParser.FileExists(vvdPath))
                    {
                        if (!bspParser.HasPakFile(vtxPath) && !vpkParser.FileExists(vtxPath))
                            vtxPath = modelsVPKDir + modelLocation + modelName + ".dx90.vtx";

                        if (bspParser.HasPakFile(vtxPath) || vpkParser.FileExists(vtxPath))
                        {
                            using (MDLParser mdl = new MDLParser())
                            using (VVDParser vvd = new VVDParser())
                            using (VTXParser vtx = new VTXParser())
                            {
                                try
                                {
                                    if (bspParser.HasPakFile(mdlPath))
                                    {
                                        using (var stream = new MemoryStream(bspParser.GetPakFile(mdlPath)))
                                            mdl.Parse(stream, 0);
                                    }
                                    else
                                        vpkParser.LoadFileAsStream(mdlPath, (stream, origOffset, byteCount) => { mdl.Parse(stream, origOffset); });

                                    if (bspParser.HasPakFile(vvdPath))
                                    {
                                        using (var stream = new MemoryStream(bspParser.GetPakFile(vvdPath)))
                                            vvd.Parse(stream, 0);
                                    }
                                    else
                                        vpkParser.LoadFileAsStream(vvdPath, (stream, origOffset, byteCount) => { vvd.Parse(stream, origOffset); });

                                    if (bspParser.HasPakFile(vtxPath))
                                    {
                                        using (var stream = new MemoryStream(bspParser.GetPakFile(vtxPath)))
                                            vtx.Parse(stream, 0);
                                    }
                                    else
                                        vpkParser.LoadFileAsStream(vtxPath, (stream, origOffset, byteCount) => { vtx.Parse(stream, origOffset); });

                                    version = mdl.header1.version;
                                    id = mdl.header1.id;

                                    if (mdl.bodyParts != null)
                                        ReadFaceMeshes(mdl, vvd, vtx, bspParser, vpkParser);
                                    else
                                        Debug.LogError("SourceModel: Could not find body parts of " + modelKey);
                                }
                                catch (System.Exception e)
                                {
                                    Debug.LogError("SourceModel: " + e.ToString());
                                }
                            }
                        }
                        else
                            Debug.LogError("SourceModel: Could not find vtx file (" + vtxPath + ")");
                    }
                    else
                        Debug.LogError("SourceModel: Could not find vvd file (" + vvdPath + ")");
                }
                else
                    Debug.LogError("SourceModel: Could not find mdl file (" + mdlPath + ")");
            }
            else
                Debug.LogError("SourceModel: VPK parser is null");
        }
        private void ReadFaceMeshes(MDLParser mdl, VVDParser vvd, VTXParser vtx, BSPParser bspParser, VPKParser vpkParser)
        {
            int textureIndex = 0;
            if (mdl.bodyParts.Length == vtx.bodyParts.Length)
            {
                for (int bodyPartIndex = 0; bodyPartIndex < mdl.bodyParts.Length; bodyPartIndex++)
                {
                    for (int modelIndex = 0; modelIndex < mdl.bodyParts[bodyPartIndex].models.Length; modelIndex++)
                    {
                        //int currentPosition = 0;
                        for (int meshIndex = 0; meshIndex < mdl.bodyParts[bodyPartIndex].models[modelIndex].theMeshes.Length; meshIndex++)
                        {
                            int rootLodIndex = mdl.header1.rootLod;
                            //int rootLodIndex = 0;
                            //int rootLodCount = 1;
                            //if (mdl.header1.numAllowedRootLods == 0)
                            //    rootLodCount = vtx.header.numLODs;

                            FaceMesh currentFace = new FaceMesh();

                            int verticesStartIndex = mdl.bodyParts[bodyPartIndex].models[modelIndex].theMeshes[meshIndex].vertexIndexStart;
                            int vertexCount = mdl.bodyParts[bodyPartIndex].models[modelIndex].theMeshes[meshIndex].vertexData.lodVertexCount[rootLodIndex];

                            Vector3[] vertices = new Vector3[vertexCount];
                            Vector3[] normals = new Vector3[vertexCount];
                            Vector2[] uv = new Vector2[vertexCount];

                            for (int verticesIndex = 0; verticesIndex < vertices.Length; verticesIndex++)
                            {
                                vertices[verticesIndex] = vvd.vertices[verticesStartIndex + verticesIndex].m_vecPosition;
                                normals[verticesIndex] = vvd.vertices[verticesStartIndex + verticesIndex].m_vecNormal;
                                uv[verticesIndex] = vvd.vertices[verticesStartIndex + verticesIndex].m_vecTexCoord;
                            }

                            List<int> triangles = new List<int>();
                            //for (int countUpLodIndex = 0; countUpLodIndex <= rootLodIndex; countUpLodIndex++)
                            //{
                                for (int stripGroupIndex = 0; stripGroupIndex < vtx.bodyParts[bodyPartIndex].theVtxModels[modelIndex].theVtxModelLods[rootLodIndex].theVtxMeshes[meshIndex].stripGroupCount; stripGroupIndex++)
                                {
                                    //var currentStripGroup = vtx.bodyParts[bodyPartIndex].theVtxModels[modelIndex].theVtxModelLods[rootLodIndex].theVtxMeshes[meshIndex].theVtxStripGroups[0];
                                    var currentStripGroup = vtx.bodyParts[bodyPartIndex].theVtxModels[modelIndex].theVtxModelLods[rootLodIndex].theVtxMeshes[meshIndex].theVtxStripGroups[stripGroupIndex];
                                    //int trianglesCount = currentStripGroup.theVtxIndices.Length;
                                    //int[] triangles = new int[trianglesCount];

                                    for (int stripIndex = 0; stripIndex < currentStripGroup.theVtxStrips.Length; stripIndex++)
                                    {
                                        var currentStrip = currentStripGroup.theVtxStrips[stripIndex];

                                        if (((StripHeaderFlags_t)currentStrip.flags & StripHeaderFlags_t.STRIP_IS_TRILIST) > 0)
                                        {
                                            for (int indexIndex = 0; indexIndex < currentStrip.indexCount; indexIndex += 3)
                                            {
                                                int vertexIndexA = verticesStartIndex + currentStripGroup.theVtxVertices[currentStripGroup.theVtxIndices[indexIndex + currentStrip.indexMeshIndex]].originalMeshVertexIndex;
                                                int vertexIndexB = verticesStartIndex + currentStripGroup.theVtxVertices[currentStripGroup.theVtxIndices[indexIndex + currentStrip.indexMeshIndex + 2]].originalMeshVertexIndex;
                                                int vertexIndexC = verticesStartIndex + currentStripGroup.theVtxVertices[currentStripGroup.theVtxIndices[indexIndex + currentStrip.indexMeshIndex + 1]].originalMeshVertexIndex;

                                                if (vertexIndexA < vertices.Length && vertexIndexB < vertices.Length && vertexIndexC < vertices.Length)
                                                {
                                                    triangles.Add(vertexIndexA);
                                                    triangles.Add(vertexIndexB);
                                                    triangles.Add(vertexIndexC);
                                                }
                                            }
                                        }
                                    }
                                }
                            //}

                            //Debug.Assert(triangles.Count % 3 == 0, "SourceModel: Triangles not a multiple of three for " + modelName);
                            //Debug.Assert(vtx.bodyParts[bodyPartIndex].theVtxModels[modelIndex].theVtxModelLods[rootLodIndex].theVtxMeshes[meshIndex].theVtxStripGroups.Length == 1, "SourceModel: Strip groups not one (" + vtx.bodyParts[bodyPartIndex].theVtxModels[modelIndex].theVtxModelLods[rootLodIndex].theVtxMeshes[meshIndex].theVtxStripGroups.Length  + ") for " + modelName);
                            //Debug.Assert(mdl.header1.includemodel_count <= 0, "SourceModel: Include model count greater than zero (" + mdl.header1.includemodel_count + ", " + mdl.header1.includemodel_index + ") for " + modelName);
                            //Debug.Assert(mdl.header1.numAllowedRootLods == 1, "SourceModel: Allowed root lods not one (" + mdl.header1.numAllowedRootLods + ", vtx#" + vtx.header.numLODs + ", vvd#" + vvd.header.numLODs + ", vvd2#" + vvd.header.numLODVertices + ", root" + mdl.header1.rootLod + ") for " + modelName);
                            Debug.Assert(vvd.header.numFixups <= 0, "SourceModel: " + vvd.header.numFixups + " fixups found for " + modelName);

                            MeshData meshData = new MeshData();
                            meshData.vertices = vertices;
                            meshData.triangles = triangles.ToArray();
                            meshData.normals = normals;
                            meshData.uv = uv;

                            currentFace.meshData = meshData;

                            string texturePath = mdl.texturePaths[0].Replace("\\", "/").ToLower();
                            string textureName = mdl.textures[textureIndex].name.Replace("\\", "/").ToLower();
                            if (textureName.IndexOf(texturePath) > -1)
                                texturePath = "";
                            string textureLocation = texturePath + textureName;
                            //if (modelTextures != null && textureIndex < modelTextures.Length) //Should not have this line
                            //    textureLocation = modelTextures[textureIndex]?.location;

                            currentFace.faceName = textureLocation;
                            currentFace.material = VMTData.GrabVMT(bspParser, vpkParser, textureLocation);
                            //if (!excludeTextures)
                            //    currentFace.texture = SourceTexture.GrabTexture(bspParser, vpkParser, textureLocation);
                            faces.Add(currentFace);

                            textureIndex++;
                        }
                    }
                }
            }
            else
                Debug.LogError("SourceModel: MDL and VTX body part count doesn't match (" + modelLocation + ")");
        }

        private void BuildPrefab()
        {
            modelPrefab = new GameObject("StaticProp_v" + version + "_id" + id + "_" + modelName);
            modelPrefab.transform.parent = staticPropLibrary.transform;
            modelPrefab.SetActive(false);

            //Material materialPrefab = modelMaterial;
            //bool destroyMatAfterBuild = modelMaterial == null;
            //if (destroyMatAfterBuild)
            //    materialPrefab = new Material(Shader.Find("Legacy Shaders/Diffuse"));
            if (faces != null)
                foreach (FaceMesh faceMesh in faces)
                {
                    GameObject meshRepresentation = new GameObject(faceMesh.faceName);
                    meshRepresentation.transform.parent = modelPrefab.transform;

                    Mesh mesh = faceMesh.meshData.GetMesh();

                    MeshFilter mesher = meshRepresentation.AddComponent<MeshFilter>();
                    mesher.sharedMesh = mesh;

                    //materialsCreated.Add(meshMaterial);
                    //meshMaterial.mainTexture = faceMesh.texture?.GetTexture();
                    meshRepresentation.AddComponent<MeshRenderer>().material = faceMesh.material?.GetMaterial();
                    meshRepresentation.AddComponent<MeshCollider>();
                }
            //if (destroyMatAfterBuild)
            //    Object.Destroy(materialPrefab);
        }
        public GameObject InstantiateGameObject()
        {
            if (!staticPropLibrary)
                staticPropLibrary = new GameObject("StaticPropPrefabs");
            if (!modelPrefab)
                BuildPrefab();

            GameObject cloned = Object.Instantiate(modelPrefab);
            cloned.SetActive(true);
            return cloned;
        }
    }
}