using E.Rendering.Universal;
using UnityEditor;
using UnityEngine;

namespace E.Test
{
    [ExecuteAlways]
    [AutoInstantiate]
    public class VirtualCameraBehaviouor : GlobalBehaviour
    {
        protected override bool IsActive => true;

        private VirtualCamera virtualCamera;

        protected override void OnEnable()
        {
            VirtualCameraBehaviouorData data = VirtualCameraBehaviouorData.Instance;
            virtualCamera = new VirtualCamera(false, 1, 50, 1, 0.03f, 1000f);
            SetData();
            BehaviourManager.OnDrawGizmosCallback -= OnDrawGizmos;
            BehaviourManager.OnDrawGizmosCallback += OnDrawGizmos;
        }

        protected override void OnUpdate()
        {
            SetData();
        }

        public bool TryGetVirtualCamera(out VirtualCamera virtualCamera)
        {
            virtualCamera = this.virtualCamera;
            return virtualCamera.IsCreated;
        }

        private void SetData()
        {
            if (virtualCamera.IsCreated)
            {
                VirtualCameraBehaviouorData data = VirtualCameraBehaviouorData.Instance;
                virtualCamera.position = data.position;
                virtualCamera.rotation = Quaternion.Euler(data.rotation);
                virtualCamera.isOrthographic = data.isOrthographic;
                virtualCamera.size = data.size;
                virtualCamera.fov = data.fov;
                virtualCamera.aspect = data.aspect;
                virtualCamera.near = data.near;
                virtualCamera.far = data.far;
                virtualCamera.shiftX = data.shiftX;
                virtualCamera.shiftY = data.shiftY;
            }
        }

        protected override void OnDisable()
        {
            BehaviourManager.OnDrawGizmosCallback -= OnDrawGizmos;
            if (virtualCamera.IsCreated)
            {
                virtualCamera.Dispose();
            }
        }

        private Vector3[] m_WireFramePoints;

        private int[] m_WireFramePointsOrder;

        private void OnDrawGizmos()
        {
            if (VirtualCameraBehaviouorData.Instance.showGizmos && virtualCamera.IsCreated)
            {
                Handles.color = Color.green;
                Handles.Label(virtualCamera.position, $"{virtualCamera.position}");
                DrawCameraWireFrame();
            }
        }

        private void DrawCameraWireFrame()
        {
            if (m_WireFramePointsOrder == null)
            {
                m_WireFramePointsOrder = new int[]
                {
                    0,1, 1,2, 2,3, 3,0,
                    4,5, 5,6, 6,7, 7,4,
                    0,4, 1,5, 2,6, 3,7
                };
            }
            if (m_WireFramePoints == null) { m_WireFramePoints = new Vector3[8]; }
            CameraClipPlane plane0 = virtualCamera.GetClipPlane(virtualCamera.near);
            CameraClipPlane plane1 = virtualCamera.GetClipPlane(virtualCamera.far);
            m_WireFramePoints[0] = plane0.bottomLeft;
            m_WireFramePoints[1] = plane0.topLeft;
            m_WireFramePoints[2] = plane0.topRight;
            m_WireFramePoints[3] = plane0.bottomRight;
            m_WireFramePoints[4] = plane1.bottomLeft;
            m_WireFramePoints[5] = plane1.topLeft;
            m_WireFramePoints[6] = plane1.topRight;
            m_WireFramePoints[7] = plane1.bottomRight;
            Handles.DrawLines(m_WireFramePoints, m_WireFramePointsOrder);
        }
    }
}