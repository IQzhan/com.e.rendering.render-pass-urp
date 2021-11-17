using System;
using UnityEngine;
namespace E.Test
{
    [ExecuteAlways]
    [AutoInstantiate]
    public class SizeofTest : GlobalBehaviour
    {
        protected override bool IsEnabled => true;

        private struct VirtualCameraData
        {
            public bool isDirty;
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

        private struct EmptyData { }

        private struct BoolData
        {
            public bool a;
            public bool b;
        }

        private unsafe struct VTest : IDisposable
        {
            private VirtualCameraData* data;

            public void Create()
            {
                *data = new VirtualCameraData();
            }

            public void Dispose()
            {
                data = null;
            }
        }

        protected override void OnAwake()
        {
            PrintSize<VirtualCameraData>();
            //PrintSize<EmptyData>();
            //PrintSize<BoolData>();
            //PrintConvert();
        }

        private unsafe void PrintConvert()
        {
            int a = 10;
            uint* ay = (uint*)&a;
            uint c = *ay <<= 2;
            Debug.Log(a + " " + c);
            ay = null;
        }

        private void PrintSize<T>() where T : unmanaged
        {
            Debug.Log($"Size of {typeof(T).Name}: {SizeofStruct<T>()}");
        }

        private unsafe int SizeofStruct<T>() where T : unmanaged
        {
            return sizeof(T);
        }
    }
}