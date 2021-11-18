using System;
using System.Runtime.InteropServices;
using UnityEngine;

namespace E.Rendering
{
    public struct CameraPlane
    {
        public CameraPlane(VirtualCamera camera, float distance)
        {
            this.distance = distance;
            float yn, yp, xn, xp;
            if (camera.isOffCenter)
            {
                yn = -camera.bottom;
                yp = camera.top;
                xn = -camera.left;
                xp = camera.right;
                
            }
            else
            {
                float y, x;
                if (camera.isOrthographic)
                {
                    y = camera.size * 0.5f;
                    x = y * camera.aspect;
                }
                else
                {
                    y = (float)(camera.near * Math.Tan(camera.fov * 0.00872664625997164788461845384244));
                    x = y * camera.aspect;
                }
                yn = -y;
                yp = y;
                xn = -x;
                xp = x;
            }
            if (!camera.isOrthographic)
            {
                float dn = distance / camera.near;
                yn *= dn;
                yp *= dn;
                xn *= dn;
                xp *= dn;
            }
            Matrix4x4 mat = camera.worldToViewMatrix.inverse;
            bottomLeft = mat * new Vector4(xn, yn, -distance, 1);
            topLeft = mat * new Vector4(xn, yp, -distance, 1);
            topRight = mat * new Vector4(xp, yp, -distance, 1);
            bottomRight = mat * new Vector4(xp, yn, -distance, 1);
            plane = new Plane(bottomLeft, topLeft, topRight);
        }

        public static CameraPlane GetPlane(VirtualCamera camera, float distance)
        {
            return new CameraPlane(camera, distance);
        }

        public float distance { get; private set; }

        public Vector3 bottomLeft { get; private set; }

        public Vector3 topLeft { get; private set; }

        public Vector3 topRight { get; private set; }

        public Vector3 bottomRight { get; private set; }

        public Plane plane { get; private set; }
    }

    public struct VirtualCameraData : IEquatable<VirtualCameraData>
    {
        public float posX, posY, posZ;
        public float quatX, quatY, quatZ, quatW;
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

        public override bool Equals(object obj) => obj is VirtualCameraData data && Equals(data);

        public bool Equals(VirtualCameraData other)
        {
            return posX == other.posX &&
                   posY == other.posY &&
                   posZ == other.posZ &&
                   quatX == other.quatX &&
                   quatY == other.quatY &&
                   quatZ == other.quatZ &&
                   quatW == other.quatW &&
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
            int hashCode = -169264683;
            hashCode = hashCode * -1521134295 + posX.GetHashCode();
            hashCode = hashCode * -1521134295 + posY.GetHashCode();
            hashCode = hashCode * -1521134295 + posZ.GetHashCode();
            hashCode = hashCode * -1521134295 + quatX.GetHashCode();
            hashCode = hashCode * -1521134295 + quatY.GetHashCode();
            hashCode = hashCode * -1521134295 + quatZ.GetHashCode();
            hashCode = hashCode * -1521134295 + quatW.GetHashCode();
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

        public static bool operator ==(VirtualCameraData left, VirtualCameraData right) => left.Equals(right);

        public static bool operator !=(VirtualCameraData left, VirtualCameraData right) => !(left == right);
    }

    public unsafe struct VirtualCamera : IDisposable, IEquatable<VirtualCamera>
    {
        private const float MIN_VALUE = 0.01f;

        private const float MIN_FOV = 0.01f;

        private const float MAX_FOV = 179.99f;

        private IntPtr m_DataAddress;

        private IntPtr m_StateAddress;

        private VirtualCameraData* m_Data;

        private bool* m_IsDirty;

        public bool IsCreated { get => m_DataAddress != IntPtr.Zero; }

