namespace UnitySourceEngine
{
    public struct mstudiomodel_t
    {
        // public char[] name;
        public string name; //2*name.length bytes
        public int type; //4 bytes
        public float boundingRadius; //4 bytes
        public int meshCount; //4 bytes
        public int meshOffset; //4 bytes
        public int vertexCount; //4 bytes
        public int vertexOffset; //4 bytes
        public int tangentOffset; //4 bytes
        public int attachmentCount; //4 bytes
        public int attachmentOffset; //4 bytes
        public int eyeballCount; //4 bytes
        public int eyeballOffset; //4 bytes
        public mstudio_modelvertexdata_t vertexData; //8 bytes
        public int[] unused; //4*unused.length bytes
        public mstudiomesh_t[] meshes;
        public mstudioeyeball_t[] eyeballs;

        public ulong CountBytes()
        {
            ulong totalBytes = (ulong)((!string.IsNullOrEmpty(name) ? 2*name.Length : 0) + (unused != null ? 4*unused.Length : 0) + 52);
            if (meshes != null)
                foreach(var mesh in meshes)
                    totalBytes += mesh.CountBytes();
            if (eyeballs != null)
                foreach(var eyeball in eyeballs)
                    totalBytes += eyeball.CountBytes();
            return totalBytes;
        }

        public void Dispose()
        {
            name = null;
            unused = null;
            if (meshes != null)
                foreach (var mesh in meshes)
                    mesh.Dispose();
            meshes = null;
            if (eyeballs != null)
                foreach (var eyeball in eyeballs)
                    eyeball.Dispose();
            eyeballs = null;
        }
    }
}