using UnityEngine;
using UnitySourceEngine;

public class LoadMapAsyncExample : MonoBehaviour
{
    public UnityEngine.UI.Text statusLabel;
    public UnityEngine.UI.Image loadingBar;

    public string vpkPath = @"C:\Program Files (x86)\Steam\steamapps\common\Counter-Strike Global Offensive\csgo";
    public string mapPath = @"C:\Program Files (x86)\Steam\steamapps\common\Counter-Strike Global Offensive\csgo\maps\ar_monastery.bsp";
    [Space(10)]
    public bool combineMeshesWithSameTextures = true;
    [Range(0, 1)]
    public float faceLoadPercent = 1;
    [Range(0, 1)]
    public float modelLoadPercent = 1;
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
        map = LoadMap(vpkPath, mapPath, combineMeshesWithSameTextures, faceLoadPercent, modelLoadPercent, flatTextures, maxTextureSize);
    }
    private void OnDisable()
    {
        if (loadingTask != null && UnityHelpers.TaskManagerController.HasTask(loadingTask))
            UnityHelpers.TaskManagerController.CancelTask(loadingTask);

        if (map != null)
            map.Unload();
        map = null;
    }

    public BSPMap LoadMap(string vpkLoc, string mapLoc, bool combineMeshesWithSameTextures = true, float faceLoadPercent = 1, float modelLoadPercent = 1, bool flatTextures = false, int maxTextureSize = 2048)
    {
        BSPMap.vpkLoc = vpkLoc;
        BSPMap map = new BSPMap(mapLoc);

        BSPMap.combineMeshesWithSameTexture = combineMeshesWithSameTextures;
        BSPMap.FaceLoadPercent = faceLoadPercent;
        BSPMap.ModelLoadPercent = modelLoadPercent;
        SourceTexture.averageTextures = flatTextures;
        SourceTexture.maxTextureSize = maxTextureSize;

        loadingTask = UnityHelpers.TaskManagerController.RunActionAsync("Parsing Map", (cancelToken) => { map.ParseFile(cancelToken, null, () =>
        {
            if (!loadingTask.cancelled)
                UnityHelpers.TaskManagerController.RunAction(() => { map.MakeGameObject(null, (go) => { go.SetActive(true); }); });
        }); });

        return map;
    }
}
