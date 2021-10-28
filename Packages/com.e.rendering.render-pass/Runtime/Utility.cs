using UnityEngine;

namespace E.Rendering
{
    internal static class Utility
    {
        public static T LoadResource<T>(string path) where T : Object
        {
            return Resources.Load<T>(path);
        }

        //public static Matrix4x4 GetViewMatrix(Vector3 position, Quaternion rotation)
        //{
        //    Matrix4x4 m = Matrix4x4.TRS(position, rotation, Vector3.one).inverse;
        //    m.m20 = -m.m20;
        //    m.m21 = -m.m21;
        //    m.m22 = -m.m22;
        //    m.m23 = -m.m23;
        //    return m;
        //}

        //public static Matrix4x4 GetProjectionMatrix(in bool isOrthographic,
        //    in float size, in float fov, in float aspect,
        //    in float near, in float far)
        //{
        //    if (isOrthographic)
        //    {
        //        float y = size;
        //        float x = y * aspect;
        //        return Matrix4x4.Ortho(-x, x, -y, y, near, far);
        //    }
        //    else
        //    {
        //        return Matrix4x4.Perspective(fov, aspect, near, far);
        //    }
        //}

        //public static Matrix4x4 GetProjectionMatrix(in bool isOrthographic,
        //    in float left, in float right, in float bottom, in float top,
        //    in float near, in float far)
        //{
        //    if (isOrthographic)
        //    {
        //        return Matrix4x4.Ortho(-left, right, -bottom, top, near, far);
        //    }
        //    else
        //    {
        //        return Matrix4x4.Frustum(-left, right, -bottom, top, near, far);
        //    }
        //}
    }
}