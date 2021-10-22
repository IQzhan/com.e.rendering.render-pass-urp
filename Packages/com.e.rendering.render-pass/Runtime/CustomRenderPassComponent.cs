using System;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace E.Rendering
{
    /// <summary>
    /// Base class of render pass,
    /// use VolumeComponentMenu attribute to configure menu in Volume Component like
    /// [VolumeComponentMenu("Custom/Volume Name")]
    /// </summary>
    [Serializable]
    public abstract partial class CustomRenderPassComponent : VolumeComponent, IPostProcessComponent, IDisposable
    {
        protected abstract CustomRenderPassComponentData Data { get; }

        /// <summary>
        /// When to execute this component
        /// </summary>
        public RenderPassEvent PassEvent { get { return Data.renderPassEvent; } }

        /// <summary>
        /// Sorting order in this PassEvent
        /// </summary>
        public int Order { get { return Data.order; } }

        /// <summary>
        /// Require input texture
        /// </summary>
        public ScriptableRenderPassInput PassInput { get { return Data.passInput; } }

        /// <summary>
        /// Is this component a post-processing?
        /// </summary>
        public bool IsPostProcessing { get { return Data.isPostProcessing; } }

        /// <summary>
        /// ScriptableRenderPass of this component
        /// </summary>
        protected CustomRenderPass Pass { get; private set; }

        /// <summary>
        /// true if this component active
        /// </summary>
        /// <returns></returns>
        public abstract bool IsActive();

        public abstract bool IsTileCompatible();

        /// <summary>
        /// Current CommandBuffer of this pass
        /// </summary>
        protected CommandBuffer Command { get { return Pass.command; } }

        /// <summary>
        /// Current ScriptableRenderContext of this pass
        /// </summary>
        protected ScriptableRenderContext Context { get { return Pass.context; } }

        /// <summary>
        /// Current target color texture of this pass, use this.Blit()
        /// </summary>
        protected RenderTargetIdentifier ColorTarget { get { return Pass.currentTargetID; } }

        /// <summary>
        /// Current camera of this pass
        /// </summary>
        protected Camera CurrentCamera { get { return Pass.camera; } }

        internal void Initialize(in CustomRenderPass pass)
        {
            Pass = pass;
            InitializeName();
            Initialize();
        }

        private void InitializeName()
        {
            Type type = GetType();
            object[] attris = type.GetCustomAttributes(typeof(VolumeComponentMenu), false);
            string name;
            if (attris.Length == 1)
            {
                name = (attris[0] as VolumeComponentMenu).menu;
            }
            else
            {
                name = type.Name;
            }
            displayName = name;
        }

        /// <summary>
        /// Execute only once for initialize resources in this component object
        /// such as materials
        /// </summary>
        protected abstract void Initialize();

        /// <summary>
        /// Do before each frame,
        /// Allow to use Command
        /// </summary>
        /// <param name="renderingData"></param>
        public abstract void OnCameraSetup(ref RenderingData renderingData);

        /// <summary>
        /// Render whatever you want each frame,
        /// Allow to use Command and Context
        /// </summary>
        /// <param name="renderingData"></param>
        public abstract void Render(ref RenderingData renderingData);

        /// <summary>
        /// Do after each frame,
        /// Allow to use Command
        /// </summary>
        public abstract void OnCameraCleanup();

        #region Dispose

        private bool m_DisposedValue;

        ~CustomRenderPassComponent()
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
                    Pass = null;
                    DisposedManaged();
                }
                DisposeUnmanaged();
                m_DisposedValue = true;
            }
        }

        /// <summary>
        /// Dispose managed object
        /// </summary>
        protected abstract void DisposedManaged();

        /// <summary>
        /// Dispose unmanaged resources and objects such as materials
        /// </summary>
        protected abstract void DisposeUnmanaged();

        #endregion
    }
}