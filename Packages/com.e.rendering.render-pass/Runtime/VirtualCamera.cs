using System;
using UnityEngine;

namespace E.Rendering
{
    public struct CameraPlane
    {
        public CameraPlane(in VirtualCamera camera, in float distance)
        {
            this.distance = distance;
            float yn, yp, xn, xp;
            yn = -camera.bottom;
            yp = camera.top;
            xn = -camera.left;
            xp = camera.right;
            if (!camera.isOrthographic)
            {
                float dn = distance / camera.near;
                yn *= dn;
                yp *= dn;
                xn *= dn;
                xp *= dn;
            }
            Matrix4x4 mat = camera.worldToViewMatrix.inverse;
            bottomLeft = mat * new Vector3(xn, yn, -distance);
            topLeft = mat * new Vector3(xn, yp, -distance);
            topRight = mat * new Vector3(xp, yp, -distance);
            bottomRight = mat * new Vector3(xp, yn, -distance);
            plane = new Plane(bottomLeft, topLeft, topRight);
        }

        public static CameraPlane GetPlane(in VirtualCamera camera, in float distance)
        {
            return new CameraPlane(camera, distance);
        }

        public float distance;

        public Vector3 bottomLeft;

        public Vector3 topLeft;

        public Vector3 topRight;

        public Vector3 bottomRight;

        public Plane plane;
    }

    internal struct VirtualCameraData
    {
        public bool isOffCenter;
        public bool isOrthographic;
        public float size;
        public float fov;
        public float aspect;
        public float left;
        public float right;
        public float bottom;
        public float top;
        public float near;
        public float far;
    }

    [Serializable]
    public unsafe struct VirtualCamera
    {
        private const float MIN_VALUE = 0.01f;

        private const float MIN_FOV = 0.01f;

        private const float MAX_FOV = 179.99f;

        private VirtualCameraData* data;

        [SerializeField]
        public bool IsCreated { get; private set; }

        private bool m_isDirty;

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

        //public VirtualCamera(in Camera camera)
        //{
        //    data = null;

        //    SetProperties(camera);
        //}

        //public VirtualCamera(in bool isOrthographic,
        //    in float size, in float fov, in float aspect,
        //    in float near, in float far)
        //{
        //    SetProperties(isOrthographic, size, fov, aspect, near, far);
        //}

        //public VirtualCamera(in bool isOrthographic,
        //    in float left, in float right, in float bottom, in float top,
        //    in float near, in float far)
        //{
        //    SetProperties(isOrthographic, left, right, bottom, top, near, far);
        //}

        public void SetTransform(in Vector3 position, in Quaternion rotation)
        {
            m_isDirty = true;
            m_Position = position;
            m_Rotation = rotation;
        }

        public void SetProperties(in Camera camera)
        {
            SetProperties(camera.orthographic,
                camera.orthographicSize, camera.fieldOfView, camera.aspect, camera.nearClipPlane, camera.farClipPlane);
        }

        public void SetProperties(in bool isOrthographic,
            in float size, in float fov, in float aspect,
            in float near, in float far)
        {
            m_isDirty = true;
            m_IsOffCenter = false;
            m_IsOrthographic = isOrthographic;
            m_Size = size;
            m_Fov = fov;
            m_Aspect = aspect;
            m_Near = near;
            m_Far = far;
            IsCreated = true;
            MinValue(ref m_Size);
            MinMaxFOV();
            MinValue(ref m_Aspect);
            MinMaxNearAndFar();
            CommitLRBT();
        }

        public void SetProperties(in bool isOrthographic,
            in float left, in float right, in float bottom, in float top,
            in float near, in float far)
        {
            m_isDirty = true;
            m_IsOffCenter = true;
            m_IsOrthographic = isOrthographic;
            m_Left = left;
            m_Right = right;
            m_Bottom = bottom;
            m_Top = top;
            m_Near = near;
            m_Far = far;
            IsCreated = true;
            MinValue(ref m_Left);
            MinValue(ref m_Right);
            MinValue(ref m_Bottom);
            MinValue(ref m_Top);
            MinMaxNearAndFar();
            CommitLRBT();
        }

        public bool isDirty { get { return m_isDirty; } set { m_isDirty = value; } }

