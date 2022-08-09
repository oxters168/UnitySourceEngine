using UnityEngine;
using System.Collections.Generic;
using UnityHelpers;
using System.Threading;

namespace UnitySourceEngine
{
    public class SourceModel
    {
        // private static GameObject staticPropLibrary;

        public static float decimationPercent = 0;

        public string currentMessage { get; private set; }
        public float PercentLoaded { get; private set; }

        public string modelPath { get; private set; }
        public string key { get { return KeyFromPath(modelPath); } }

        public int version { get; private set; }
        public int id { get; private set; }
        public MDLData mdl;
        public VVDData vvd;
        public VTXData vtx;
        private List<FaceMesh> faces = new List<FaceMesh>();

        public SourceModel(string _modelPath)
        {
            modelPath = _modelPath.Replace("\\", "/");//.ToLower();
        }

        public void Dispose()
        {
            if (faces != null)
                foreach (FaceMesh face in faces)
                    face?.Dispose();
            faces = null;

            // if (modelPrefab != null)
            //     Object.Destroy(modelPrefab);
            // modelPrefab = null;
        }

        public static string KeyFromPath(string path)
        {
            return path.Replace("\\", "/").ToLower();
        }

        public void Parse(BSPParser bspParser, VPKParser vpkParser, CancellationToken? cancelToken = null, System.Action onFinished = null)
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
                            using (MDLParser mdlParser = new MDLParser())
                            using (VVDParser vvdParser = new VVDParser())
                            using (VTXParser vtxParser = new VTXParser())
                            {
                                try
                                {
                                    currentMessage = "Reading MDL data";
                                    if (!(cancelToken?.IsCancellationRequested ?? false) && bspParser != null && bspParser.HasPakFile(mdlPath))
                                        bspParser.LoadPakFileAsStream(mdlPath, (stream, origOffset, byteCount) => { mdl = mdlParser.Parse(stream, origOffset); });
                                    else if (!(cancelToken?.IsCancellationRequested ?? false))
                                        vpkParser.LoadFileAsStream(mdlPath, (stream, origOffset, byteCount) => { mdl = mdlParser.Parse(stream, origOffset); });
                                    PercentLoaded = 1f / 6;

                                    currentMessage = "Reading VVD header";
                                    if (!(cancelToken?.IsCancellationRequested ?? false) && bspParser != null && bspParser.HasPakFile(vvdPath))
                                        bspParser.LoadPakFileAsStream(vvdPath, (stream, origOffset, byteCount) => { vvd = vvdParser.Parse(stream, origOffset, mdl.header1.rootLod); });
                                    else if (!(cancelToken?.IsCancellationRequested ?? false))
                                        vpkParser.LoadFileAsStream(vvdPath, (stream, origOffset, byteCount) => { vvd = vvdParser.Parse(stream, origOffset, mdl.header1.rootLod); });
                                    PercentLoaded = 2f / 6;

                                    int mdlChecksum = mdl.header1.checkSum;
                                    int vvdChecksum = (int)vvd.header.checksum;

                                    if (mdlChecksum == vvdChecksum)
                                    {
                                        currentMessage = "Reading VTX header";
                                        if (!(cancelToken?.IsCancellationRequested ?? false) && bspParser != null && bspParser.HasPakFile(vtxPath))
                                            bspParser.LoadPakFileAsStream(vtxPath, (stream, origOffset, byteCount) => { vtx = vtxParser.Parse(stream, origOffset); });
                                        else if (!(cancelToken?.IsCancellationRequested ?? false))
                                            vpkParser.LoadFileAsStream(vtxPath, (stream, origOffset, byteCount) => { vtx = vtxParser.Parse(stream, origOffset); });
                                        PercentLoaded = 3f / 6;

                                        int vtxChecksum = vtx.header.checkSum;

                                        if (mdlChecksum == vtxChecksum)
                                        {
                                            // currentMessage = "Parsing MDL";
                                            // if (!(cancelToken?.IsCancellationRequested ?? false) && bspParser != null && bspParser.HasPakFile(mdlPath))
                                            //     bspParser.LoadPakFileAsStream(mdlPath, (stream, origOffset, byteCount) => { mdlParser.Parse(stream, origOffset); });
                                            // else if (!(cancelToken?.IsCancellationRequested ?? false))
                                            //     vpkParser.LoadFileAsStream(mdlPath, (stream, origOffset, byteCount) => { mdlParser.Parse(stream, origOffset); });
                                            // PercentLoaded = 4f / 7;

                                            // currentMessage = "Parsing VVD";
                                            // if (!(cancelToken?.IsCancellationRequested ?? false) && bspParser != null && bspParser.HasPakFile(vvdPath))
                                            //     bspParser.LoadPakFileAsStream(vvdPath, (stream, origOffset, byteCount) => { vvdParser.Parse(stream, mdl.header1.rootLod, origOffset); });
                                            // else if (!(cancelToken?.IsCancellationRequested ?? false))
                                            //     vpkParser.LoadFileAsStream(vvdPath, (stream, origOffset, byteCount) => { vvdParser.Parse(stream, mdl.header1.rootLod, origOffset); });
                                            // PercentLoaded = 4f / 6;

                                            // currentMessage = "Parsing VTX";
                                            // if (!(cancelToken?.IsCancellationRequested ?? false) && bspParser != null && bspParser.HasPakFile(vtxPath))
                                            //     bspParser.LoadPakFileAsStream(vtxPath, (stream, origOffset, byteCount) => { vtxParser.Parse(stream, origOffset); });
                                            // else if (!(cancelToken?.IsCancellationRequested ?? false))
                                            //     vpkParser.LoadFileAsStream(vtxPath, (stream, origOffset, byteCount) => { vtxParser.Parse(stream, origOffset); });
                                            // PercentLoaded = 5f / 6;

                                            version = mdl.header1.version;
                                            id = mdl.header1.id;

                                            currentMessage = "Converting to mesh";
                                            if (!(cancelToken?.IsCancellationRequested ?? false) && mdl.bodyParts != null)
                                                ReadFaceMeshes(mdlParser, vvdParser, vtxParser, bspParser, vpkParser);
                                            else if (!(cancelToken?.IsCancellationRequested ?? false))
                                                Debug.LogError("SourceModel: Could not find body parts of " + modelPath);
                                            PercentLoaded = 1;
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

            onFinished?.Invoke();
        }
        private void ReadFaceMeshes(MDLParser mdl, VVDParser vvd, VTXParser vtx, BSPParser bspParser, VPKParser vpkParser)
        {
            int textureIndex = 0;
            if (this.mdl.bodyParts.Length == this.vtx.bodyParts.Length)
            {
                for (int bodyPartIndex = 0; bodyPartIndex < this.mdl.bodyParts.Length; bodyPartIndex++)
                {
                    for (int modelIndex = 0; modelIndex < this.mdl.bodyParts[bodyPartIndex].models.Length; modelIndex++)
                    {
                        //int currentPosition = 0;
                        for (int meshIndex = 0; meshIndex < this.mdl.bodyParts[bodyPartIndex].models[modelIndex].meshes.Length; meshIndex++)
                        {
                            int rootLodIndex = this.mdl.header1.rootLod;
                            //int rootLodIndex = 0;
                            //int rootLodCount = 1;
                            //if (mdl.header1.numAllowedRootLods == 0)
                            //    rootLodCount = vtx.header.numLODs;

                            FaceMesh currentFace = new FaceMesh();

                            int verticesStartIndex = this.mdl.bodyParts[bodyPartIndex].models[modelIndex].meshes[meshIndex].vertexIndexStart;
                            int vertexCount = 0;
                            //int vertexCount = mdl.bodyParts[bodyPartIndex].models[modelIndex].theMeshes[meshIndex].vertexCount;
                            //int vertexCount = mdl.bodyParts[bodyPartIndex].models[modelIndex].theMeshes[meshIndex].vertexData.lodVertexCount[rootLodIndex];
                            //int vertexCount = 0;
                            //for (int i = 0; i <= rootLodIndex; i++)
                            //    vertexCount += mdl.bodyParts[bodyPartIndex].models[modelIndex].theMeshes[meshIndex].vertexData.lodVertexCount[i];

                            int trianglesCount = 0;
                            for (int stripGroupIndex = 0; stripGroupIndex < this.vtx.bodyParts[bodyPartIndex].vtxModels[modelIndex].vtxModelLods[rootLodIndex].vtxMeshes[meshIndex].stripGroupCount; stripGroupIndex++)
                            {
                                var currentStripGroup = this.vtx.bodyParts[bodyPartIndex].vtxModels[modelIndex].vtxModelLods[rootLodIndex].vtxMeshes[meshIndex].vtxStripGroups[stripGroupIndex];
                                for (int stripIndex = 0; stripIndex < currentStripGroup.vtxStrips.Length; stripIndex++)
                                {
                                    var currentStrip = currentStripGroup.vtxStrips[stripIndex];
                                    if (((StripHeaderFlags_t)currentStrip.flags & StripHeaderFlags_t.STRIP_IS_TRILIST) > 0)
                                        trianglesCount += currentStrip.indexCount;
                                }
                            }

                            //List<int> triangles = new List<int>();
                            int[] triangles = new int[trianglesCount];
                            int trianglesIndex = 0;
                            //for (int countUpLodIndex = 0; countUpLodIndex <= rootLodIndex; countUpLodIndex++)
                            //{
                            for (int stripGroupIndex = 0; stripGroupIndex < this.vtx.bodyParts[bodyPartIndex].vtxModels[modelIndex].vtxModelLods[rootLodIndex].vtxMeshes[meshIndex].stripGroupCount; stripGroupIndex++)
                            {
                                //var currentStripGroup = vtx.bodyParts[bodyPartIndex].theVtxModels[modelIndex].theVtxModelLods[rootLodIndex].theVtxMeshes[meshIndex].theVtxStripGroups[0];
                                var currentStripGroup = this.vtx.bodyParts[bodyPartIndex].vtxModels[modelIndex].vtxModelLods[rootLodIndex].vtxMeshes[meshIndex].vtxStripGroups[stripGroupIndex];
                                //int trianglesCount = currentStripGroup.theVtxIndices.Length;
                                //int[] triangles = new int[trianglesCount];

                                for (int stripIndex = 0; stripIndex < currentStripGroup.vtxStrips.Length; stripIndex++)
                                {
                                    var currentStrip = currentStripGroup.vtxStrips[stripIndex];

                                    if (((StripHeaderFlags_t)currentStrip.flags & StripHeaderFlags_t.STRIP_IS_TRILIST) > 0)
                                    {
                                        for (int indexIndex = 0; indexIndex < currentStrip.indexCount; indexIndex += 3)
                                        {
                                            int vertexIndexA = verticesStartIndex + currentStripGroup.vtxVertices[currentStripGroup.vtxIndices[indexIndex + currentStrip.indexMeshIndex]].originalMeshVertexIndex;
                                            int vertexIndexB = verticesStartIndex + currentStripGroup.vtxVertices[currentStripGroup.vtxIndices[indexIndex + currentStrip.indexMeshIndex + 2]].originalMeshVertexIndex;
                                            int vertexIndexC = verticesStartIndex + currentStripGroup.vtxVertices[currentStripGroup.vtxIndices[indexIndex + currentStrip.indexMeshIndex + 1]].originalMeshVertexIndex;

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
                                vertices[verticesIndex] = this.vvd.vertices[verticesIndex].m_vecPosition;
                                normals[verticesIndex] = this.vvd.vertices[verticesIndex].m_vecNormal;
                                uv[verticesIndex] = this.vvd.vertices[verticesIndex].m_vecTexCoord;
                            }

                            Debug.Assert(triangles.Length % 3 == 0, "SourceModel: Triangles not a multiple of three for " + modelPath);
                            if (this.mdl.header1.includemodel_count > 0)
                                Debug.LogWarning("SourceModel: Include model count greater than zero (" + this.mdl.header1.includemodel_count + ", " + this.mdl.header1.includemodel_index + ") for " + modelPath);
                            if (this.vvd.header.numFixups > 0)
                                Debug.LogWarning("SourceModel: " + this.vvd.header.numFixups + " fixups found for " + modelPath);

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
                            string texturePath = this.mdl.texturePaths[0].Replace("\\", "/").ToLower();
                            if (textureIndex < this.mdl.textures.Length)
                                textureName = this.mdl.textures[textureIndex].name.Replace("\\", "/").ToLower();
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

        public GameObject Build()
        {
            var modelGO = new GameObject(mdl.header1.name);

            if (faces != null)
                foreach (FaceMesh faceMesh in faces)
                {
                    GameObject meshRepresentation = new GameObject(faceMesh.faceName);
                    meshRepresentation.transform.parent = modelGO.transform;

                    Mesh mesh = faceMesh.meshData.GenerateMesh();
                    mesh.name = faceMesh.faceName;

                    MeshFilter mesher = meshRepresentation.AddComponent<MeshFilter>();
                    mesher.sharedMesh = mesh;

                    meshRepresentation.AddComponent<MeshRenderer>().material = faceMesh.material?.GetMaterial();
                    // meshRepresentation.AddComponent<MeshCollider>();
                }

            return modelGO;
        }
    }
}