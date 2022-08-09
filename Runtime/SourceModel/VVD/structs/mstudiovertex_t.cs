using UnityEngine;

namespace UnitySourceEngine
{
    // NOTE: This is exactly 48 bytes
    public struct mstudiovertex_t
    {
        public mstudioboneweight_t m_BoneWeights;
        public Vector3 m_vecPosition; //12
        public Vector3 m_vecNormal; //12
        public Vector2 m_vecTexCoord; //8

        public ulong CountBytes()
        {
            return (ulong)(32) + m_BoneWeights.CountBytes();
        }

        public void Dispose()
        {
            m_BoneWeights.Dispose();
        }
    }
}