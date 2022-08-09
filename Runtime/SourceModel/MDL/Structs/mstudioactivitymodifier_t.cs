namespace UnitySourceEngine
{
    public struct mstudioactivitymodifier_t
    {
        public string name;
        public int nameOffset;

        public ulong CountBytes()
        {
            return (ulong)((!string.IsNullOrEmpty(name) ? 2*name.Length : 0) + 4);
        }
    }
}