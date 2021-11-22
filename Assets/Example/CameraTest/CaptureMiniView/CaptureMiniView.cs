using E.Rendering;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace E.Test
{
    [VolumeComponentMenu("Custom/Capture Mini View")]
    public class CaptureMiniView : CustomRenderPassComponent
    {
        protected override CustomRenderPassComponentData Data => CaptureMiniViewData.Instance;

        private VirtualCameraBehaviouor m_CameraBehaviouor;

        private DrawingSettings m_DrawingSettings;

        private FilteringSettings m_FilteringSettings;

        private List<ShaderTagId> m_ShaderTagIds;

        /// <summary>
        /// 复制ColorTarget并去除深度值
        /// </summary>
        private RenderTargetHandle tempRT;

        protected override void Initialize()
        {
            m_FilteringSettings = new FilteringSettings(RenderQueueRange.all, 1);
            m_ShaderTagIds =
            new List<ShaderTagId>()
            {
                new ShaderTagId("SRPDefaultUnlit"),
                new ShaderTagId("UniversalForward"),
                new ShaderTagId("UniversalForwardOnly"),
                new ShaderTagId("LightweightForward")
            };
            tempRT.Init("_MiniMapRT");
        }

        public override bool IsTileCompatible()
        {
            return false;
        }

        public override bool IsActive()
        {
            //return size.Value > 0 && material != null;
            return true;
        }

        public override void OnCameraSetup(ref RenderingData renderingData)
        {
            // CommandBuffer cmd = Command;
            
        }

        public override void Render(ref RenderingData renderingData)
        {
            if (m_CameraBehaviouor == null)
            {
                if (BehaviourManager.IsReady)
                {
                    m_CameraBehaviouor = BehaviourManager.GetInstance<VirtualCameraBehaviouor>();
                }
            }
            if (m_CameraBehaviouor == null) return;
            if (m_CameraBehaviouor.TryGetVirtualCamera(out VirtualCamera virtualCamera))
            {
                CommandBuffer cmd = Command;
                ScriptableRenderContext context = Context;
                RenderTextureDescriptor desc = renderingData.cameraData.cameraTargetDescriptor;
                //desc.depthBufferBits = 0;
                desc.msaaSamples = 1;
                cmd.GetTemporaryRT(tempRT.id, desc);
                RenderTargetIdentifier tempID = tempRT.Identifier();
                Blit(ColorTarget, tempID);
                cmd.SetRenderTarget(tempID);
                SetCameraMatrices(virtualCamera.worldToViewMatrix, virtualCamera.projectionMatrix, virtualCamera.cullingMatrix);
                CurrentCamera.TryGetCullingParameters(out ScriptableCullingParameters cullingParameters);
                CullingResults cullingResults = context.Cull(ref cullingParameters);
                var sortFlags = renderingData.cameraData.defaultOpaqueSortFlags;
                m_DrawingSettings = Pass.CreateDrawingSettings(m_ShaderTagIds, ref renderingData, sortFlags);
                cmd.SetViewport(new Rect(0, 0, Screen.width * 0.5f, Screen.height * 0.5f));
                DrawRenderers(cullingResults, ref m_DrawingSettings, ref m_FilteringSettings);
                ResetCameraMatrices();
                Blit(tempID, ColorTarget);
                cmd.ReleaseTemporaryRT(tempRT.id);
            }
        }

        public override void OnCameraCleanup()
        {
            // CommandBuffer cmd = Command;

        }

        protected override void DisposeUnmanaged()
        {

        }

        protected override void DisposeManaged()
        {
            m_CameraBehaviouor = null;
        }
    }
}