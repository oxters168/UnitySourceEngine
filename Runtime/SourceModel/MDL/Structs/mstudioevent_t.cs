namespace UnitySourceEngine
{
    public struct mstudioevent_t
    {
        public const int NEW_EVENT_STYLE = 1 << 10;

        public string name;

        public double cycle;
        public int eventIndex;
        public int eventType;
        public string options; //SizeOf 64
        public int nameOffset;

        public ulong CountBytes()
        {
            return (ulong)((!string.IsNullOrEmpty(name) ? 2*name.Length : 0) + (!string.IsNullOrEmpty(options) ? 2*options.Length : 0) + 24);
        }

        public void Dispose()
        {
            options = null;
        }
    }
}