using UnityEngine;

namespace E.Rendering
{
    public struct VirtualCameraPlane
    {
        public VirtualCameraPlane(in VirtualCamera camera, in float distance)
        {
            this.distance = distance;
            p0 = p1 = p2 = p3 = Vector3.zero;
        }

        public float distance;

        public Vector3 p0;

        public Vector3 p1;

        public Vector3 p2;

        public Vector3 p3;
    }

    [System.Serializable]
    public struct VirtualCamera
    {
        public VirtualCamera(
            in Vector3 position,
            in Quaternion rotation,
            in bool isOrthographic,
            in float size, in float fov, in float aspect,
            in float near, in float far)
        {
            m_isDirt = true;
            m_Position = position;
            m_Rotation = rotation;
            m_IsOffCenter = false;
            m_IsOrthographic = isOrthographic;
            m_Size = size;
            m_Fov = fov;
            m_Aspect = aspect;
            m_Near = near;
            m_Far = far;
            m_Left = m_Right = m_Bottom = m_Top = MIN_VALUE;
        }

        public VirtualCamera(
            in Vector3 position,
            in Quaternion rotation,
            in bool isOrthographic,
            in float left, in float right, in float bottom, in float top,
            in float near, in float far)
        {
            m_isDirt = true;
            m_Position = position;
            m_Rotation = rotation;
            m_IsOffCenter = true;
            m_IsOrthographic = isOrthographic;
            m_Near = near;
            m_Far = far;
            m_Left = left;
            m_Right = right;
            m_Bottom = bottom;
            m_Top = top;
            m_Size = m_Fov = m_Aspect = MIN_VALUE;
        }

        private const float MIN_VALUE = 0.01f;

        private const float MIN_FOV = 0.01f;

        private const float MAX_FOV = 179.99f;

        internal bool m_isDirt;

        [SerializeField]
        private Vector3 m_Position;

        [SerializeField]
        private Quaternion m_Rotation;

        [SerializeField]
        private bool m_IsOffCenter;

        [SerializeField]
        private bool m_IsOrthographic;

        [SerializeField]
        private float m_Size;

        [SerializeField]
        private float m_Fov;

        [SerializeField]
        private float m_Aspect;

        [SerializeField]
        private float m_Left;

        [SerializeField]
        private float m_Right;

        [SerializeField]
        private float m_Bottom;

        [SerializeField]
        private float m_Top;

        [SerializeField]
        private float m_Near;

        [SerializeField]
        private float m_Far;

        public Vector3 position
        {
            get { return m_Position; }
            set
            {
                SetDirtIfChanged(m_Position, value);
                m_Position = value;
            }
        }

        public Quaternion rotation
        {
            get { return m_Rotation; }
            set
            {
                SetDirtIfChanged(m_Rotation, value);
                m_Rotation = value;
            }
        }

        public bool isOffCenter
        {
            get { return m_IsOffCenter; }
            set
            {
                SetDirtIfChanged(m_IsOffCenter, value);
                m_IsOffCenter = value;
            }
        }

        public bool isOrthographic
        {
            get { return m_IsOrthographic; }
            set
            {
                SetDirtIfChanged(m_IsOrthographic, value);
                m_IsOrthographic = value;
                ResetNearAndFar();
            }
        }

        public float size
        {
            get { return m_Size; }
            set
            {
                SetDirtIfChanged(m_Size, value);
                m_Size = value;
                if (m_Size < MIN_VALUE) m_Size = MIN_VALUE;
            }
        }

        public float fov
        {
            get { return m_Fov; }
            set
            {
                SetDirtIfChanged(m_Fov, value);
                m_Fov = value;
                if (m_Fov < MIN_FOV) m_Fov = MIN_FOV;
                if (m_Fov > MAX_FOV) m_Fov = MAX_FOV;
            }
        }

        public float aspect
        {
            get { return m_Aspect; }
            set
            {
                SetDirtIfChanged(m_Aspect, value);
                m_Aspect = value;
                if (m_Aspect < MIN_VALUE) m_Aspect = MIN_VALUE;
            }
        }

        public float left
        {
            get { return m_Left; }
            set
            {
                SetDirtIfChanged(m_Left, value);
                m_Left = value;
                if (m_Left < MIN_VALUE) m_Left = MIN_VALUE;
            }
        }

        public float right
        {
            get { return m_Right; }
            set
            {
                SetDirtIfChanged(m_Right, value);
                m_Right = value;
                if (m_Right < MIN_VALUE) m_Right = MIN_VALUE;
            }
        }

        public float bottom
        {
            get { return m_Bottom; }
            set
            {
                SetDirtIfChanged(m_Bottom, value);
                m_Bottom = value;
                if (m_Bottom < MIN_VALUE) m_Bottom = MIN_VALUE;
            }
        }

        public float top
        {
            get { return m_Top; }
            set
            {
                SetDirtIfChanged(m_Top, value);
                m_Top = value;
                if (m_Top < MIN_VALUE) m_Top = MIN_VALUE;
            }
        }

        public float near
        {
            get { return m_Near; }
            set
            {
                SetDirtIfChanged(m_Near, value);
                m_Near = value;
                ResetNearAndFar();
            }
        }

        public float far
        {
            get { return m_Far; }
            set
            {
                SetDirtIfChanged(m_Far, value);
                m_Far = value;
                ResetNearAndFar();
            }
        }

        private void ResetNearAndFar()
        {
            if (!m_IsOrthographic)
            {
                if (m_Near < MIN_VALUE) m_Near = MIN_VALUE;
            }
            if (m_Far <= m_Near) m_Far = m_Near + MIN_VALUE;
        }

        private void SetDirtIfChanged<T>(in T a, in T b) where T : System.IEquatable<T>
        {
            if (!a.Equals(b)) m_isDirt = true;
        }

        public Matrix4x4 worldToViewMatrix
        {
            get
            {
                Matrix4x4 m = Matrix4x4.TRS(position, rotation, Vector3.one).inverse;
                m.m20 = -m.m20;
                m.m21 = -m.m21;
                m.m22 = -m.m22;
                m.m23 = -m.m23;
                return m;
            }
        }

        public Matrix4x4 projectionMatrix
        {
            get
            {
                if (m_IsOffCenter)
                {
                    if (isOrthographic)
                    {
                        return Matrix4x4.Ortho(-left, right, -bottom, top, near, far);
                    }
                    else
                    {
                        return Matrix4x4.Frustum(-left, right, -bottom, top, near, far);
                    }
                }
                else
                {
                    if (isOrthographic)
                    {
                        float y = size;
                        float x = y * aspect;
                        return Matrix4x4.Ortho(-x, x, -y, y, near, far);
                    }
                    else
                    {
                        return Matrix4x4.Perspective(fov, aspect, near, far);
                    }
                }
            }
        }

        public Matrix4x4 cullingMatrix
        {
            get
            {
                return projectionMatrix * worldToViewMatrix;
            }
        }

        public bool PropertiesChanged()
        {
            return m_isDirt;
        }

        public void EnqueueUpdate()
        {

            m_isDirt = true;
        }

        public VirtualCameraPlane GetPlane(in VirtualCamera camera, in float distance)
        {
            return new VirtualCameraPlane(camera, distance);
        }
    }
}