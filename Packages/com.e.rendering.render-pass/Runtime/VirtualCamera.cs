using System;
using System.Runtime.InteropServices;
using System.Threading;
using UnityEngine;

namespace E.Rendering
{
    /// <summary>
    /// Calculate four vertex at virtual camera's clip plane.
    /// </summary>
    public struct CameraClipPlane
    {
        public CameraClipPlane(VirtualCamera camera, float distance)
        {
            this.distance = distance;
            float yn, yp, xn, xp;
            yn = camera.bottom;
            yp = camera.top;
            xn = camera.left;
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
            bottomLeft = mat * new Vector4(xn, yn, -distance, 1);
            topLeft = mat * new Vector4(xn, yp, -distance, 1);
            topRight = mat * new Vector4(xp, yp, -distance, 1);
            bottomRight = mat * new Vector4(xp, yn, -distance, 1);
            plane = new Plane(bottomLeft, topLeft, topRight);
        }

        public float distance { get; private set; }

        public Vector3 bottomLeft { get; private set; }

        public Vector3 topLeft { get; private set; }

        public Vector3 topRight { get; private set; }

        public Vector3 bottomRight { get; private set; }

        public Plane plane { get; private set; }
    }

    /// <summary>
    /// Inner data of virtual camera.
    /// </summary>
    public struct VirtualCameraData : IEquatable<VirtualCameraData>
    {
        public int id { get; internal set; }
        public float posX, posY, posZ;
        public float quatX, quatY, quatZ, quatW;
        public bool isOrthographic;
        public float size;
        public float fov;
        public float aspect;
        public float near;
        public float far;
        public float shiftX;
        public float shiftY;

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
                   isOrthographic == other.isOrthographic &&
                   size == other.size &&
                   fov == other.fov &&
                   aspect == other.aspect &&
                   near == other.near &&
                   far == other.far &&
                   shiftX == other.shiftX &&
                   shiftY == other.shiftY;
        }

        public override int GetHashCode() => id;

        public static bool operator ==(VirtualCameraData left, VirtualCameraData right) => left.Equals(right);

