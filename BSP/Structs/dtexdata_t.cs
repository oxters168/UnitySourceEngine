using UnityEngine;

namespace UnitySourceEngine
{
    public struct dtexdata_t
    {
        public Vector3 reflectivity; // RGB reflectivity
        public int nameStringTableID; // index into TexdataStringTable
        public int width, height; // source image
        public int view_width, view_height;
    }
}