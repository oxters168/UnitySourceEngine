namespace UnitySourceEngine
{
    public struct dgamelumpheader_t
    {
        public int lumpCount;  // number of game lumps
        public dgamelump_t[] gamelump; //(size of [lumpCount])

        public void Dispose()
        {
            gamelump = null;
        }
    }
}