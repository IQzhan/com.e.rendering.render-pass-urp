using System.Collections.Generic;
using System.Linq;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace E.Rendering.Universal
{
    /// <summary>
    /// Beginning of this render-pass system.
    /// <para>Add by click: ScriptableRendererData -> Add Renderer Feature -> CustomRendererFeature</para>
    /// </summary>
    public class CustomRendererFeature : ScriptableRendererFeature
    {
        /// <summary>
        /// Should non-post-process render-pass component enabled in scene camera?
        /// </summary>
        public bool nonPostProcessEnabledInSceneCamera = true;

        private List<CustomRenderPass> m_RenderPasses;

        private List<CustomRenderPassComponent> m_Components;

        private RenderTargetHandle m_TempColorTarget;
        private RenderTargetHandle m_AfterPostProcessTexture;

        public override void Create()
        {
            CollectComponents();
            CollectRenderPasses();
            InitializeRenderTarget();
        }

        private void CollectComponents()
        {
            VolumeManager.instance.CheckBaseTypes();
            VolumeStack stack = VolumeManager.instance.stack;
            VolumeManager.instance.CheckStack(stack);
            m_Components = VolumeManager.instance.baseComponentTypeArray
                .Where(t => t.IsSubclassOf(typeof(CustomRenderPassComponent)))
                .Select(t => stack.GetComponent(t) as CustomRenderPassComponent)
                .OrderBy(c => (int)c.PassEvent * 100 + c.Order)
                .ToList();
        }

        private void CollectRenderPasses()
        {
            m_RenderPasses = m_Components
                .GroupBy(c => c.PassEvent)
                .Select(g =>
                {
                    ScriptableRenderPassInput passInput = ScriptableRenderPassInput.None;
                    foreach (var c in g) { passInput |= c.PassInput; }
                    return new CustomRenderPass($"CustomRenderPass.{g.Key}", g.Key, passInput, g.ToList());
                })
                .ToList();
        }

        private void InitializeRenderTarget()
        {
            m_AfterPostProcessTexture.Init("_AfterPostProcessTexture");
            m_TempColorTarget.Init("_TemporaryColorTarget");
        }

        public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
        {
            int count = m_RenderPasses.Count;
            if (count == 0) return;
#if UNITY_EDITOR
            bool isSceneCamera = renderingData.cameraData.cameraType == UnityEngine.CameraType.SceneView;
            bool nonPostProcessEnabled = (isSceneCamera && nonPostProcessEnabledInSceneCamera) || !isSceneCamera;
#else
            bool nonPostProcessEnabled = true;
#endif
            bool postProcessEnabled = renderingData.cameraData.postProcessEnabled;
            if (!nonPostProcessEnabled && !postProcessEnabled) return;
            RenderTargetHandle colorTarget = new RenderTargetHandle(renderer.cameraColorTarget);
            for (int i = 0; i < count; i++)
            {
                CustomRenderPass renderPass = m_RenderPasses[i];
                if (renderPass.CheckActiveComponents(nonPostProcessEnabled, postProcessEnabled))
                {
                    if (postProcessEnabled &&
                        renderPass.renderPassEvent == RenderPassEvent.AfterRendering)
                    { colorTarget = m_AfterPostProcessTexture; }
                    renderPass.Setup(colorTarget, m_TempColorTarget);
                    renderer.EnqueuePass(renderPass);
                }
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (m_Components != null)
                {
                    for (int i = 0; i < m_Components.Count; i++)
                    {
                        m_Components[i].Dispose();
                    }
                    m_Components.Clear();
                    m_Components = null;
                }
                if (m_RenderPasses != null)
                {
                    for (int i = 0; i < m_RenderPasses.Count; i++)
                    {
                        m_RenderPasses[i].Dispose();
                    }
                    m_RenderPasses.Clear();
                    m_RenderPasses = null;
                }
            }
            base.Dispose(disposing);
        }
    }
}