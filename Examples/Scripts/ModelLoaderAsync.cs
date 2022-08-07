using System.Collections;
using UnityEngine;
#if UNITY_EDITOR
using UnityHelpers.Editor;
#endif

namespace UnitySourceEngine.Examples
{
    public class ModelLoaderAsync : MonoBehaviour
    {
        public UnityEngine.UI.Text statusLabel;
        public UnityEngine.UI.Image loadingBar;

        public string vpkPath = @"C:\Program Files (x86)\Steam\steamapps\common\Counter-Strike Global Offensive\csgo";
        public string modelPath = "models/weapons/v_rif_m4a1";
        [Space(10)]
        public bool flatTextures = false;
        public int maxTextureSize = 2048;

        private SourceModel model;
        private UnityHelpers.TaskWrapper loadingTask;

        #if UNITY_EDITOR
        [Space(10), Debug(true, true)]
        #endif
        public string header1 = "";
        #if UNITY_EDITOR
        [Debug(true, true)]
        #endif
        public string header2 = "";

        private void Update()
        {
            if (model != null)
            {
                statusLabel.text = model.currentMessage;
                loadingBar.fillAmount = model.PercentLoaded;
            }
        }
        private void OnEnable()
        {
            model = LoadModel(vpkPath, modelPath, flatTextures, maxTextureSize);
        }
        private void OnDisable()
        {
            if (loadingTask != null && UnityHelpers.TaskManagerController.HasTask(loadingTask))
                UnityHelpers.TaskManagerController.CancelTask(loadingTask);

            if (model != null)
                model.Dispose();
            model = null;
        }

        public SourceModel LoadModel(string vpkLoc, string modelLoc, bool flatTextures = false, int maxTextureSize = 2048)
        {
            SourceTexture.averageTextures = flatTextures;
            SourceTexture.maxTextureSize = maxTextureSize;

            SourceModel model = new SourceModel(modelPath);

            loadingTask = UnityHelpers.TaskManagerController.RunActionAsync("Parsing Map", (cancelToken) =>
            {
                using (VPKParser vpk = new VPKParser(vpkPath))
                    model.Parse(null, vpk, cancelToken, () =>
                    {
                        header1 = model.header1.ToString();
                        header2 = model.header2.ToString();
                        if (!loadingTask.cancelled)
                            UnityHelpers.TaskManagerController.RunAction(() => {
                                model.Build();
                                loadingBar.transform.root.gameObject.SetActive(false);
                                Debug.Log("Loaded " + model.key);
                            });
                    });
            });

            return model;
        }
    }
}