        public Vector3 position
        {
            get { return m_Position; }
            set
            {
                SetDirtyIfChanged(ref m_Position, value);
            }
        }

        public Quaternion rotation
        {
            get { return m_Rotation; }
            set
            {
                SetDirtyIfChanged(ref m_Rotation, value);
            }
        }

        public bool isOffCenter
        {
            get { return m_IsOffCenter; }
            set
            {
                SetDirtyIfChanged(ref m_IsOffCenter, value);
                CommitLRBT();
            }
        }

        public bool isOrthographic
        {
            get { return m_IsOrthographic; }
            set
            {
                SetDirtyIfChanged(ref m_IsOrthographic, value);
                MinMaxNearAndFar();
                CommitLRBT();
            }
        }

        public float size
        {
            get { return m_Size; }
            set
            {
                m_IsOffCenter = false;
                SetDirtyIfChanged(ref m_Size, value);
                MinValue(ref m_Size);
                CommitLRBT();
            }
        }

        public float fov
        {
            get { return m_Fov; }
            set
            {
                m_IsOffCenter = false;
                SetDirtyIfChanged(ref m_Fov, value);
                MinMaxFOV();
                CommitLRBT();
            }
        }

        public float aspect
        {
            get { return m_Aspect; }
            set
            {
                m_IsOffCenter = false;
                SetDirtyIfChanged(ref m_Aspect, value);
                MinValue(ref m_Aspect);
                CommitLRBT();
            }
        }

        public float left
        {
            get { return m_Left; }
            set { SetNearSideDistance(ref m_Left, value); }
        }

        public float right
        {
            get { return m_Right; }
            set { SetNearSideDistance(ref m_Right, value); }
        }

        public float bottom
        {
            get { return m_Bottom; }
            set { SetNearSideDistance(ref m_Bottom, value); }
        }

        public float top
        {
            get { return m_Top; }
            set { SetNearSideDistance(ref m_Top, value); }
        }

        public float near
        {
            get { return m_Near; }
            set { SetNearFar(ref m_Near, value); }
        }

        public float far
        {
            get { return m_Far; }
            set { SetNearFar(ref m_Far, value); }
        }

        private void SetNearSideDistance(ref float d, in float value)
        {
            m_IsOffCenter = true;
            SetDirtyIfChanged(ref d, value);
            MinValue(ref d);
        }

        private void SetNearFar(ref float d, in float value)
        {
            SetDirtyIfChanged(ref d, value);
            MinMaxNearAndFar();
            CommitLRBT();
        }

        private void MinMaxNearAndFar()
        {
            if (!m_IsOrthographic)
            {
                MinValue(ref m_Near);
            }
            if (m_Far <= m_Near) m_Far = m_Near + MIN_VALUE;
        }

        private void CommitLRBT()
        {
            if (m_IsOffCenter) return;
            float x, y;
            if (m_IsOrthographic)
            {
                y = m_Size;
                x = y * m_Aspect;
            }
            else
            {
                //degree to radians: r = d * 0.01745329251994329576923690768489
                //half radians: r *= 0.5
                y = (float)(m_Near * Math.Tan(0.00872664625997164788461845384244 * fov));
                x = y * aspect;
            }
            m_Left = -x;
            m_Right = x;
            m_Bottom = -y;
            m_Top = y;
        }

        private void SetDirtyIfChanged<T>(ref T a, in T b) where T : System.IEquatable<T>
        {
            if (!a.Equals(b)) m_isDirty = true;
            a = b;
        }

        private void MinMaxFOV()
        {
            if (m_Fov < MIN_FOV) m_Fov = MIN_FOV;
            if (m_Fov > MAX_FOV) m_Fov = MAX_FOV;
        }

        private void MinValue(ref float val)
        {
            if (val < MIN_VALUE) val = MIN_VALUE;
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
                if (isOrthographic)
                {
                    return Matrix4x4.Ortho(-left, right, -bottom, top, near, far);
                }
                else
                {
                    return Matrix4x4.Frustum(-left, right, -bottom, top, near, far);
                }
            }
        }

        public Matrix4x4 cullingMatrix
        { get { return projectionMatrix * worldToViewMatrix; } }

        public CameraPlane GetPlane(in float distance)
        {
            return new CameraPlane(this, distance);
        }
    }
}