        public VirtualCamera(bool isOrthographic, float size, float fov, float aspect, float near, float far)
        {
            m_DataAddress = Marshal.AllocHGlobal(Marshal.SizeOf<VirtualCameraData>());
            m_Data = (VirtualCameraData*)m_DataAddress.ToPointer();
            *m_Data = new VirtualCameraData();
            m_StateAddress = Marshal.AllocHGlobal(Marshal.SizeOf<bool>());
            m_IsDirty = (bool*)m_StateAddress.ToPointer();
            *m_IsDirty = true;
            rotation = Quaternion.identity;
            position = Vector3.zero;
            isOffCenter = false;
            this.isOrthographic = isOrthographic;
            this.near = near;
            this.far = far;
            this.size = size;
            this.fov = fov;
            this.aspect = aspect;
            left = 1;
            right = 1;
            bottom = 1;
            top = 1;
        }

        public VirtualCamera(bool isOrthographic, float left, float right, float bottom, float top, float near, float far)
        {
            m_DataAddress = Marshal.AllocHGlobal(Marshal.SizeOf<VirtualCameraData>());
            m_Data = (VirtualCameraData*)m_DataAddress.ToPointer();
            *m_Data = new VirtualCameraData();
            m_StateAddress = Marshal.AllocHGlobal(Marshal.SizeOf<bool>());
            m_IsDirty = (bool*)m_StateAddress.ToPointer();
            *m_IsDirty = true;
            rotation = Quaternion.identity;
            position = Vector3.zero;
            isOffCenter = true;
            this.isOrthographic = isOrthographic;
            this.near = near;
            this.far = far;
            this.left = left;
            this.right = right;
            this.bottom = bottom;
            this.top = top;
            size = 5;
            fov = 45;
            aspect = 1;
        }

        public VirtualCamera(Camera camera)
        {
            m_DataAddress = Marshal.AllocHGlobal(Marshal.SizeOf<VirtualCameraData>());
            m_Data = (VirtualCameraData*)m_DataAddress.ToPointer();
            *m_Data = new VirtualCameraData();
            m_StateAddress = Marshal.AllocHGlobal(Marshal.SizeOf<bool>());
            m_IsDirty = (bool*)m_StateAddress.ToPointer();
            *m_IsDirty = true;
            rotation = Quaternion.identity;
            position = Vector3.zero;
            left = 1;
            right = 1;
            bottom = 1;
            top = 1;
            SetProperties(camera);
        }

        public bool isDirty { get { return *m_IsDirty; } set { *m_IsDirty = value; } }

        public Vector3 position
        {
            get { return new Vector3(m_Data->posX, m_Data->posY, m_Data->posZ); }
            set
            {
                if (m_Data->posX != value.x || m_Data->posY != value.y || m_Data->posZ != value.z)
                {
                    m_Data->posX = value.x; m_Data->posY = value.y; m_Data->posZ = value.z;
                    *m_IsDirty = true;
                }
            }
        }

        public Quaternion rotation
        {
            get { return new Quaternion(m_Data->quatX, m_Data->quatY, m_Data->quatZ, m_Data->quatW); }
            set
            {
                if (m_Data->quatX != value.x || m_Data->quatY != value.y || m_Data->quatZ != value.z || m_Data->quatW != value.w)
                {
                    m_Data->quatX = value.x; m_Data->quatY = value.y; m_Data->quatZ = value.z; m_Data->quatW = value.w;
                    *m_IsDirty = true;
                }
            }
        }

        public bool isOffCenter
        {
            get { return m_Data->isOffCenter; }
            set
            {
                if (m_Data->isOffCenter != value)
                {
                    m_Data->isOffCenter = value;
                    *m_IsDirty = true;
                }
            }
        }

        public bool isOrthographic
        {
            get { return m_Data->isOrthographic; }
            set
            {
                if (m_Data->isOrthographic != value)
                {
                    m_Data->isOrthographic = value;
                    MinMaxNearAndFar();
                    *m_IsDirty = true;
                }
            }
        }

        public float size
        {
            get { return m_Data->size; }
            set
            {
                if (m_Data->size != value)
                {
                    m_Data->size = value;
                    MinValue(ref m_Data->size);
                    *m_IsDirty = true;
                }
            }
        }

        public float fov
        {
            get { return m_Data->fov; }
            set
            {
                if (m_Data->fov != value)
                {
                    m_Data->fov = value;
                    MinMaxFOV();
                    *m_IsDirty = true;
                }
            }
        }

