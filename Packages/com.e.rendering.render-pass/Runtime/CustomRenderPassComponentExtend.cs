using UnityEngine;
using UnityEngine.Rendering;

namespace E.Rendering
{
    public partial class CustomRenderPassComponent
    {
        /// <summary>
        /// Set camera matrix
        /// </summary>
        /// <param name="position">camera position</param>
        /// <param name="rotation">camera rotation</param>
        /// <param name="isOrthographic"></param>
        /// <param name="size">half height of orthographic camera</param>
        /// <param name="fov">vertical field of view in degree</param>
        /// <param name="aspect">width/height</param>
        /// <param name="near">near plane</param>
        /// <param name="far">far plane</param>
        protected void SetCameraMatrix(in Vector3 position, in Quaternion rotation,
            in bool isOrthographic,
            in float size, in float fov, in float aspect,
            in float near, in float far,
            out Matrix4x4 worldToViewMatrix, out Matrix4x4 projectionMatrix, out Matrix4x4 cullingMatrix)
        {
            Pass.SetCameraMatrix(position, rotation, isOrthographic, size, fov, aspect, near, far,
                out worldToViewMatrix, out projectionMatrix, out cullingMatrix);
        }

        /// <summary>
        /// Set camera matrix off center
        /// </summary>
        /// <param name="position">camera position</param>
        /// <param name="rotation">camera rotation</param>
        /// <param name="isOrthographic">true is orthographic camera</param>
        /// <param name="left">left distance at near plane</param>
        /// <param name="right">right distance at near plane</param>
        /// <param name="bottom">bottom distance at near plane</param>
        /// <param name="top">top distance at near plane</param>
        /// <param name="near">near plane</param>
        /// <param name="far">far plane</param>
        internal void SetCameraMatrix(in Vector3 position, in Quaternion rotation,
            in bool isOrthographic,
            in float left, in float right, in float bottom, in float top,
            in float near, in float far,
            out Matrix4x4 worldToViewMatrix, out Matrix4x4 projectionMatrix, out Matrix4x4 cullingMatrix)
        {
            Pass.SetCameraMatrix(position, rotation, isOrthographic, left, right, bottom, top, near, far,
                out worldToViewMatrix, out projectionMatrix, out cullingMatrix);
        }

        /// <summary>
        /// Reset camera matrix after use
        /// </summary>
        protected void ResetCameraMatrix()
        {
            Pass.ResetCameraMatrix();
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
            Pass.SafeBlit(source, destination, material, passIndex);
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