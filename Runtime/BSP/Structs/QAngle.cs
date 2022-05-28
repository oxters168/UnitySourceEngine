using UnityEngine;

namespace UnitySourceEngine
{
    public struct QAngle
    {
        public float pitch;
        public float roll;
        public float yaw;

        public QAngle(float _pitch, float _roll, float _yaw)
        {
            pitch = _pitch;
            roll = _roll;
            yaw = _yaw;
        }

        public Quaternion ToQuaternion()
        {
            Quaternion outQuat;
            float sr, sp, sy, cr, cp, cy;

            float yRad = roll * Mathf.Deg2Rad * 0.5f;
            float xRad = pitch * Mathf.Deg2Rad * 0.5f;
            float zRad = yaw * Mathf.Deg2Rad * 0.5f;
            sy = Mathf.Sin(yRad);
            cy = Mathf.Cos(yRad);
            sp = Mathf.Sin(xRad);
            cp = Mathf.Cos(xRad);
            sr = Mathf.Sin(zRad);
            cr = Mathf.Cos(zRad);

            float srXcp = sr * cp, crXsp = cr * sp;
            outQuat.x = srXcp * cy - crXsp * sy;
            outQuat.y = crXsp * cy + srXcp * sy;

            float crXcp = cr * cp, srXsp = sr * sp;
            outQuat.z = crXcp * sy - srXsp * cy;
            outQuat.w = crXcp * cy + srXsp * sy;

            return outQuat;
        }
    }
}