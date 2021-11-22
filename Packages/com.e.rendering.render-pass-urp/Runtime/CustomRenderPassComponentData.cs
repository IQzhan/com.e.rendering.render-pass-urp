using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace E.Rendering
{
    public abstract class CustomRenderPassComponentData : ScriptableObject
    {
        /// <summary>
        /// Controls when the render pass executes.
        /// <para>See: <seealso cref="RenderPassEvent"/></para>
        /// </summary>
        public RenderPassEvent renderPassEvent = RenderPassEvent.AfterRenderingTransparents;

        /// <summary>
        /// Controls render order in pass, smaller, earlier.
        /// </summary>
        [Range(0, 99)]
        public int order = 0;

        /// <summary>
        /// Input requirements.
        /// <para>See: <seealso cref="ScriptableRenderPassInput"/></para>
        /// </summary>
        public ScriptableRenderPassInput passInput = ScriptableRenderPassInput.None;

        /// <summary>
        /// Is this pass a Post-Processing?
        /// <para>if false, pass will enabled only effected by <see cref="CustomRenderPassComponent.IsActive"/>,
        /// otherwise effected by both <see cref="CustomRenderPassComponent.IsActive"/> and <see cref="RenderingData.cameraData.postProcessingEnabled"/>.</para>
        /// </summary>
        public bool isPostProcessing = false;
    }

    /// <summary>
    /// Base class of render pass component data.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public abstract class CustomRenderPassComponentData<T> : CustomRenderPassComponentData
        where T : CustomRenderPassComponentData<T>
    {
        private static T instance;

        /// <summary>
        /// Only instance.
        /// </summary>
        public static T Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = Utility.LoadResource<T>();
                }
                return instance;
            }
        }
    }
}