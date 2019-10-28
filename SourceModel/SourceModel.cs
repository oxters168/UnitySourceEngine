using UnityEngine;
using System.Collections.Generic;
using UnityHelpers;

namespace UnitySourceEngine
{
    public class SourceModel
    {
        private static Dictionary<string, SourceModel> loadedModels = new Dictionary<string, SourceModel>();
        private static GameObject staticPropLibrary;

        public static float decimationPercent = 0;

        public string modelPath { get; private set; }

        public int version { get; private set; }
        public int id { get; private set; }

        private GameObject modelPrefab;
        private List<FaceMesh> faces = new List<FaceMesh>();

        private SourceModel(string key)
        {
            modelPath = key;

            loadedModels.Add(modelPath, this);
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
            if (loadedModels != null && loadedModels.ContainsKey(modelPath))
                loadedModels.Remove(modelPath);

            if (faces != null)
                foreach (FaceMesh face in faces)
                    face?.Dispose();
            faces = null;

            if (modelPrefab != null)
                Object.Destroy(modelPrefab);
            modelPrefab = null;
        }

        public static SourceModel GrabModel(BSPParser bspParser, VPKParser vpkParser, string rawPath)
        {
            SourceModel model = null;

            string fixedLocation = rawPath.Replace("\\", "/").ToLower();

            if (loadedModels.ContainsKey(fixedLocation))
            {
                model = loadedModels[fixedLocation];
            }
            else
            {
                model = new SourceModel(fixedLocation);
                model.Parse(bspParser, vpkParser);
            }

            return model;
        }
        private void Parse(BSPParser bspParser, VPKParser vpkParser)
        {
            if (vpkParser != null)
            {
                string mdlPath = modelPath + ".mdl";
                string vvdPath = modelPath + ".vvd";
                string vtxPath = modelPath + ".vtx";

                if ((bspParser != null && bspParser.HasPakFile(mdlPath)) || (vpkParser != null && vpkParser.FileExists(mdlPath)))
                {
                    if ((bspParser != null && bspParser.HasPakFile(vvdPath)) || (vpkParser != null && vpkParser.FileExists(vvdPath)))
                    {
                        if ((bspParser == null || !bspParser.HasPakFile(vtxPath)) && (vpkParser == null || !vpkParser.FileExists(vtxPath)))
                            vtxPath = modelPath + ".dx90.vtx";

                        if ((bspParser != null && bspParser.HasPakFile(vtxPath)) || (vpkParser != null && vpkParser.FileExists(vtxPath)))
                        {
                            using (MDLParser mdl = new MDLParser())
                            using (VVDParser vvd = new VVDParser())
                            using (VTXParser vtx = new VTXParser())
                            {
                                try
                                {
                                    if (bspParser != null && bspParser.HasPakFile(mdlPath))
                                        bspParser.LoadPakFileAsStream(mdlPath, (stream, origOffset, byteCount) => { mdl.ParseHeader(stream, origOffset); });
                                    else
                                        vpkParser.LoadFileAsStream(mdlPath, (stream, origOffset, byteCount) => { mdl.ParseHeader(stream, origOffset); });

                                    if (bspParser != null && bspParser.HasPakFile(vvdPath))
                                        bspParser.LoadPakFileAsStream(vvdPath, (stream, origOffset, byteCount) => { vvd.ParseHeader(stream, origOffset); });
                                    else
                                        vpkParser.LoadFileAsStream(vvdPath, (stream, origOffset, byteCount) => { vvd.ParseHeader(stream, origOffset); });

                                    int mdlChecksum = mdl.header1.checkSum;
                                    int vvdChecksum = (int)vvd.header.checksum;

                                    if (mdlChecksum == vvdChecksum)
                                    {
                                        if (bspParser != null && bspParser.HasPakFile(vtxPath))
                                            bspParser.LoadPakFileAsStream(vtxPath, (stream, origOffset, byteCount) => { vtx.ParseHeader(stream, origOffset); });
                                        else
                                            vpkParser.LoadFileAsStream(vtxPath, (stream, origOffset, byteCount) => { vtx.ParseHeader(stream, origOffset); });

                                        int vtxChecksum = vtx.header.checkSum;

                                        if (mdlChecksum == vtxChecksum)
                                        {
                                            if (bspParser != null && bspParser.HasPakFile(mdlPath))
                                                bspParser.LoadPakFileAsStream(mdlPath, (stream, origOffset, byteCount) => { mdl.Parse(stream, origOffset); });
                                            else
                                                vpkParser.LoadFileAsStream(mdlPath, (stream, origOffset, byteCount) => { mdl.Parse(stream, origOffset); });

                                            if (bspParser != null && bspParser.HasPakFile(vvdPath))
                                                bspParser.LoadPakFileAsStream(vvdPath, (stream, origOffset, byteCount) => { vvd.Parse(stream, mdl.header1.rootLod, origOffset); });
                                            else
                                                vpkParser.LoadFileAsStream(vvdPath, (stream, origOffset, byteCount) => { vvd.Parse(stream, mdl.header1.rootLod, origOffset); });

                                            if (bspParser != null && bspParser.HasPakFile(vtxPath))
                                                bspParser.LoadPakFileAsStream(vtxPath, (stream, origOffset, byteCount) => { vtx.Parse(stream, origOffset); });
                                            else
                                                vpkParser.LoadFileAsStream(vtxPath, (stream, origOffset, byteCount) => { vtx.Parse(stream, origOffset); });

                                            version = mdl.header1.version;
                                            id = mdl.header1.id;

                                            if (mdl.bodyParts != null)
                                                ReadFaceMeshes(mdl, vvd, vtx, bspParser, vpkParser);
                                            else
                                                Debug.LogError("SourceModel: Could not find body parts of " + modelPath);
                                        }
                                        else
                                            Debug.LogError("SourceModel: MDL and VTX checksums don't match (" + mdlChecksum + " != " + vtxChecksum + ") vtxver(" + vtx.header.version + ") for " + modelPath);
                                    }
                                    else
                                        Debug.LogError("SourceModel: MDL and VVD checksums don't match (" + mdlChecksum + " != " + vvdChecksum + ") for " + modelPath);
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
                            int vertexCount = 0;
                            //int vertexCount = mdl.bodyParts[bodyPartIndex].models[modelIndex].theMeshes[meshIndex].vertexCount;
                            //int vertexCount = mdl.bodyParts[bodyPartIndex].models[modelIndex].theMeshes[meshIndex].vertexData.lodVertexCount[rootLodIndex];
                            //int vertexCount = 0;
                            //for (int i = 0; i <= rootLodIndex; i++)
                            //    vertexCount += mdl.bodyParts[bodyPartIndex].models[modelIndex].theMeshes[meshIndex].vertexData.lodVertexCount[i];

                            int trianglesCount = 0;
                            for (int stripGroupIndex = 0; stripGroupIndex < vtx.bodyParts[bodyPartIndex].theVtxModels[modelIndex].theVtxModelLods[rootLodIndex].theVtxMeshes[meshIndex].stripGroupCount; stripGroupIndex++)
                            {
                                var currentStripGroup = vtx.bodyParts[bodyPartIndex].theVtxModels[modelIndex].theVtxModelLods[rootLodIndex].theVtxMeshes[meshIndex].theVtxStripGroups[stripGroupIndex];
                                for (int stripIndex = 0; stripIndex < currentStripGroup.theVtxStrips.Length; stripIndex++)
                                {
                                    var currentStrip = currentStripGroup.theVtxStrips[stripIndex];
                                    if (((StripHeaderFlags_t)currentStrip.flags & StripHeaderFlags_t.STRIP_IS_TRILIST) > 0)
                                        trianglesCount += currentStrip.indexCount;
                                }
                            }

                            //List<int> triangles = new List<int>();
                            int[] triangles = new int[trianglesCount];
                            int trianglesIndex = 0;
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

                                            vertexCount = Mathf.Max(vertexCount, vertexIndexA, vertexIndexB, vertexIndexC);
                                            //if (vertexIndexA < vertices.Length && vertexIndexB < vertices.Length && vertexIndexC < vertices.Length)
                                            //{
                                            triangles[trianglesIndex++] = vertexIndexA;
                                            triangles[trianglesIndex++] = vertexIndexB;
                                            triangles[trianglesIndex++] = vertexIndexC;
                                            //triangles.Add(vertexIndexA);
                                            //triangles.Add(vertexIndexB);
                                            //triangles.Add(vertexIndexC);
                                            //}
                                        }
                                    }
                                }
                            }
                            //}

                            vertexCount += 1;
                            //vertexCount = vvd.vertices.Length;
                            //vertexCount = vvd.header.numLODVertices[rootLodIndex];

                            Vector3[] vertices = new Vector3[vertexCount];
                            Vector3[] normals = new Vector3[vertexCount];
                            Vector2[] uv = new Vector2[vertexCount];

                            for (int verticesIndex = 0; verticesIndex < vertices.Length; verticesIndex++)
                            {
                                vertices[verticesIndex] = vvd.vertices[verticesIndex].m_vecPosition;
                                normals[verticesIndex] = vvd.vertices[verticesIndex].m_vecNormal;
                                uv[verticesIndex] = vvd.vertices[verticesIndex].m_vecTexCoord;
                            }

                            Debug.Assert(triangles.Length % 3 == 0, "SourceModel: Triangles not a multiple of three for " + modelPath);
                            if (mdl.header1.includemodel_count > 0)
                                Debug.LogWarning("SourceModel: Include model count greater than zero (" + mdl.header1.includemodel_count + ", " + mdl.header1.includemodel_index + ") for " + modelPath);
                            if (vvd.header.numFixups > 0)
                                Debug.LogWarning("SourceModel: " + vvd.header.numFixups + " fixups found for " + modelPath);

                            MeshHelpers.MeshData meshData;

                            //var outcome = MeshHelpers.GenerateConvexHull(vertices, out meshData, 0.2);
                            //if (outcome != MIConvexHull.ConvexHullCreationResultOutcome.Success)
                            //    Debug.LogError("SourceModel: Convex hull error " + outcome + " for " + modelPath);

                            if (decimationPercent > 0)
                            {
                                meshData = MeshHelpers.DecimateByTriangleCount(vertices, triangles, normals, 1 - decimationPercent);
                                meshData.uv = new Vector2[meshData.vertices.Length];
                                System.Array.Copy(uv, meshData.uv, meshData.vertices.Length);
                            }
                            else
                            {
                                meshData = new MeshHelpers.MeshData();
                                meshData.vertices = vertices;
                                meshData.triangles = triangles;
                                meshData.normals = normals;
                                meshData.uv = uv;
                            }

                            currentFace.meshData = meshData;

                            string textureName = "";
                            string texturePath = mdl.texturePaths[0].Replace("\\", "/").ToLower();
                            if (textureIndex < mdl.textures.Length)
                                textureName = mdl.textures[textureIndex].name.Replace("\\", "/").ToLower();
                            //textureName = mdl.textures[textureIndex].name.Replace("\\", "/").ToLower();
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
                Debug.LogError("SourceModel: MDL and VTX body part count doesn't match (" + modelPath + ")");
        }

        private void BuildPrefab()
        {
            modelPrefab = new GameObject("StaticProp_v" + version + "_id" + id + "_" + modelPath);
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