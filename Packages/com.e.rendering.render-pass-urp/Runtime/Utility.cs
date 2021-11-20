using UnityEngine;

namespace E.Rendering
{
    internal static class Utility
    {
        public static T LoadResource<T>(string path) where T : Object
        {
            return Resources.Load<T>(path);
        }
    }
}