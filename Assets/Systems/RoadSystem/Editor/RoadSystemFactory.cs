using UnityEditor;
using UnityEngine;

namespace RoadSystem.Utils
{
    public static class RoadSystemFactory
    {
        public static T LoadPrefab<T>(string path) where T : Object
        {
            return AssetDatabase.LoadAssetAtPath<T>(path);
        }
    }
}
