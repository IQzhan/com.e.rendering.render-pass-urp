using UnityEngine;
using UnityEngine.Rendering;

namespace E.Rendering
{
    public partial class CustomRenderPassComponent
    {

        protected void SetCamera(in Vector3 position, in Quaternion rotation,
            in bool isOrthographics, in float orthoSize, in float fov, in float aspact,
            in float near, in float far)
        {
            //TODO

        }

        protected void ResetCamera()
        {
            //TODO
        }

        /// <summary>
        /// Use this instead of ScriptableRenderContext.DrawRenderers
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
        /// especially if source and destination are the same
        /// </summary>
        /// <param name="source"></param>
        /// <param name="destination"></param>
        /// <param name="material"></param>
        /// <param name="passIndex"></param>
        protected void Blit(in RenderTargetIdentifier source, in RenderTargetIdentifier destination, in Material material = null, in int passIndex = 0)
        {
            Pass.SafeBlit(Command, source, destination, material, passIndex);
        }

        /// <summary>
        /// Use this instead of CommandBuffer.Blit
        /// </summary>
        /// <param name="destination"></param>
        /// <param name="material"></param>
        /// <param name="passIndex"></param>
        protected void Blit(in RenderTargetIdentifier destination, in Material material, in int passIndex)
        {
            Blit(BuiltinRenderTextureType.None, destination, material, passIndex);
        }
    }
}