using UnityEngine;
using UnitySourceEngine;

public class LoadMapExample : MonoBehaviour
{
    public UnityEngine.UI.Text statusLabel;
    public UnityEngine.UI.Image loadingBar;

    public string vpkPath = @"C:\Program Files (x86)\Steam\steamapps\common\Counter-Strike Global Offensive\csgo";
    public string mapPath = @"C:\Program Files (x86)\Steam\steamapps\common\Counter-Strike Global Offensive\csgo\maps\ar_shoots.bsp";
    [Space(10)]
    public bool combineMeshesWithSameTextures = true;
    public bool excludeMapFaces = false;
    public bool excludeModels = false;
    public bool excludeMapTextures = false;
    public bool excludeModelTextures = false;
    public bool flatTextures = false;
    public int maxTextureSize = 2048;

    private BSPMap map;
    private UnityHelpers.TaskWrapper loadingTask;

    private void Update()
    {
        if (map != null)
        {
            statusLabel.text = map.currentMessage;
            loadingBar.fillAmount = map.PercentLoaded;
        }
    }
    private void OnEnable()
    {
        map = LoadMap(vpkPath, mapPath, combineMeshesWithSameTextures, excludeMapFaces, excludeModels, excludeMapTextures, excludeModelTextures, flatTextures, maxTextureSize);
    }
    private void OnDisable()
    {
        if (loadingTask != null && UnityHelpers.TaskManagerController.HasTask(loadingTask))
            UnityHelpers.TaskManagerController.CancelTask(loadingTask);

        if (map != null)
            map.Unload();
        map = null;
    }

    public BSPMap LoadMap(string vpkLoc, string mapLoc, bool combineMeshesWithSameTextures = true, bool excludeMapFaces = false, bool excludeModels = false, bool excludeMapTextures = false, bool excludeModelTextures = false, bool flatTextures = false, int maxTextureSize = 2048)
    {
        BSPMap.vpkLoc = vpkLoc;
        BSPMap map = new BSPMap(mapLoc);

        BSPMap.combineMeshesWithSameTextures = combineMeshesWithSameTextures;
        BSPMap.excludeMapFaces = excludeMapFaces;
        BSPMap.excludeModels = excludeModels;
        BSPMap.excludeTextures = excludeMapTextures;
        SourceModel.excludeTextures = excludeModelTextures;
        SourceTexture.averageTextures = flatTextures;
        SourceTexture.maxTextureSize = maxTextureSize;

        loadingTask = UnityHelpers.TaskManagerController.RunActionAsync("Parsing Map", (cts) => { map.ParseFile(cts.Token, null, () =>
        {
            if (!loadingTask.cancelled)
                UnityHelpers.TaskManagerController.RunAction(() => { map.MakeGameObject(null, (go) => { go.SetActive(true); }); });
        }); });

        return map;
    }
}
