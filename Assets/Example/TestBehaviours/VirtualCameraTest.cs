using E.Rendering;
using UnityEditor;
using UnityEngine;

namespace E.Test
{
    [ExecuteAlways]
    [AutoInstantiate]
    public class VirtualCameraTest : GlobalBehaviour
    {
        protected override bool IsEnabled => true;

        private VirtualCamera virtualCamera;

        private Camera realCamera;

        protected override void OnAwake()
        {

        }

        protected override void OnEnable()
        {
            realCamera = Camera.main;
            virtualCamera = new VirtualCamera(realCamera);
            BehaviourManager.OnDrawGizmosCallback -= OnDrawGizmos;
            BehaviourManager.OnDrawGizmosCallback += OnDrawGizmos;
        }

        protected override void OnUpdate()
        {
            if (realCamera == null) realCamera = Camera.main;
            virtualCamera.SetProperties(realCamera);
            virtualCamera.position = realCamera.transform.position;
            virtualCamera.rotation = realCamera.transform.rotation;
        }

        protected override void OnDisable()
        {
            BehaviourManager.OnDrawGizmosCallback -= OnDrawGizmos;
            virtualCamera.Dispose();
        }

        protected override void OnDestroy()
        {

        }

        private Vector3[] linePoints;

        private void OnDrawGizmos()
        {
            if (virtualCamera.IsCreated)
            {
                CameraClipPlane plane0 = virtualCamera.GetClipPlane(virtualCamera.near);
                CameraClipPlane plane1 = virtualCamera.GetClipPlane(virtualCamera.far);
                Handles.color = Color.green;
                Handles.Label(virtualCamera.position, $"{virtualCamera.position}");
                if (linePoints == null || linePoints.Length < 24)
                { linePoints = new Vector3[24]; }
                SetPlanePoints(0, plane0.bottomLeft, plane0.topLeft, plane0.topRight, plane0.bottomRight);
                SetPlanePoints(8, plane1.bottomLeft, plane1.topLeft, plane1.topRight, plane1.bottomRight);
                linePoints[16] = plane0.bottomLeft;
                linePoints[17] = plane1.bottomLeft;
                linePoints[18] = plane0.topLeft;
                linePoints[19] = plane1.topLeft;
                linePoints[20] = plane0.topRight;
                linePoints[21] = plane1.topRight;
                linePoints[22] = plane0.bottomRight;
                linePoints[23] = plane1.bottomRight;
                Handles.DrawLines(linePoints);
            }
        }

        private void SetPlanePoints(int index, Vector3 v0, Vector3 v1, Vector3 v2, Vector3 v3)
        {
            linePoints[index + 0] = v0;
            linePoints[index + 1] = v1;
            linePoints[index + 2] = v1;
            linePoints[index + 3] = v2;
            linePoints[index + 4] = v2;
            linePoints[index + 5] = v3;
            linePoints[index + 6] = v3;
            linePoints[index + 7] = v0;
        }
    }
}