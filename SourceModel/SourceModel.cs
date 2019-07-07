using UnityEngine;
using System.Collections.Generic;

public class SourceModel
{
    public static Material modelMaterial;
    private static Dictionary<string, SourceModel> loadedModels = new Dictionary<string, SourceModel>();
    private static GameObject staticPropLibrary;
    public static bool excludeTextures;

    public string modelName { get; private set; }
    public string modelLocation { get; private set; }
    private string modelKey { get { return modelLocation + modelName; } }

    private GameObject modelPrefab;
    private List<FaceMesh> faces = new List<FaceMesh>();
    private SourceTexture[] modelTextures;
    private List<Material> materialsCreated = new List<Material>();

    public Vector3 origin, angles;

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
        {
            loadedModels.Remove(modelKey);
            Debug.Log("Model removed, unique model count: " + loadedModels.Count);
        }

        if (materialsCreated != null)
            foreach (Material mat in materialsCreated)
                if (mat != null)
                    Object.Destroy(mat);
        materialsCreated = new List<Material>();

        if (faces != null)
            foreach (FaceMesh face in faces)
                face?.Dispose();
        faces = null;

        if (modelTextures != null)
            foreach (SourceTexture texture in modelTextures)
                texture?.Dispose();
        modelTextures = null;

        if (modelPrefab != null)
            Object.Destroy(modelPrefab);
        modelPrefab = null;
    }

    public static SourceModel GrabModel(VPKParser vpkParser, string fullModelPath)
    {
        SourceModel model = null;

        string modelName = "";
        string modelLocation = fullModelPath.Replace("\\", "/").ToLower();

        if(modelLocation.IndexOf("/") > -1)
        {
            modelName = modelLocation.Substring(modelLocation.LastIndexOf("/") + 1);
            modelLocation = modelLocation.Substring(0, modelLocation.LastIndexOf("/") + 1);

            model = GrabModel(vpkParser, modelName, modelLocation);
        }

        return model;
    }
    public static SourceModel GrabModel(VPKParser vpkParser, string name, string location)
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
            Debug.Log("Model created, unique model count: " + loadedModels.Count);
            model.Parse(vpkParser);
        }

        return model;
    }
    private void Parse(VPKParser vpkParser)
    {
        if (vpkParser != null)
        {
            string modelsVPKDir = ((modelLocation.IndexOf("/") == 0) ? "/models" : "/models/");
            string mdlPath = modelsVPKDir + modelLocation + modelName + ".mdl";
            string vvdPath = modelsVPKDir + modelLocation + modelName + ".vvd";
            string vtxPath = modelsVPKDir + modelLocation + modelName + ".vtx";

            if (vpkParser.FileExists(mdlPath))
            {
                if (vpkParser.FileExists(vvdPath))
                {
                    if (!vpkParser.FileExists(vtxPath))
                        vtxPath = modelsVPKDir + modelLocation + modelName + ".dx90.vtx";

                    if (vpkParser.FileExists(vtxPath))
                    {
                        using (MDLParser mdl = new MDLParser())
                        using (VVDParser vvd = new VVDParser())
                        using (VTXParser vtx = new VTXParser())
                        {
                            vpkParser.LoadFileAsStream(mdlPath, (stream, origOffset, byteCount) => { mdl.Parse(stream, origOffset); });
                            vpkParser.LoadFileAsStream(vvdPath, (stream, origOffset, byteCount) => { vvd.Parse(stream, origOffset); });
                            vpkParser.LoadFileAsStream(vtxPath, (stream, origOffset, byteCount) => { vtx.Parse(stream, origOffset); });

                            if (mdl.bodyParts == null)
                            {
                                Debug.LogError("SourceModel: Could not find body parts of " + modelKey);
                                return;
                            }

                            GetTextures(mdl, vpkParser);
                            ReadFaceMeshes(mdl, vvd, vtx, vpkParser);
                        }
                    }
                    else
                        Debug.LogError("SourceModel: Could not find vtx file in vpk (" + vtxPath + ")");
                }
                else
                    Debug.LogError("SourceModel: Could not find vvd file in vpk (" + vvdPath + ")");
            }
            else
                Debug.LogError("SourceModel: Could not find mdl file in vpk (" + mdlPath + ")");
        }
        else
            Debug.Log("SourceModel: VPK parser is null");
    }
    private void GetTextures(MDLParser mdl, VPKParser vpkParser)
    {
        if (mdl != null && vpkParser != null)
        {
            try
            {
                #region Grabbing Textures
                modelTextures = new SourceTexture[mdl.textures.Length];
                for (int i = 0; i < modelTextures.Length; i++)
                {
                    string texturePath = "", textureName = "";
                    if (mdl.texturePaths != null && mdl.texturePaths.Length > 0 && mdl.texturePaths[0] != null)
                        texturePath = mdl.texturePaths[0].Replace("\\", "/").ToLower();
                    if (mdl.textures[i] != null)
                        textureName = mdl.textures[i].name.Replace("\\", "/").ToLower();
                    if (textureName.IndexOf(texturePath) > -1)
                        texturePath = "";
                    modelTextures[i] = SourceTexture.GrabTexture(vpkParser, texturePath + textureName);
                }
                #endregion
            }
            catch(System.Exception e)
            {
                Debug.LogError("SourceModel: " + e);
            }
        }
    }
    private void ReadFaceMeshes(MDLParser mdl, VVDParser vvd, VTXParser vtx, VPKParser vpkParser)
    {
        int textureIndex = 0;
        if (mdl.bodyParts.Length == vtx.bodyParts.Length)
        {
            for (int i = 0; i < mdl.bodyParts.Length; i++)
            {
                for (int j = 0; j < mdl.bodyParts[i].models.Length; j++)
                {
                    int currentPosition = 0;
                    for (int k = 0; k < mdl.bodyParts[i].models[j].theMeshes.Length; k++)
                    {
                        FaceMesh currentFace = new FaceMesh();

                        Vector3[] vertices = new Vector3[mdl.bodyParts[i].models[j].theMeshes[k].vertexData.lodVertexCount[mdl.header1.rootLod]];
                        Vector3[] normals = new Vector3[mdl.bodyParts[i].models[j].theMeshes[k].vertexData.lodVertexCount[mdl.header1.rootLod]];
                        Vector2[] uv = new Vector2[mdl.bodyParts[i].models[j].theMeshes[k].vertexData.lodVertexCount[mdl.header1.rootLod]];
                        for (int l = 0; l < vertices.Length; l++)
                        {
                            if (currentPosition < vvd.vertices[mdl.header1.rootLod].Length)
                                vertices[l] = vvd.vertices[mdl.header1.rootLod][currentPosition].m_vecPosition;
                            if (currentPosition < vvd.vertices[mdl.header1.rootLod].Length)
                                normals[l] = vvd.vertices[mdl.header1.rootLod][currentPosition].m_vecNormal;
                            if (currentPosition < vvd.vertices[mdl.header1.rootLod].Length)
                                uv[l] = vvd.vertices[mdl.header1.rootLod][currentPosition].m_vecTexCoord;
                            currentPosition++;
                        }

                        int[] triangles = new int[vtx.bodyParts[i].theVtxModels[j].theVtxModelLods[0].theVtxMeshes[k].theVtxStripGroups[0].theVtxIndices.Length];
                        for (int l = 0; l < vtx.bodyParts[i].theVtxModels[j].theVtxModelLods[0].theVtxMeshes[k].theVtxStripGroups[0].theVtxIndices.Length; l++)
                        {
                            triangles[l + 0] = vtx.bodyParts[i].theVtxModels[j].theVtxModelLods[0].theVtxMeshes[k].theVtxStripGroups[0].theVtxVertices[vtx.bodyParts[i].theVtxModels[j].theVtxModelLods[0].theVtxMeshes[k].theVtxStripGroups[0].theVtxIndices[l + 0]].originalMeshVertexIndex;
                        }

                        MeshData meshData = new MeshData();
                        meshData.vertices = vertices;
                        meshData.triangles = triangles;
                        meshData.normals = normals;
                        meshData.uv = uv;

                        currentFace.meshData = meshData;
                        string textureLocation = "";
                        if (modelTextures != null && textureIndex < modelTextures.Length)
                            textureLocation = modelTextures[textureIndex]?.location;
                        textureIndex++;

                        currentFace.faceName = textureLocation;
                        if (!excludeTextures)
                            currentFace.texture = SourceTexture.GrabTexture(vpkParser, textureLocation);
                        faces.Add(currentFace);
                    }
                }
            }
        }
        else
            Debug.Log("SourceModel: MDL and VTX body part count doesn't match (" + modelLocation + ")");
    }

    private void BuildPrefab()
    {
        modelPrefab = new GameObject(modelName);
        modelPrefab.transform.parent = staticPropLibrary.transform;
        modelPrefab.SetActive(false);

        Material materialPrefab = modelMaterial;
        bool destroyMatAfterBuild = modelMaterial == null;
        if (destroyMatAfterBuild)
            materialPrefab = new Material(Shader.Find("Legacy Shaders/Diffuse"));
        if (faces != null)
            foreach (FaceMesh faceMesh in faces)
            {
                GameObject meshRepresentation = new GameObject("ModelPart");
                meshRepresentation.transform.parent = modelPrefab.transform;

                Mesh mesh = faceMesh.meshData.GetMesh();

                MeshFilter mesher = meshRepresentation.AddComponent<MeshFilter>();
                mesher.sharedMesh = mesh;

                Material meshMaterial = new Material(materialPrefab);
                materialsCreated.Add(meshMaterial);
                meshMaterial.mainTexture = faceMesh.texture?.GetTexture();
                meshRepresentation.AddComponent<MeshRenderer>().material = meshMaterial;
                meshRepresentation.AddComponent<MeshCollider>();
            }
        if (destroyMatAfterBuild)
            Object.Destroy(materialPrefab);
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
