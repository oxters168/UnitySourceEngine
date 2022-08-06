using UnityEngine;

namespace UnitySourceEngine.Examples
{
    public class ModelLoader : MonoBehaviour
    {
        public string vpkPath = @"C:\Program Files (x86)\Steam\steamapps\common\Counter-Strike Global Offensive\csgo";
        public string modelPath = "models/weapons/v_rif_m4a1";
        [Space(10)]
        public bool flatTextures = false;
        public int maxTextureSize = 2048;

        private SourceModel model;

        private void Start()
        {
            model = LoadModel(vpkPath, modelPath, flatTextures, maxTextureSize);
            if (model != null)
                model.InstantiateGameObject().transform.SetParent(transform, false);
        }
        private void OnDestroy()
        {
            if (model != null)
                model.Dispose();
            model = null;
        }

        public SourceModel LoadModel(string vpkLoc, string modelLoc, bool flatTextures = false, int maxTextureSize = 2048)
        {
            SourceTexture.averageTextures = flatTextures;
            SourceTexture.maxTextureSize = maxTextureSize;

            SourceModel model = new SourceModel(modelPath);
            using (VPKParser vpk = new VPKParser(vpkPath))
                model.Parse(null, vpk);
            //     model = SourceModel.GrabModel(null, vpk, modelPath);
            model.InstantiateGameObject();
            return model;
        }
    }
}