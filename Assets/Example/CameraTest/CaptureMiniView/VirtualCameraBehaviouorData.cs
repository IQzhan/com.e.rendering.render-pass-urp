using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace E.Test
{
    public class VirtualCameraBehaviouorData : ScriptableObject
    {
        private static VirtualCameraBehaviouorData m_Instance;

        public static VirtualCameraBehaviouorData Instance
        {
            get
            {
                if(m_Instance == null)
                {
                    m_Instance = Resources.Load<VirtualCameraBehaviouorData>("VirtualCameraBehaviouorData");
                }
                return m_Instance;
            }
        }

        public Vector3 position;

        public Vector3 rotation;

        public bool isOrthographic;

        [Min(0.001f)]
        public float size;

        [Range(0.001f, 179f)]
        public float fov;

        [Min(0.001f)]
        public float aspect;

        public float near;

        public float far;

        public float shiftX;

        public float shiftY;

        public bool showGizmos;
    }
}
