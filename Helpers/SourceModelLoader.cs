﻿using UnityEngine;
using UnitySourceEngine;

public class SourceModelLoader : MonoBehaviour
{
    public string vpkPath = @"C:\Program Files (x86)\Steam\steamapps\common\Counter-Strike Global Offensive\csgo";
    public string modelPath = "models/weapons/v_rif_m4a1.mdl";
    [Space(10)]
    public bool excludeTextures = false;
    public bool flatTextures = false;
    public int maxTextureSize = 2048;

    private SourceModel model;

    private void Start()
    {
        model = LoadModel(vpkPath, modelPath, excludeTextures, flatTextures, maxTextureSize);
        if (model != null)
            model.InstantiateGameObject().transform.SetParent(transform, false);
    }
    private void OnDestroy()
    {
        if (model != null)
            model.Dispose();
        model = null;
    }

    public SourceModel LoadModel(string vpkLoc, string modelLoc, bool excludeTextures = false, bool flatTextures = false, int maxTextureSize = 2048)
    {
        SourceModel.excludeTextures = excludeTextures;
        SourceTexture.averageTextures = flatTextures;
        SourceTexture.maxTextureSize = maxTextureSize;

        SourceModel model = null;
        using (VPKParser vpk = new VPKParser(vpkPath))
            model = SourceModel.GrabModel(vpk, modelPath);
        return model;
    }
}