using UnityEngine;
using UnitySourceEngine;

public class SourceMapLoader : MonoBehaviour
{
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

    private void Start()
    {
        map = LoadMap(vpkPath, mapPath, combineMeshesWithSameTextures, faceLoadPercent, modelLoadPercent, flatTextures, maxTextureSize);
    }
    private void OnDestroy()
    {
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

        map.ParseFile(System.Threading.CancellationToken.None);
        map.MakeGameObject(null, (go) => { go.transform.SetParent(transform, false); go.SetActive(true); });

        return map;
    }
}
