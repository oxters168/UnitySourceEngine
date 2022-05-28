namespace UnitySourceEngine
{
    public class mstudioevent_t
    {
        public string name;

        public double cycle;
        public int eventIndex;
        public int eventType;
        public char[] options; //SizeOf 64
        public int nameOffset;

        public const int NEW_EVENT_STYLE = 1 << 10;

        public void Dispose()
        {
            options = null;
        }
    }
}