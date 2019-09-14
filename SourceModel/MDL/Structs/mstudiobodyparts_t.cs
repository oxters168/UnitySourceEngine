namespace UnitySourceEngine
{
    public class mstudiobodyparts_t
    {
        public string name;
        public int nameOffset;
        public int modelCount;
        public int theBase;
        public int modelOffset;
        public mstudiomodel_t[] models;

        public void Dispose()
        {
            if (models != null)
                foreach (var model in models)
                    model?.Dispose();
            models = null;
        }
    }
}