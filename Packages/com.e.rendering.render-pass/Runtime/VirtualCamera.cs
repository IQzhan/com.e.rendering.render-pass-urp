using System;
using UnityEngine;

namespace E.Rendering
{
    public struct CameraPlane
    {
        public CameraPlane(VirtualCamera camera, float distance)
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

        public static CameraPlane GetPlane(VirtualCamera camera, float distance)
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

    public struct VirtualCameraData : IEquatable<VirtualCameraData>
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

        public override bool Equals(object obj)
        {
            return obj is VirtualCameraData data && Equals(data);
        }

        public bool Equals(VirtualCameraData other)
        {
            return position.Equals(other.position) &&
                   rotation.Equals(other.rotation) &&
                   isOffCenter == other.isOffCenter &&
                   isOrthographic == other.isOrthographic &&
                   size == other.size &&
                   fov == other.fov &&
                   aspect == other.aspect &&
                   left == other.left &&
                   right == other.right &&
                   bottom == other.bottom &&
                   top == other.top &&
                   near == other.near &&
                   far == other.far;
        }

        public override int GetHashCode()
        {
            int hashCode = 931829884;
            hashCode = hashCode * -1521134295 + position.GetHashCode();
            hashCode = hashCode * -1521134295 + rotation.GetHashCode();
            hashCode = hashCode * -1521134295 + isOffCenter.GetHashCode();
            hashCode = hashCode * -1521134295 + isOrthographic.GetHashCode();
            hashCode = hashCode * -1521134295 + size.GetHashCode();
            hashCode = hashCode * -1521134295 + fov.GetHashCode();
            hashCode = hashCode * -1521134295 + aspect.GetHashCode();
            hashCode = hashCode * -1521134295 + left.GetHashCode();
            hashCode = hashCode * -1521134295 + right.GetHashCode();
            hashCode = hashCode * -1521134295 + bottom.GetHashCode();
            hashCode = hashCode * -1521134295 + top.GetHashCode();
            hashCode = hashCode * -1521134295 + near.GetHashCode();
            hashCode = hashCode * -1521134295 + far.GetHashCode();
            return hashCode;
        }

        public static bool operator ==(VirtualCameraData left, VirtualCameraData right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(VirtualCameraData left, VirtualCameraData right)
        {
            return !(left == right);
        }
    }

    public unsafe struct VirtualCamera : IDisposable, IEquatable<VirtualCamera>
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

        public VirtualCamera(Camera camera)
        {
            VirtualCameraData value = new VirtualCameraData();
            m_Data = &value;
            bool isDirty = false;
            m_IsDirty = &isDirty;
            SetProperties(camera);
        }

        public VirtualCamera(bool isOrthographic,
            float size, float fov, float aspect,
            float near, float far)
        {
            VirtualCameraData value = new VirtualCameraData();
            m_Data = &value;
            bool isDirty = false;
            m_IsDirty = &isDirty;
            SetProperties(isOrthographic, size, fov, aspect, near, far);
        }

        public VirtualCamera(bool isOrthographic,
            float left, float right, float bottom, float top,
            float near, float far)
        {
            VirtualCameraData value = new VirtualCameraData();
            m_Data = &value;
            bool isDirty = false;
            m_IsDirty = &isDirty;
            SetProperties(isOrthographic, left, right, bottom, top, near, far);
        }

        public ref VirtualCameraData GetRefData()
        {
            return ref *m_Data;
        }

        public VirtualCameraData* GetData()
        {
            return m_Data;
        }

        public void SetTransform(Vector3 position, Quaternion rotation)
        {
            *m_IsDirty = true;
            m_Data->position = position;
            m_Data->rotation = rotation;
        }

        public void SetProperties(Camera camera)
        {
            SetProperties(camera.orthographic,
                camera.orthographicSize, camera.fieldOfView, camera.aspect, camera.nearClipPlane, camera.farClipPlane);
        }

        public void SetProperties(bool isOrthographic,
            float size, float fov, float aspect,
            float near, float far)
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

        public void SetProperties(bool isOrthographic,
            float left, float right, float bottom, float top,
            float near, float far)
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

        private void SetNearSideDistance(ref float d, float value)
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

        public CameraPlane GetPlane(float distance)
        {
            return new CameraPlane(this, distance);
        }

        public override bool Equals(object obj)
        {
            return obj is VirtualCamera camera && Equals(camera);
        }

        public bool Equals(VirtualCamera other)
        {
            return m_Data->Equals(*other.m_Data);
        }

        public override int GetHashCode()
        {
            return m_Data->GetHashCode();
        }

        public static bool operator ==(VirtualCamera left, VirtualCamera right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(VirtualCamera left, VirtualCamera right)
        {
            return !(left == right);
        }
    }
}