        public static bool operator !=(VirtualCameraData left, VirtualCameraData right) => !(left == right);
    }

    /// <summary>
    /// Pure camera data for rendering or others that does not need real camera.
    /// </summary>
    public unsafe struct VirtualCamera : IDisposable, IEquatable<VirtualCamera>
    {
        private const float MIN_VALUE = 0.00001f;

        private const float MIN_FOV = 0.00001f;

        private const float MAX_FOV = 179f;

        private IntPtr m_DataAddress;

        private IntPtr m_StateAddress;

        private VirtualCameraData* m_Data;

        private bool* m_IsDirty;

        private static int m_IdOrder = 0;

        private static void GetNewID(VirtualCameraData* data) => data->id = Interlocked.Increment(ref m_IdOrder);

        /// <summary>
        /// Is this virtual camera created?
        /// </summary>
        public bool IsCreated { get => m_DataAddress != IntPtr.Zero; }

        /// <summary>
        /// Create a virtual camera.
        /// </summary>
        /// <param name="isOrthographic"></param>
        /// <param name="size"></param>
        /// <param name="fov"></param>
        /// <param name="aspect"></param>
        /// <param name="near"></param>
        /// <param name="far"></param>
        /// <param name="shiftX"></param>
        /// <param name="shiftY"></param>
        public VirtualCamera(bool isOrthographic,
            float size, float fov, float aspect, float near, float far,
            float shiftX = 0, float shiftY = 0)
        {
            m_DataAddress = Marshal.AllocHGlobal(Marshal.SizeOf<VirtualCameraData>());
            m_Data = (VirtualCameraData*)m_DataAddress.ToPointer();
            *m_Data = new VirtualCameraData();
            GetNewID(m_Data);
            m_StateAddress = Marshal.AllocHGlobal(Marshal.SizeOf<bool>());
            m_IsDirty = (bool*)m_StateAddress.ToPointer();
            *m_IsDirty = true;
            rotation = Quaternion.identity;
            position = Vector3.zero;
            this.isOrthographic = isOrthographic;
            this.near = near;
            this.far = far;
            this.size = size;
            this.fov = fov;
            this.aspect = aspect;
            this.shiftX = shiftX;
            this.shiftY = shiftY;
        }

        /// <summary>
        /// Create a virtual camera by UnityEngine.Camera and set the same properties.
        /// </summary>
        /// <param name="camera"></param>
        public VirtualCamera(Camera camera)
        {
            m_DataAddress = Marshal.AllocHGlobal(Marshal.SizeOf<VirtualCameraData>());
            m_Data = (VirtualCameraData*)m_DataAddress.ToPointer();
            *m_Data = new VirtualCameraData();
            GetNewID(m_Data);
            m_StateAddress = Marshal.AllocHGlobal(Marshal.SizeOf<bool>());
            m_IsDirty = (bool*)m_StateAddress.ToPointer();
            *m_IsDirty = true;
            rotation = Quaternion.identity;
            position = Vector3.zero;
            SetProperties(camera);
        }

        /// <summary>
        /// Runtime id, dont save it.
        /// </summary>
        public int id => m_Data->id;

        /// <summary>
        /// Mark true if properties has changed,
        /// need to set false by yourself.
        /// </summary>
        public bool isDirty { get => *m_IsDirty; set => *m_IsDirty = value; }

        /// <summary>
        /// Position of this virtual camera.
        /// </summary>
        public Vector3 position
        {
            get => new Vector3(m_Data->posX, m_Data->posY, m_Data->posZ);
            set
            {
                if (m_Data->posX != value.x || m_Data->posY != value.y || m_Data->posZ != value.z)
                {
                    m_Data->posX = value.x; m_Data->posY = value.y; m_Data->posZ = value.z;
                    *m_IsDirty = true;
                }
            }
        }

        /// <summary>
        /// Rotation of this virtual camera.
        /// </summary>
        public Quaternion rotation
        {
            get => new Quaternion(m_Data->quatX, m_Data->quatY, m_Data->quatZ, m_Data->quatW);
            set
            {
                if (m_Data->quatX != value.x || m_Data->quatY != value.y || m_Data->quatZ != value.z || m_Data->quatW != value.w)
                {
                    m_Data->quatX = value.x; m_Data->quatY = value.y; m_Data->quatZ = value.z; m_Data->quatW = value.w;
                    *m_IsDirty = true;
                }
            }
        }

        /// <summary>
        /// Is the camera orthographic (true) or perspective (false)?
        /// </summary>
        public bool isOrthographic
        {
            get => m_Data->isOrthographic;
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

        /// <summary>
        /// Vertical size in orthographic mode,
        /// It is near clip plane's height.
        /// </summary>
        public float size
        {
            get => m_Data->size;
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

        /// <summary>
        /// Gate fitted vertical angle in degree of field of view in perspective mode.
        /// see <see cref="Camera.GetGateFittedFieldOfView"/>
        /// </summary>
        public float fov
        {
            get => m_Data->fov;
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

        /// <summary>
        /// The aspect ratio (width divided by height).
        /// </summary>
        public float aspect
        {
            get => m_Data->aspect;
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

        /// <summary>
        /// The distance of the near clipping plane from the the Camera, in world units.
        /// </summary>
        public float near
        {
            get => m_Data->near;
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

        /// <summary>
        /// The distance of the far clipping plane from the Camera, in world units.
        /// </summary>
        public float far
        {
            get => m_Data->far;
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

        /// <summary>
        /// Gate fitted lens shift x,
        /// see <see cref="Camera.GetGateFittedLensShift"/>
        /// </summary>
        public float shiftX
        {
            get => m_Data->shiftX;
            set
            {
                if (m_Data->shiftX != value)
                {
                    m_Data->shiftX = value;
                    *m_IsDirty = true;
                }
            }
        }

        /// <summary>
        /// Gate fitted lens shift y,
        /// see <see cref="Camera.GetGateFittedLensShift"/>
        /// </summary>
        public float shiftY
        {
            get => m_Data->shiftY;
            set
            {
                if (m_Data->shiftY != value)
                {
                    m_Data->shiftY = value;
                    *m_IsDirty = true;
                }
            }
        }

        /// <summary>
        /// Left side's x coordinate value at near clip plane in view sapce.
        /// </summary>
        public float left
        {
            get => NearX * (-1 + 2 * shiftX);
        }

        /// <summary>
        /// Right side's x coordinate value at near clip plane in view space.
        /// </summary>
        public float right
        {
            get => NearX * (1 + 2 * shiftX);
        }

        /// <summary>
        /// Bottom side's y coordinate value at near clip plane in view space.
        /// </summary>
        public float bottom
        {
            get => NearY * (-1 + 2 * shiftY);
        }

        /// <summary>
        /// Top side's y coordinate value at near clip plane in view space.
        /// </summary>
        public float top
        {
            get => NearY * (1 + 2 * shiftY);
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
                    return Matrix4x4.Ortho(left, right, bottom, top, near, far);
                }
                else
                {
                    return Matrix4x4.Frustum(left, right, bottom, top, near, far);
                }
            }
        }

        /// <summary>
        /// projectionMatrix * worldToViewMatrix
        /// </summary>
        public Matrix4x4 cullingMatrix { get => projectionMatrix * worldToViewMatrix; }

        /// <summary>
        /// Get inner data by ref.
        /// </summary>
        /// <returns></returns>
        public ref VirtualCameraData GetRefData() => ref *m_Data;

        /// <summary>
        /// Get inner data by pointer.
        /// </summary>
        /// <returns></returns>
        public VirtualCameraData* GetData() => m_Data;

        /// <summary>
        /// Get clip plane's four vertex.
        /// </summary>
        /// <param name="distance"></param>
        /// <returns></returns>
        public CameraClipPlane GetClipPlane(float distance) => new CameraClipPlane(this, distance);

        /// <summary>
        /// Use camera's properties.
        /// </summary>
        /// <param name="camera"></param>
        public void SetProperties(Camera camera)
        {
            isOrthographic = camera.orthographic;
            size = camera.orthographicSize * 2;
            aspect = camera.aspect;
            near = camera.nearClipPlane;
            far = camera.farClipPlane;
#if UNITY_2018_2_OR_NEWER
            fov = camera.GetGateFittedFieldOfView();
            Vector2 lensShift = camera.GetGateFittedLensShift();
            shiftX = lensShift.x;
            shiftY = lensShift.y;
#else
            fov = camera.fieldOfView;
            shiftX = 0;
            shiftY = 0;
#endif
        }

        /// <summary>
        /// Must Dispose this virtual camera data if no needed.
        /// </summary>
        public void Dispose()
        {
            Release(ref m_DataAddress);
            Release(ref m_StateAddress);
            m_Data = null;
            m_IsDirty = null;
        }

        private float NearY
        {
            get
            {
                if (isOrthographic)
                {
                    return size * 0.5f;
                }
                else
                {
                    return (float)(near * Math.Tan(fov * 0.00872664625997164788461845384244));
                }
            }
        }

        private float NearX
        {
            get => aspect * NearY;
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