using UnityEngine;

namespace UnitySourceEngine
{
    public struct dDispVert //lump 33 : 20 bytes
    {
        public Vector3 vec; //Vector field defining displacement volume.
        public float dist; //Displacement distances.
        public float alpha; //"per vertex" alpha values.
    }
}