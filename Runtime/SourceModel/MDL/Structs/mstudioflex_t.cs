namespace UnitySourceEngine
{
    public struct mstudioflex_t //49+(2*uc.length)+(4*u.length)+(4*tva.length) bytes
    {
        public int flexDescIndex; //4 bytes
        public double target0; //8 bytes
        public double target1; //8 bytes
        public double target2; //8 bytes
        public double target3; //8 bytes

        public int vertCount; //4 bytes
        public int vertOffset; //4 bytes

        public int flexDescPartnerIndex; //4 bytes
        public byte vertAnimType; //1 byte
        public char[] unusedChar; //SizeOf 3 //2*unusedChar.length bytes
        public int[] unused; //SizeOf 6 //4*unused.length bytes

        public mstudiovertanim_t[] theVertAnims; //4*theVertAnims.length bytes

        public const byte STUDIO_VERT_ANIM_NORMAL = 0;
        public const byte STUDIO_VERT_ANIM_WRINKLE = 1;

        public ulong CountBytes()
        {
            ulong totalBytes = (ulong)((unusedChar != null ? 2*unusedChar.Length : 0) + (unused != null ? 4*unused.Length : 0) + (theVertAnims != null ? 4*theVertAnims.Length : 0) + 49);
            // if (theVertAnims != null)
            //     foreach(var vertAnim in theVertAnims)
            //         totalBytes += vertAnim.CountBytes();
            return totalBytes;
        }

        public void Dispose()
        {
            unusedChar = null;
            unused = null;
            theVertAnims = null;
        }
    }
}