        public float aspect
        {
            get { return m_Data->aspect; }
            set
            {
                if (m_Data->aspect != value)
                {
                    m_Data->aspect = value;
                    MinValue(ref m_Data->aspect);
                    *m_IsDirty = true;
                }
            }
        }

        public float left
        {
            get { return m_Data->left; }
            set
            {
                if (m_Data->left != value)
                {
                    m_Data->left = value;
                    MinValue(ref m_Data->left);
                    *m_IsDirty = true;
                }
            }
        }

        public float right
        {
            get { return m_Data->right; }
            set
            {
                if (m_Data->right != value)
                {
                    m_Data->right = value;
                    MinValue(ref m_Data->right);
                    *m_IsDirty = true;
                }
            }
        }

        public float bottom
        {
            get { return m_Data->bottom; }
            set
            {
                if (m_Data->bottom != value)
                {
                    m_Data->bottom = value;
                    MinValue(ref m_Data->bottom);
                    *m_IsDirty = true;
                }
            }
        }

        public float top
        {
            get { return m_Data->top; }
            set
            {
                if (m_Data->top != value)
                {
                    m_Data->top = value;
                    MinValue(ref m_Data->top);
                    *m_IsDirty = true;
                }
            }
        }

        public float near
        {
            get { return m_Data->near; }
            set
            {
                if (m_Data->near != value)
                {
                    m_Data->near = value;
                    MinMaxNearAndFar();
                    *m_IsDirty = true;
                }
            }
        }

        public float far
        {
            get { return m_Data->far; }
            set
            {
                if (m_Data->far != value)
                {
                    m_Data->far = value;
                    MinMaxNearAndFar();
                    *m_IsDirty = true;
                }
            }
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
                    if (isOffCenter)
                    {
                        return Matrix4x4.Ortho(-left, right, -bottom, top, near, far);
                    }
                    else
                    {
                        float y = size * 0.5f;
                        float x = y * aspect;
                        return Matrix4x4.Ortho(-x, x, -y, y, near, far);
                    }
                }
                else
                {
                    if (isOffCenter)
                    {
                        return Matrix4x4.Frustum(-left, right, -bottom, top, near, far);
                    }
                    else
                    {
                        return Matrix4x4.Perspective(fov, aspect, near, far);
                    }
                }
            }
        }

        public Matrix4x4 cullingMatrix { get => projectionMatrix * worldToViewMatrix; }

        public ref VirtualCameraData GetRefData() => ref *m_Data;

        public VirtualCameraData* GetData() => m_Data;

        public CameraPlane GetPlane(float distance) => new CameraPlane(this, distance);

        public void SetProperties(Camera camera)
        {
            isOffCenter = false;
            isOrthographic = camera.orthographic;
            size = camera.orthographicSize * 2;
            fov = camera.fieldOfView;
            aspect = camera.aspect;
            near = camera.nearClipPlane;
            far = camera.farClipPlane;
        }

        public void Dispose()
        {
            Release(ref m_DataAddress);
            Release(ref m_StateAddress);
            m_Data = null;
            m_IsDirty = null;
        }

        private void MinMaxNearAndFar()
        {
            if (!m_Data->isOrthographic)
            {
                MinValue(ref m_Data->near);
            }
            if (m_Data->far <= m_Data->near) m_Data->far = m_Data->near + MIN_VALUE;
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

        private void Release(ref IntPtr ptr)
        {
            if (ptr != IntPtr.Zero)
            {
                Marshal.Release(ptr);
                ptr = IntPtr.Zero;
            }
        }

        public override bool Equals(object obj) => obj is VirtualCamera camera && Equals(camera);

        public bool Equals(VirtualCamera other) => m_Data->Equals(*other.m_Data);

        public override int GetHashCode() => m_Data->GetHashCode();

        public static bool operator ==(VirtualCamera left, VirtualCamera right) => left.Equals(right);

        public static bool operator !=(VirtualCamera left, VirtualCamera right) => !(left == right);
    }
}