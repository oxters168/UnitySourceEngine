namespace UnitySourceEngine
{
    public static class ColorRGBExp32
    {
        //public byte r, g, b;
        //public sbyte exponent;

        public static UnityEngine.Color GetColor(byte r, byte g, byte b, sbyte exponent)
        {
            float powerResult = UnityEngine.Mathf.Pow(2, exponent);
            return new UnityEngine.Color(r / 255f * powerResult, g / 255f * powerResult, b / 255f * powerResult);
        }
    }
}