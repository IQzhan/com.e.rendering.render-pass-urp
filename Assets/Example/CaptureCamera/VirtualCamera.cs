using UnityEngine;

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
    [SerializeField]
    public Vector3 position;

    [SerializeField]
    public Quaternion rotation;

    private const float MIN_VALUE = 0.01f;

    private const float MIN_FOV = 0.01f;

    private const float MAX_FOV = 179.99f;

    [SerializeField]
    private bool m_IsOrthographic;

    [SerializeField]
    private float m_Size;

    [SerializeField]
    private float m_Fov;

    [SerializeField]
    private float m_Aspect;

    [SerializeField]
    private float m_Near;

    [SerializeField]
    private float m_Far;

    public bool isOrthographic
    {
        get { return m_IsOrthographic; }
        set
        {
            m_IsOrthographic = value;
            ResetNearAndFar();
        }
    }

    public float size
    {
        get { return m_Size; }
        set
        {
            m_Size = value;
            if (m_Size < MIN_VALUE) m_Size = MIN_VALUE;
        }
    }

    public float fov
    {
        get { return m_Fov; }
        set
        {
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
            m_Aspect = value;
            if (m_Aspect < MIN_VALUE) m_Aspect = MIN_VALUE;
        }
    }

    public float near
    {
        get { return m_Near; }
        set
        {
            m_Near = value;
            ResetNearAndFar();
        }
    }

    public float far
    {
        get { return m_Far; }
        set
        {
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
            //非斜视锥体
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
            //斜视锥体

        }
    }

    public Matrix4x4 cullingMatrix
    {
        get
        {
            return projectionMatrix * worldToViewMatrix;
        }
    }

    public RenderTexture renderTarget;

    public bool PropertiesChanged()
    {

        return false;
    }

    public void EnqueueUpdate()
    {

    }

    public VirtualCameraPlane GetPlane(in VirtualCamera camera, in float distance)
    {
        return new VirtualCameraPlane(camera, distance);
    }
}