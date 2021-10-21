using System.Collections.Generic;
using System.Linq;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace E.Rendering
{
    public class CustomRendererFeature : ScriptableRendererFeature
    {
        private List<CustomRenderPass> m_RenderPasses;

        private List<CustomRenderPassComponent> m_Components;

        private RenderTargetHandle m_TempColorTarget;
        private RenderTargetHandle m_AfterPostProcessTexture;

        private struct RenderPassEventProps
        {
            public List<CustomRenderPassComponent> volumeComponents;

            public ScriptableRenderPassInput passInput;
        }

        public override void Create()
        {
            Dictionary<RenderPassEvent, RenderPassEventProps> splitByRenderPassEvent = CollectAllComponents();
            CreateRenderPasses(splitByRenderPassEvent);
            Initialize();
        }

        private Dictionary<RenderPassEvent, RenderPassEventProps> CollectAllComponents()
        {
            VolumeManager.instance.CheckBaseTypes();
            VolumeStack stack = VolumeManager.instance.stack;
            VolumeManager.instance.CheckStack(stack);
            m_Components = VolumeManager.instance.baseComponentTypeArray
                .Where(t => t.IsSubclassOf(typeof(CustomRenderPassComponent)))
                .Select(t => stack.GetComponent(t) as CustomRenderPassComponent)
                .OrderBy(c => (int)c.PassEvent * 100 + c.Order)
                .ToList();
            Dictionary<RenderPassEvent, RenderPassEventProps> splitByRenderPassEvent =
                new Dictionary<RenderPassEvent, RenderPassEventProps>();
            for (int i = 0; i < m_Components.Count; i++)
            {
                CustomRenderPassComponent component = m_Components[i];
                if (!splitByRenderPassEvent.TryGetValue(component.PassEvent, out RenderPassEventProps props))
                {
                    splitByRenderPassEvent[component.PassEvent] = props = new RenderPassEventProps()
                    { volumeComponents = new List<CustomRenderPassComponent>() };
                }
                props.volumeComponents.Add(component);
                props.passInput |= component.PassInput;
                splitByRenderPassEvent[component.PassEvent] = props;
            }
            return splitByRenderPassEvent;
        }

        private void CreateRenderPasses(in Dictionary<RenderPassEvent, RenderPassEventProps> splitByRenderPassEvent)
        {
            m_RenderPasses = new List<CustomRenderPass>();
            foreach (KeyValuePair<RenderPassEvent, RenderPassEventProps> kv in splitByRenderPassEvent)
            {
                CustomRenderPass renderPass =
                    new CustomRenderPass("CustomRenderPass." + kv.Key.ToString(), kv.Key, kv.Value.passInput, kv.Value.volumeComponents);
                m_RenderPasses.Add(renderPass);
            }
            splitByRenderPassEvent.Clear();
        }

        private void Initialize()
        {
            m_AfterPostProcessTexture.Init("_AfterPostProcessTexture");
            m_TempColorTarget.Init("_TemporaryColorTarget");
        }

        public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
        {
            if (renderingData.cameraData.postProcessEnabled)
            {
                RenderTargetHandle colorTarget = new RenderTargetHandle(renderer.cameraColorTarget);
                for (int i = 0; i < m_RenderPasses.Count; i++)
                {
                    CustomRenderPass renderPass = m_RenderPasses[i];
                    if (renderPass.CheckActiveComponents())
                    {
                        if (renderPass.renderPassEvent == RenderPassEvent.AfterRendering)
                        { colorTarget = m_AfterPostProcessTexture; }
                        renderPass.Setup(colorTarget, m_TempColorTarget);
                        renderer.EnqueuePass(renderPass);
                    }
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