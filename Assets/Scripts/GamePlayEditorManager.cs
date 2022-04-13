#if UNITY_EDITOR
using System;
using System.IO;
using UnityEditor;
using UnityEngine;

public class GamePlayEditorManager
{
    public static void OpenScriptableAtDefault<T>(string defaultName = null) where T : ScriptableObject
    {

        defaultName = defaultName ?? typeof(T).Name;

        var folder = $"Assets/Resources";

        if (!Directory.Exists(folder))
        {
            Directory.CreateDirectory(folder);
        }

        var path = $"{folder}/{defaultName}.asset";
        var asset = AssetDatabase.LoadAssetAtPath<T>(path);

        if (asset == null)
        {
            asset = ScriptableObject.CreateInstance<T>();
            AssetDatabase.CreateAsset(asset, path);
            AssetDatabase.SaveAssets();
        }

        Selection.activeObject = asset;
    }
}

#endif