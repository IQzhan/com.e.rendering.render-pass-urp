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
            realCamera = Camera.main;
            virtualCamera = new VirtualCamera(realCamera);
        }

        protected override void OnEnable()
        {
            BehaviourManager.OnDrawGizmosCallback -= OnDrawGizmos;
            BehaviourManager.OnDrawGizmosCallback += OnDrawGizmos;
        }

        protected override void OnUpdate()
        {
            virtualCamera.SetProperties(realCamera);
            virtualCamera.position = realCamera.transform.position;
            virtualCamera.rotation = realCamera.transform.rotation;
        }

        protected override void OnDisable()
        {
            BehaviourManager.OnDrawGizmosCallback -= OnDrawGizmos;
        }

        protected override void OnDestroy()
        {
            Debug.Log("Destroy camera test");
            virtualCamera.Dispose();
        }

        private void OnDrawGizmos()
        {
            if (virtualCamera.IsCreated)
            {
                CameraPlane plane0 = virtualCamera.GetPlane(virtualCamera.near);
                CameraPlane plane1 = virtualCamera.GetPlane(virtualCamera.far);
                Handles.color = Color.red;
                DrawPlane(plane0.bottomLeft, plane0.topLeft, plane0.topRight, plane0.bottomRight);
                DrawPlane(plane1.bottomLeft, plane1.topLeft, plane1.topRight, plane1.bottomRight);
                DrawLine(plane0.bottomLeft, plane1.bottomLeft);
                DrawLine(plane0.topLeft, plane1.topLeft);
                DrawLine(plane0.topRight, plane1.topRight);
                DrawLine(plane0.bottomRight, plane1.bottomRight);
            }
        }

        private void DrawPlane(Vector3 v0, Vector3 v1, Vector3 v2, Vector3 v3)
        {
            Handles.DrawLine(v0, v1);
            Handles.DrawLine(v1, v2);
            Handles.DrawLine(v2, v3);
            Handles.DrawLine(v3, v0);
        }

        private void DrawLine(Vector3 from, Vector3 to)
        {
            Handles.DrawLine(from, to);
        }
    }
}