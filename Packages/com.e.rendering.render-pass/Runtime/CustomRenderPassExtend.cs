using UnityEngine;
using UnityEngine.Rendering;

namespace E.Rendering
{
    public partial class CustomRenderPass
    {
        internal void SetCameraMatrix(in VirtualCamera virtualCamera)
        {
            camera.aspect = virtualCamera.aspect;
            SetCameraMatrices(virtualCamera.worldToViewMatrix, virtualCamera.projectionMatrix, virtualCamera.cullingMatrix);
        }

        //internal void SetCameraMatrix(in Vector3 position, in Quaternion rotation,
        //    in bool isOrthographic,
        //    in float size, in float fov, in float aspect,
        //    in float near, in float far,
        //    out Matrix4x4 worldToViewMatrix, out Matrix4x4 projectionMatrix, out Matrix4x4 cullingMatrix)
        //{
        //    camera.aspect = aspect;
        //    worldToViewMatrix = Utility.GetViewMatrix(position, rotation);
        //    projectionMatrix = Utility.GetProjectionMatrix(isOrthographic, size, fov, aspect, near, far);
        //    cullingMatrix = projectionMatrix * worldToViewMatrix;
        //    SetCameraMatrices(worldToViewMatrix, projectionMatrix, cullingMatrix);
        //}

        //internal void SetCameraMatrix(in Vector3 position, in Quaternion rotation,
        //    in bool isOrthographic,
        //    in float left, in float right, in float bottom, in float top,
        //    in float near, in float far,
        //    out Matrix4x4 worldToViewMatrix, out Matrix4x4 projectionMatrix, out Matrix4x4 cullingMatrix)
        //{
        //    worldToViewMatrix = Utility.GetViewMatrix(position, rotation);
        //    projectionMatrix = Utility.GetProjectionMatrix(isOrthographic, left, right, bottom, top, near, far);
        //    cullingMatrix = projectionMatrix * worldToViewMatrix;
        //    SetCameraMatrices(worldToViewMatrix, projectionMatrix, cullingMatrix);
        //}

        private void SetCameraMatrices(in Matrix4x4 worldToViewMatrix, in Matrix4x4 projectionMatrix, in Matrix4x4 cullingMatrix)
        {
            camera.worldToCameraMatrix = worldToViewMatrix;
            camera.projectionMatrix = projectionMatrix;
            camera.cullingMatrix = cullingMatrix;
            //SetViewProjectionMatrices is not compatible with URP?
            //https://docs.unity3d.com/2021.2/Documentation/ScriptReference/Rendering.CommandBuffer.SetViewProjectionMatrices.html
            //command.SetViewProjectionMatrices(worldToViewMatrix, projectionMatrix);
            command.SetViewMatrix(worldToViewMatrix);
            command.SetProjectionMatrix(projectionMatrix);
        }

        internal void ResetCameraMatrix()
        {
            camera.ResetAspect();
            camera.ResetCullingMatrix();
            camera.ResetProjectionMatrix();
            camera.ResetWorldToCameraMatrix();
        }

        internal void SafeBlit(RenderTargetIdentifier sources, RenderTargetIdentifier destination,
            in Material material = null, in int pass = 0)
        {
            if (destination == currentTargetID)
            {
                if (sources == destination)
                {
                    destination = currentTargetID =
                        currentTargetID == m_TempColorTargetID ? m_ColorTargetID : m_TempColorTargetID;
                }
                else if (IsFinalComponent())
                {
                    destination = currentTargetID = m_ColorTargetID;
                }
            }
            command.SetRenderTarget(destination);
            command.Blit(sources, destination, material, pass);
        }

        internal void DrawRenderers(in CullingResults cullingResults, ref DrawingSettings drawingSettings, ref FilteringSettings filteringSettings)
        {
            using (new ProfilingScope(command, m_InsideProfilingSampler))
            {
                context.ExecuteCommandBuffer(command);
                command.Clear();
                context.DrawRenderers(cullingResults, ref drawingSettings, ref filteringSettings);
            }
        }
    }
}