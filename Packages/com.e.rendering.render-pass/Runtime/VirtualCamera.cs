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

    public struct VirtualCameraData
    {
        public Vector3 position;
        public Quaternion rotation;
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

    public unsafe struct VirtualCamera : IDisposable
    {
        private const float MIN_VALUE = 0.01f;

        private const float MIN_FOV = 0.01f;

        private const float MAX_FOV = 179.99f;

        private VirtualCameraData* m_Data;

        private bool* m_IsDirty;

        public bool IsCreated { get => m_Data != null; }

        public void Dispose()
        {
            m_Data = null;
            m_IsDirty = null;
        }

        public VirtualCamera(in Camera camera)
        {
            VirtualCameraData value = new VirtualCameraData();
            m_Data = &value;
            bool isDirty = false;
            m_IsDirty = &isDirty;
            SetProperties(camera);
        }

        public VirtualCamera(in bool isOrthographic,
            in float size, in float fov, in float aspect,
            in float near, in float far)
        {
            VirtualCameraData value = new VirtualCameraData();
            m_Data = &value;
            bool isDirty = false;
            m_IsDirty = &isDirty;
            SetProperties(isOrthographic, size, fov, aspect, near, far);
        }

        public VirtualCamera(in bool isOrthographic,
            in float left, in float right, in float bottom, in float top,
            in float near, in float far)
        {
            VirtualCameraData value = new VirtualCameraData();
            m_Data = &value;
            bool isDirty = false;
            m_IsDirty = &isDirty;
            SetProperties(isOrthographic, left, right, bottom, top, near, far);
        }

        public ref VirtualCameraData GetData()
        {
            return ref *m_Data;
        }

        public void SetTransform(in Vector3 position, in Quaternion rotation)
        {
            *m_IsDirty = true;
            m_Data->position = position;
            m_Data->rotation = rotation;
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
            *m_IsDirty = true;
            m_Data->isOffCenter = false;
            m_Data->isOrthographic = isOrthographic;
            m_Data->size = size;
            m_Data->fov = fov;
            m_Data->aspect = aspect;
            m_Data->near = near;
            m_Data->far = far;
            MinValue(ref m_Data->size);
            MinMaxFOV();
            MinValue(ref m_Data->aspect);
            MinMaxNearAndFar();
            CommitLRBT();
        }

        public void SetProperties(in bool isOrthographic,
            in float left, in float right, in float bottom, in float top,
            in float near, in float far)
        {
            *m_IsDirty = true;
            m_Data->isOffCenter = true;
            m_Data->isOrthographic = isOrthographic;
            m_Data->left = left;
            m_Data->right = right;
            m_Data->bottom = bottom;
            m_Data->top = top;
            m_Data->near = near;
            m_Data->far = far;
            MinValue(ref m_Data->left);
            MinValue(ref m_Data->right);
            MinValue(ref m_Data->bottom);
            MinValue(ref m_Data->top);
            MinMaxNearAndFar();
            CommitLRBT();
        }

        public bool isDirty { get { return *m_IsDirty; } set { *m_IsDirty = value; } }

        public Vector3 position
        {
            get { return m_Data->position; }
            set
            {
                SetDirtyIfChanged(ref m_Data->position, value);
            }
        }

        public Quaternion rotation
        {
            get { return m_Data->rotation; }
            set
            {
                SetDirtyIfChanged(ref m_Data->rotation, value);
            }
        }

        public bool isOffCenter
        {
            get { return m_Data->isOffCenter; }
            set
            {
                SetDirtyIfChanged(ref m_Data->isOffCenter, value);
                CommitLRBT();
            }
        }

        public bool isOrthographic
        {
            get { return m_Data->isOrthographic; }
            set
            {
                SetDirtyIfChanged(ref m_Data->isOrthographic, value);
                MinMaxNearAndFar();
                CommitLRBT();
            }
        }

        public float size
        {
            get { return m_Data->size; }
            set
            {
                m_Data->isOffCenter = false;
                SetDirtyIfChanged(ref m_Data->size, value);
                MinValue(ref m_Data->size);
                CommitLRBT();
            }
        }

        public float fov
        {
            get { return m_Data->fov; }
            set
            {
                m_Data->isOffCenter = false;
                SetDirtyIfChanged(ref m_Data->fov, value);
                MinMaxFOV();
                CommitLRBT();
            }
        }

        public float aspect
        {
            get { return m_Data->aspect; }
            set
            {
                m_Data->isOffCenter = false;
                SetDirtyIfChanged(ref m_Data->aspect, value);
                MinValue(ref m_Data->aspect);
                CommitLRBT();
            }
        }

        public float left
        {
            get { return m_Data->left; }
            set { SetNearSideDistance(ref m_Data->left, value); }
        }

        public float right
        {
            get { return m_Data->right; }
            set { SetNearSideDistance(ref m_Data->right, value); }
        }

        public float bottom
        {
            get { return m_Data->bottom; }
            set { SetNearSideDistance(ref m_Data->bottom, value); }
        }

        public float top
        {
            get { return m_Data->top; }
            set { SetNearSideDistance(ref m_Data->top, value); }
        }

        public float near
        {
            get { return m_Data->near; }
            set { SetNearFar(ref m_Data->near, value); }
        }

        public float far
        {
            get { return m_Data->far; }
            set { SetNearFar(ref m_Data->far, value); }
        }

        private void SetNearSideDistance(ref float d, in float value)
        {
            m_Data->isOffCenter = true;
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
            if (!m_Data->isOrthographic)
            {
                MinValue(ref m_Data->near);
            }
            if (m_Data->far <= m_Data->near) m_Data->far = m_Data->near + MIN_VALUE;
        }

        private void CommitLRBT()
        {
            if (m_Data->isOffCenter) return;
            float x, y;
            if (m_Data->isOrthographic)
            {
                y = m_Data->size;
                x = y * m_Data->aspect;
            }
            else
            {
                //degree to radians: r = d * 0.01745329251994329576923690768489
                //half radians: r *= 0.5
                y = (float)(m_Data->near * Math.Tan(0.00872664625997164788461845384244 * fov));
                x = y * aspect;
            }
            m_Data->left = -x;
            m_Data->right = x;
            m_Data->bottom = -y;
            m_Data->top = y;
        }

        private void SetDirtyIfChanged<T>(ref T a, in T b) where T : System.IEquatable<T>
        {
            if (!a.Equals(b)) *m_IsDirty = true;
            a = b;
        }

        private void MinMaxFOV()
        {
            if (m_Data->fov < MIN_FOV) m_Data->fov = MIN_FOV;
            if (m_Data->fov > MAX_FOV) m_Data->fov = MAX_FOV;
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