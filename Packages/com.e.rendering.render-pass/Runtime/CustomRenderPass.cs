using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace E.Rendering
{
    public partial class CustomRenderPass : ScriptableRenderPass, IDisposable
    {
        private List<CustomRenderPassComponent> m_VolumeComponents;
        private List<int> m_ActiveComponents;
        private int m_CurrActiveComponentIndex;

        private string m_ProfilerTag;
        private List<ProfilingSampler> m_ProfilingSamplers;
        private ProfilingSampler m_InsideProfilingSampler;

        private RenderTargetHandle m_ColorTarget;
        private RenderTargetHandle m_TempColorTarget;

        private RenderTargetIdentifier m_ColorTargetID;
        private RenderTargetIdentifier m_TempColorTargetID;
        internal RenderTargetIdentifier currentTargetID;

        internal CommandBuffer command;
        internal ScriptableRenderContext context;
        internal Camera camera;

        internal CustomRenderPass
            (in string profilerTag, in RenderPassEvent renderPassEvent, in ScriptableRenderPassInput passInput, in List<CustomRenderPassComponent> volumeComponents)
        {
            InitializeProperties(renderPassEvent, passInput);
            InitializeComponents(volumeComponents);
            InitializeProfilingSamplers(profilerTag, volumeComponents);
        }

        private void InitializeProperties(in RenderPassEvent renderPassEvent, in ScriptableRenderPassInput passInput)
        {
            if (renderPassEvent == RenderPassEvent.AfterRenderingPostProcessing)
            { this.renderPassEvent = RenderPassEvent.AfterRendering; }
            else { this.renderPassEvent = renderPassEvent; }
            ConfigureInput(passInput);
        }

        private void InitializeComponents(in List<CustomRenderPassComponent> volumeComponents)
        {
            m_VolumeComponents = volumeComponents;
            for (int i = 0; i < m_VolumeComponents.Count; i++)
            {
                m_VolumeComponents[i].Initialize(this);
            }
            m_ActiveComponents = new List<int>(volumeComponents.Count);
        }

        private void InitializeProfilingSamplers(in string profilerTag, in List<CustomRenderPassComponent> volumeComponents)
        {
            m_ProfilerTag = profilerTag;
            m_ProfilingSamplers = volumeComponents.Select(c => new ProfilingSampler(c.displayName)).ToList();
            m_InsideProfilingSampler = new ProfilingSampler(string.Empty);
        }

        internal bool CheckActiveComponents(in bool nonPostProcessEnabled, in bool postProcessEnabled)
        {
            m_ActiveComponents.Clear();
            for (int i = 0; i < m_VolumeComponents.Count; i++)
            {
                CustomRenderPassComponent component = m_VolumeComponents[i];
                if (((!component.IsPostProcessing && nonPostProcessEnabled) ||
                    (component.IsPostProcessing && postProcessEnabled))
                    && component.IsActive())
                {
                    m_ActiveComponents.Add(i);
                }
            }
            return m_ActiveComponents.Count != 0;
        }

        internal void Setup(RenderTargetHandle colorTarget, RenderTargetHandle TempColorTarget)
        {
            m_ColorTarget = colorTarget;
            m_TempColorTarget = TempColorTarget;
        }

        public override void OnCameraSetup(CommandBuffer cmd, ref RenderingData renderingData)
        {
            ConfigureCommandBuffer(cmd);
            for (int i = 0; i < m_ActiveComponents.Count; i++)
            {
                int index = m_ActiveComponents[i];
                CustomRenderPassComponent component = m_VolumeComponents[index];
                component.OnCameraSetup(ref renderingData);
            }
            ClearCommandBuffer();
        }

        public override void OnCameraCleanup(CommandBuffer cmd)
        {
            ConfigureCommandBuffer(cmd);
            for (int i = 0; i < m_ActiveComponents.Count; i++)
            {
                int index = m_ActiveComponents[i];
                CustomRenderPassComponent component = m_VolumeComponents[index];
                component.OnCameraCleanup();
            }
            ClearCommandBuffer();
        }

        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            CommandBuffer cmd = GetCommand(context);
            ConfigureCommandBuffer(cmd);
            InitializeRenderTextures(cmd, ref renderingData);
            RenderComponents(context, ref renderingData, cmd);
            ReleaseRenderTextures(cmd);
            ReleaseCommand(context, cmd);
            ClearCommandBuffer();
        }

        private CommandBuffer GetCommand(in ScriptableRenderContext context)
        {
            CommandBuffer cmd = CommandBufferPool.Get(m_ProfilerTag);
            context.ExecuteCommandBuffer(cmd);
            cmd.Clear();
            return cmd;
        }

        private void ReleaseCommand(in ScriptableRenderContext context, in CommandBuffer cmd)
        {
            context.ExecuteCommandBuffer(cmd);
            cmd.Clear();
            CommandBufferPool.Release(cmd);
        }

        private void ConfigureCommandBuffer(in CommandBuffer cmd)
        {
            command = cmd;
        }

        private void ClearCommandBuffer()
        {
            command = null;
        }

        private void ConfigureRenderContext(in ScriptableRenderContext context)
        {
            this.context = context;
        }

        private void ConfigureCamera(in Camera camera)
        {
            this.camera = camera;
        }

        private void InitializeRenderTextures(in CommandBuffer cmd, ref RenderingData renderingData)
        {
            RenderTextureDescriptor descriptor = renderingData.cameraData.cameraTargetDescriptor;
            descriptor.msaaSamples = 1;
            descriptor.depthBufferBits = 0;
            if (m_ColorTarget != RenderTargetHandle.CameraTarget && !m_ColorTarget.HasInternalRenderTargetId())
            {
                cmd.GetTemporaryRT(m_ColorTarget.id, descriptor);
            }
            currentTargetID = m_ColorTargetID = m_ColorTarget.Identifier();
            cmd.GetTemporaryRT(m_TempColorTarget.id, descriptor);
            m_TempColorTargetID = m_TempColorTarget.Identifier();
        }

        private void ReleaseRenderTextures(in CommandBuffer cmd)
        {
            cmd.ReleaseTemporaryRT(m_TempColorTarget.id);
        }

        private void RenderComponents(in ScriptableRenderContext context,
            ref RenderingData renderingData, in CommandBuffer cmd)
        {
            ConfigureRenderContext(context);
            ConfigureCamera(renderingData.cameraData.camera);
            m_CurrActiveComponentIndex = -1;
            for (int i = 0; i < m_ActiveComponents.Count; i++)
            {
                RenderComponent(i, context, ref renderingData, cmd);
            }
            FinalBlit(cmd);
        }

        private void RenderComponent(in int activeIndex, in ScriptableRenderContext context,
            ref RenderingData renderingData, in CommandBuffer cmd)
        {
            m_CurrActiveComponentIndex = activeIndex;
            int index = m_ActiveComponents[activeIndex];
            CustomRenderPassComponent component = m_VolumeComponents[index];
            using (new ProfilingScope(cmd, m_ProfilingSamplers[index]))
            {
                component.Render(ref renderingData);
            }
        }

        private bool IsFinalComponent()
        {
            return m_CurrActiveComponentIndex == m_ActiveComponents.Count - 1;
        }

        private void FinalBlit(in CommandBuffer cmd)
        {
            if (currentTargetID == m_TempColorTargetID)
            {
                cmd.Blit(m_TempColorTargetID, m_ColorTargetID);
                currentTargetID = m_ColorTargetID;
            }
        }

        #region Dispose

        private bool m_DisposedValue;

        ~CustomRenderPass()
        {
            Dispose(disposing: false);
        }

        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        private void Dispose(bool disposing)
        {
            if (!m_DisposedValue)
            {
                if (disposing)
                {
                    m_VolumeComponents.Clear();
                    m_VolumeComponents = null;
                    m_ActiveComponents.Clear();
                    m_ActiveComponents = null;
                    m_ProfilerTag = null;
                    m_ProfilingSamplers.Clear();
                    m_ProfilingSamplers = null;
                }
                m_DisposedValue = true;
            }
        }

        #endregion
    }
}