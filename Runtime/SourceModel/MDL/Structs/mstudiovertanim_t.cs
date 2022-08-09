namespace UnitySourceEngine
{
    public struct mstudiovertanim_t //4 bytes
    {
        public ushort index; //2 bytes
        public byte speed; //1 byte
        public byte side; //1 byte

        public ulong CountBytes()
        {
            return (ulong)(4);
        }
    }
}