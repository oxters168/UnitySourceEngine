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
        [Debug]
        #endif
        public string byteCount = "";
        #if UNITY_EDITOR
        [Space(10), Debug(true, true)]
        #endif
        public string mdlHeader1 = "";
        #if UNITY_EDITOR
        [Debug(true, true)]
        #endif
        public string mdlHeader2 = "";
        #if UNITY_EDITOR
        [Debug(true, true)]
        #endif
        public string vtxHeader = "";
        #if UNITY_EDITOR
        [Debug(true, true)]
        #endif
        public string vvdHeader = "";
        #if UNITY_EDITOR
        [Debug(true, true)]
        #endif
        public string mdlBones = "";

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
                        ulong mdlBytes = model.mdl.CountBytes();
                        ulong vtxBytes = model.vtx.CountBytes();
                        ulong vvdBytes = model.vvd.CountBytes();
                        ulong totalBytes = mdlBytes + vtxBytes + vvdBytes;
                        byteCount = "MDL:\n    " + mdlBytes + " bytes\n    " + (mdlBytes / 1000f) + " kb\n    " + (mdlBytes / 1000000.0) + " mb" + "\n\nVTX:\n    " + vtxBytes + " bytes\n    " + (vtxBytes / 1000f) + " kb\n    " + (vtxBytes / 1000000.0) + " mb" + "\n\nVVD:\n    " + vvdBytes + " bytes\n    " + (vvdBytes / 1000f) + " kb\n    " + (vvdBytes / 1000000.0) + " mb" + "\n\nTotal:\n    " + totalBytes + " bytes\n    " + (totalBytes / 1000f) + " kb\n    " + (totalBytes / 1000000.0) + " mb";
                        mdlHeader1 = model.mdl.header1.ToString();
                        mdlHeader2 = model.mdl.header2.ToString();
                        vtxHeader = model.vtx.header.ToString();
                        vvdHeader = model.vvd.header.ToString();
                        if (model.mdl.bones != null)
                            for (int i = 0; i < model.mdl.bones.Length; i++)
                                mdlBones += "bones[" + i + "]:\n" + model.mdl.bones[i].ToString() + (i < model.mdl.bones.Length - 1 ? "\n\n" : "");

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