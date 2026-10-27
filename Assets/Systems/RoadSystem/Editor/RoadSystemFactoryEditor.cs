using UnityEditor;
using UnityEngine;

public static class RoadSystemFactoryEditor
{
    public static T LoadPrefab<T>(string path) where T : Object
    {
        return AssetDatabase.LoadAssetAtPath<T>(path);
    }
}
