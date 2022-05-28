namespace UnitySourceEngine
{
    public class SourceLightmap
    {
        public UnityEngine.Color[] lightmapColors;
        public int width, height;
        public int lightmapIndex;
        private UnityEngine.Texture2D texture;

        public void Dispose()
        {
            lightmapColors = null;
            UnityEngine.Object.Destroy(texture);
        }
        public UnityEngine.Texture2D GetTexture()
        {
            if (texture == null)
            {
                //var pixels = new UnityEngine.Color[lightmapColors.Length];
                //for (int i = 0; i < pixels.Length; i++)
                //    pixels[i] = lightmapColors[i].GetColor();

                texture = new UnityEngine.Texture2D(width, height, UnityEngine.TextureFormat.RGBA32, false);

                texture.SetPixels(lightmapColors);
                texture.Apply();
            }
            return texture;
        }
    }
}