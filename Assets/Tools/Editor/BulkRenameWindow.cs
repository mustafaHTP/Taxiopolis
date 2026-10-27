using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class BulkRenameWindow : EditorWindow
{
    private string _prefix = "";
    private string _match = "";
    private string _replace = "";

    private bool _usePrefix = true;
    private bool _useMatchReplace;

    private Vector2 _scrollPosition;

    [MenuItem("Tools/Bulk Rename")]
    private static void Open()
    {
        GetWindow<BulkRenameWindow>("Bulk Rename");
    }

    private void OnGUI()
    {
        EditorGUILayout.Space(8);

        EditorGUILayout.LabelField(
            "Bulk Rename",
            EditorStyles.boldLabel);

        EditorGUILayout.HelpBox(
            "Select prefab assets in the Project window. " +
            "The asset filename and the prefab root GameObject name will both be changed.",
            MessageType.Info);

        EditorGUILayout.Space(8);

        Object[] selection = Selection.objects;

        EditorGUILayout.LabelField(
            $"Selected Objects: {selection.Length}",
            EditorStyles.boldLabel);

        EditorGUILayout.Space(8);

        // --------------------------------------------------
        // PREFIX
        // --------------------------------------------------

        _usePrefix = EditorGUILayout.BeginToggleGroup(
            "Add Prefix",
            _usePrefix);

        _prefix = EditorGUILayout.TextField(
            "Prefix",
            _prefix);

        EditorGUILayout.EndToggleGroup();

        EditorGUILayout.Space(5);

        // --------------------------------------------------
        // MATCH & REPLACE
        // --------------------------------------------------

        _useMatchReplace = EditorGUILayout.BeginToggleGroup(
            "Match & Replace",
            _useMatchReplace);

        _match = EditorGUILayout.TextField(
            "Match",
            _match);

        _replace = EditorGUILayout.TextField(
            "Replace",
            _replace);

        EditorGUILayout.EndToggleGroup();

        EditorGUILayout.Space(8);

        // --------------------------------------------------
        // PREVIEW
        // --------------------------------------------------

        EditorGUILayout.LabelField(
            "Preview",
            EditorStyles.boldLabel);

        // Flexible preview area
        DrawPreview();

        // --------------------------------------------------
        // BUTTON
        // --------------------------------------------------

        EditorGUILayout.Space(8);

        GUI.enabled =
            selection.Length > 0 &&
            HasValidOperation();

        if (GUILayout.Button(
                "Rename Selected",
                GUILayout.Height(32)))
        {
            RenameSelected();
        }

        GUI.enabled = true;

        EditorGUILayout.Space(5);
    }

    private void DrawPreview()
    {
        Object[] selection = Selection.objects;

        if (selection.Length == 0)
        {
            EditorGUILayout.HelpBox(
                "No objects selected.",
                MessageType.Warning);

            return;
        }

        // Calculate available space so the button
        // remains visible at the bottom.
        float previewHeight =
            Mathf.Max(80f, position.height - 270f);

        _scrollPosition = EditorGUILayout.BeginScrollView(
            _scrollPosition,
            GUILayout.Height(previewHeight));

        foreach (Object selectedObject in selection)
        {
            if (selectedObject == null)
                continue;

            string assetPath =
                AssetDatabase.GetAssetPath(selectedObject);

            string oldName;

            if (!string.IsNullOrEmpty(assetPath))
            {
                oldName =
                    System.IO.Path.GetFileNameWithoutExtension(
                        assetPath);
            }
            else
            {
                oldName = selectedObject.name;
            }

            string newName = GetNewName(oldName);

            EditorGUILayout.BeginHorizontal();

            EditorGUILayout.LabelField(
                oldName,
                GUILayout.Width(180));

            EditorGUILayout.LabelField(
                "→",
                GUILayout.Width(20));

            EditorGUILayout.LabelField(newName);

            EditorGUILayout.EndHorizontal();
        }

        EditorGUILayout.EndScrollView();
    }

    private void DrawGameObjectPreview(Object selectedObject)
    {
        string oldName = selectedObject.name;
        string newName = GetNewName(oldName);

        EditorGUILayout.BeginHorizontal();

        EditorGUILayout.LabelField(
            oldName,
            GUILayout.Width(180));

        EditorGUILayout.LabelField(
            "→",
            GUILayout.Width(20));

        EditorGUILayout.LabelField(newName);

        EditorGUILayout.EndHorizontal();
    }

    private string GetNewName(string originalName)
    {
        string newName = originalName;

        if (_useMatchReplace &&
            !string.IsNullOrEmpty(_match))
        {
            newName = newName.Replace(
                _match,
                _replace);
        }

        if (_usePrefix &&
            !string.IsNullOrEmpty(_prefix))
        {
            newName = _prefix + newName;
        }

        return newName;
    }

    private bool HasValidOperation()
    {
        bool hasPrefix =
            _usePrefix &&
            !string.IsNullOrEmpty(_prefix);

        bool hasMatchReplace =
            _useMatchReplace &&
            !string.IsNullOrEmpty(_match);

        return hasPrefix || hasMatchReplace;
    }

    private void RenameSelected()
    {
        Object[] selection = Selection.objects;

        int renamedCount = 0;

        foreach (Object selectedObject in selection)
        {
            if (selectedObject == null)
                continue;

            string assetPath =
                AssetDatabase.GetAssetPath(selectedObject);

            // --------------------------------------------------
            // PROJECT WINDOW ASSET
            // --------------------------------------------------

            if (!string.IsNullOrEmpty(assetPath))
            {
                RenameAsset(selectedObject, assetPath, ref renamedCount);
            }
            // --------------------------------------------------
            // HIERARCHY GAMEOBJECT
            // --------------------------------------------------

            else if (selectedObject is GameObject gameObject)
            {
                RenameGameObject(
                    gameObject,
                    ref renamedCount);
            }
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log(
            $"Bulk Rename: Renamed {renamedCount} object(s).");
    }

    private void RenameAsset(
        Object selectedObject,
        string assetPath,
        ref int renamedCount)
    {
        string oldAssetName =
            System.IO.Path.GetFileNameWithoutExtension(
                assetPath);

        string newAssetName =
            GetNewName(oldAssetName);

        if (oldAssetName == newAssetName)
            return;

        string extension =
            System.IO.Path.GetExtension(assetPath);

        string newPath =
            System.IO.Path.Combine(
                System.IO.Path.GetDirectoryName(assetPath),
                newAssetName + extension);

        // --------------------------------------------------
        // Rename the actual asset file
        // --------------------------------------------------

        string error = AssetDatabase.RenameAsset(
            assetPath,
            newAssetName);

        if (!string.IsNullOrEmpty(error))
        {
            Debug.LogError(
                $"Failed to rename '{oldAssetName}': {error}");

            return;
        }

        // --------------------------------------------------
        // Rename prefab root GameObject
        // --------------------------------------------------

        string renamedPath =
            AssetDatabase.GUIDToAssetPath(
                AssetDatabase.AssetPathToGUID(assetPath));

        Object loadedAsset =
            AssetDatabase.LoadAssetAtPath<Object>(
                renamedPath);

        if (loadedAsset is GameObject prefab)
        {
            prefab.name = newAssetName;

            EditorUtility.SetDirty(prefab);
        }

        renamedCount++;
    }

    private void RenameGameObject(
        GameObject gameObject,
        ref int renamedCount)
    {
        string oldName = gameObject.name;
        string newName = GetNewName(oldName);

        if (oldName == newName)
            return;

        Undo.RecordObject(
            gameObject,
            "Bulk Rename GameObject");

        gameObject.name = newName;

        EditorUtility.SetDirty(gameObject);

        renamedCount++;
    }
}