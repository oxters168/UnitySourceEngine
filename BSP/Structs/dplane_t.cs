using UnityEngine;

namespace UnitySourceEngine
{
    public struct dplane_t
    {
        public Vector3 normal;  // normal vector
        public float dist;  // distance from origin
        public int type;    // plane axis identifier
    }
}