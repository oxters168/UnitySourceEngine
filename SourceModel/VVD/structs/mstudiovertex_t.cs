using UnityEngine;

namespace UnitySourceEngine
{
    // NOTE: This is exactly 48 bytes
    public struct mstudiovertex_t
    {
        public mstudioboneweight_t m_BoneWeights;
        public Vector3 m_vecPosition;
        public Vector3 m_vecNormal;
        public Vector2 m_vecTexCoord;

        public void Dispose()
        {
            m_BoneWeights.Dispose();
        }
    }
}