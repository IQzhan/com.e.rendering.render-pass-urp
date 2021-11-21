using UnityEngine;
using UnityEngine.Rendering;

namespace E.Rendering
{
    public partial class CustomRenderPass
    {
        private struct CameraData
        {
            public bool usePhysicalProperties;

            public void Push(Camera camera)
            {
                usePhysicalProperties = camera.usePhysicalProperties;
            }

            public void Pop(Camera camera)
            {
                camera.usePhysicalProperties = usePhysicalProperties;
            }
        }

        private CameraData m_CameraData;

        internal void SetCameraMatrices(in VirtualCamera virtualCamera)
        {
            SetCameraMatrices(virtualCamera.worldToViewMatrix, virtualCamera.projectionMatrix, virtualCamera.cullingMatrix);
        }

        internal void SetCameraMatrices(in Matrix4x4 worldToViewMatrix, in Matrix4x4 projectionMatrix)
        {
            Matrix4x4 cullingMatrix = projectionMatrix * worldToViewMatrix;
            SetCameraMatrices(worldToViewMatrix, projectionMatrix, cullingMatrix);
        }

        internal void SetCameraMatrices(in Matrix4x4 worldToViewMatrix, in Matrix4x4 projectionMatrix, in Matrix4x4 cullingMatrix)
        {
            //Some properties might not be reset
            m_CameraData.Push(camera);
            camera.worldToCameraMatrix = worldToViewMatrix;
            camera.projectionMatrix = projectionMatrix;
            camera.cullingMatrix = cullingMatrix;
            //SetViewProjectionMatrices is not compatible with URP?
            //https://docs.unity3d.com/2021.2/Documentation/ScriptReference/Rendering.CommandBuffer.SetViewProjectionMatrices.html
            command.SetViewProjectionMatrices(worldToViewMatrix, projectionMatrix);
            //command.SetViewMatrix(worldToViewMatrix);
            //command.SetProjectionMatrix(projectionMatrix);
        }

        internal void ResetCameraMatrices()
        {
            camera.ResetCullingMatrix();
            camera.ResetProjectionMatrix();
            camera.ResetWorldToCameraMatrix();
            m_CameraData.Pop(camera);
        }

        internal void SafeBlit(RenderTargetIdentifier sources, RenderTargetIdentifier destination,
            Material material = null, int pass = 0)
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