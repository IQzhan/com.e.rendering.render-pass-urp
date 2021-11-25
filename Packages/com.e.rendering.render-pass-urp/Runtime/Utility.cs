using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace E.Rendering.Universal
{
    internal static class Utility
    {
        public static T LoadResource<T>() where T : Object
        {
            string name = typeof(T).Name;
#if UNITY_EDITOR
            string[] guids = AssetDatabase.FindAssets($"{name} t:{name}");
            if (guids.Length > 0)
            {
                string path = AssetDatabase.GUIDToAssetPath(guids[0]);
                return AssetDatabase.LoadAssetAtPath<T>(path);
            }
            return null;
#else
            return Resources.Load<T>(name);
#endif
        }
    }
}