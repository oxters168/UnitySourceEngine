namespace UnitySourceEngine
{
    public class mstudioflex_t
    {
        public int flexDescIndex;
        public double target0;
        public double target1;
        public double target2;
        public double target3;

        public int vertCount;
        public int vertOffset;

        public int flexDescPartnerIndex;
        public byte vertAnimType;
        public char[] unusedChar; //SizeOf 3
        public int[] unused; //SizeOf 6

        public mstudiovertanim_t[] theVertAnims;

        public const byte STUDIO_VERT_ANIM_NORMAL = 0;
        public const byte STUDIO_VERT_ANIM_WRINKLE = 1;

        public void Dispose()
        {
            unusedChar = null;
            unused = null;
            theVertAnims = null;
        }
    }
}