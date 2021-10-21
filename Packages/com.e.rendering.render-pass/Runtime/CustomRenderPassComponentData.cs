using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace E.Rendering
{
    public abstract class CustomRenderPassComponentData : ScriptableObject
    {
        public RenderPassEvent renderPassEvent = RenderPassEvent.AfterRenderingTransparents;

        [Range(0, 99)]
        public int order = 0;

        public ScriptableRenderPassInput passInput = ScriptableRenderPassInput.None;
    }

    public abstract class CustomRenderPassComponentData<T> : CustomRenderPassComponentData
        where T : CustomRenderPassComponentData<T>
    {
        private static T instance;

        public static T Instance
        {
            get
            {
                if (instance == null)
                {
                    string assetName = typeof(T).Name;
                    instance = Utility.LoadResource<T>(assetName);
                }
                return instance;
            }
        }
    }
}