using UnityEngine;

namespace UnitySourceEngine
{
    public struct dmodel_t
    {
        public Vector3 mins, maxs; // bounding box
        public Vector3 origin; // for sounds or lights
        public int headnode; // index into node array
        public int firstface, numfaces; // index into face array
    }
}