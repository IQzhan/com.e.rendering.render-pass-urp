using UnityEngine;
using UnityEngine.Rendering;

namespace E.Rendering.Universal
{
    public partial class CustomRenderPassComponent
    {
        /// <summary>
        /// Set matrices to current render camera.
        /// </summary>
        /// <param name="worldToViewMatrix"></param>
        /// <param name="projectionMatrix"></param>
        protected void SetCameraMatrices(in Matrix4x4 worldToViewMatrix, in Matrix4x4 projectionMatrix)
        {
            Pass.SetCameraMatrices(worldToViewMatrix, projectionMatrix);
        }

        /// <summary>
        /// Set matrices to current render camera.
        /// </summary>
        /// <param name="worldToViewMatrix"></param>
        /// <param name="projectionMatrix"></param>
        /// <param name="cullingMatrix"></param>
        protected void SetCameraMatrices(in Matrix4x4 worldToViewMatrix, in Matrix4x4 projectionMatrix, in Matrix4x4 cullingMatrix)
        {
            Pass.SetCameraMatrices(worldToViewMatrix, projectionMatrix, cullingMatrix);
        }

        /// <summary>
        /// Reset camera matrix after set.
        /// </summary>
        protected void ResetCameraMatrices()
        {
            Pass.ResetCameraMatrices();
        }

        /// <summary>
        /// Use this instead of ScriptableRenderContext.DrawRenderers.
        /// </summary>
        /// <param name="cullingResults"></param>
        /// <param name="drawingSettings"></param>
        /// <param name="filteringSettings"></param>
        protected void DrawRenderers(in CullingResults cullingResults, ref DrawingSettings drawingSettings, ref FilteringSettings filteringSettings)
        {
            Pass.DrawRenderers(cullingResults, ref drawingSettings, ref filteringSettings);
        }

        /// <summary>
        /// Use this instead of CommandBuffer.Blit
        /// especially if source and destination are the same.
        /// </summary>
        /// <param name="source"></param>
        /// <param name="destination"></param>
        /// <param name="material"></param>
        /// <param name="passIndex"></param>
        protected void Blit(in RenderTargetIdentifier source, in RenderTargetIdentifier destination, Material material = null, int passIndex = 0)
        {
            Pass.SafeBlit(source, destination, material, passIndex);
        }

        /// <summary>
        /// Use this instead of CommandBuffer.Blit.
        /// </summary>
        /// <param name="destination"></param>
        /// <param name="material"></param>
        /// <param name="passIndex"></param>
        protected void Blit(in RenderTargetIdentifier destination, Material material, int passIndex)
        {
            Blit(BuiltinRenderTextureType.None, destination, material, passIndex);
        }
    }
}