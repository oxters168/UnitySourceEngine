namespace UnitySourceEngine
{
    public class mstudioautolayer_t
    {
        public short sequenceIndex;
        public short poseIndex;

        public int flags;
        public double influenceStart;
        public double influencePeak;
        public double influenceTail;
        public double influenceEnd;
    }

    public enum autolayer_flags
    {
        STUDIO_AL_POST = 0x0010, //
        STUDIO_AL_SPLINE = 0x0040, //convert layer ramp in/out curve is a spline instead of linear
        STUDIO_AL_XFADE = 0x0080, //pre-bias the ramp curve to compense for a non-1 weight, assuming a second layer is also going to accumulate
        STUDIO_AL_NOBLEND = 0x0200, //animation always blends at 1.0 (ignores weight)
        STUDIO_AL_LOCAL = 0x1000, //layer is a local context sequence
        STUDIO_AL_POSE = 0x4000, //layer blends using a pose parameter instead of parent cycle
    }
}