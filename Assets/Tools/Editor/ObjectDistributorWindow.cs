using UnityEditor;
using UnityEngine;

public class ObjectDistributorWindow : EditorWindow
{
    private int _rows = 1;

    private float _spacingX = 5f;
    private float _spacingZ = 5f;

    private bool _centerGrid;

    private bool _randomRotation;
    private bool _randomScale;

    private float _minScale = 0.9f;
    private float _maxScale = 1.1f;

    private Transform _parent;

    private Vector2 _scrollPosition;

    [MenuItem("Tools/Object Distributor")]
    private static void ShowWindow()
    {
        GetWindow<ObjectDistributorWindow>("Object Distributor");
    }

    private void OnGUI()
    {
        _scrollPosition = EditorGUILayout.BeginScrollView(
            _scrollPosition
        );

        EditorGUILayout.Space(8);

        DrawHeader();

        EditorGUILayout.Space(10);

        DrawParentSettings();

        EditorGUILayout.Space(10);

        DrawDistributionSettings();

        EditorGUILayout.Space(10);

        DrawOptions();

        EditorGUILayout.Space(10);

        DrawSelectionInfo();

        EditorGUILayout.Space(15);

        DrawButtons();

        EditorGUILayout.Space(10);

        EditorGUILayout.EndScrollView();
    }

    private void DrawHeader()
    {
        EditorGUILayout.LabelField(
            "Prop Distributor",
            EditorStyles.boldLabel
        );

        EditorGUILayout.HelpBox(
            "Select objects in the Scene and assign a Parent / Origin. " +
            "The selected objects will be distributed into a grid " +
            "relative to the parent.",
            MessageType.Info
        );
    }

    private void DrawParentSettings()
    {
        EditorGUILayout.LabelField(
            "Parent / Origin",
            EditorStyles.boldLabel
        );

        _parent = (Transform)EditorGUILayout.ObjectField(
            "Parent",
            _parent,
            typeof(Transform),
            true
        );

        if (_parent == null)
        {
            EditorGUILayout.HelpBox(
                "Assign a Transform to use as the grid origin.",
                MessageType.Warning
            );
        }
    }

    private void DrawDistributionSettings()
    {
        EditorGUILayout.LabelField(
            "Distribution",
            EditorStyles.boldLabel
        );

        _rows = Mathf.Max(
            1,
            EditorGUILayout.IntField(
                "Rows",
                _rows
            )
        );

        int selectedCount =
            Selection.gameObjects.Length;

        int columns =
            CalculateColumnCount(selectedCount);

        EditorGUILayout.LabelField(
            "Columns",
            columns.ToString()
        );

        EditorGUILayout.Space(5);

        _spacingX = Mathf.Max(
            0f,
            EditorGUILayout.FloatField(
                "Spacing X",
                _spacingX
            )
        );

        _spacingZ = Mathf.Max(
            0f,
            EditorGUILayout.FloatField(
                "Spacing Z",
                _spacingZ
            )
        );

        EditorGUILayout.Space(5);

        _centerGrid = EditorGUILayout.Toggle(
            "Center Grid",
            _centerGrid
        );
    }

    private void DrawOptions()
    {
        EditorGUILayout.LabelField(
            "Options",
            EditorStyles.boldLabel
        );

        _randomRotation = EditorGUILayout.Toggle(
            "Random Y Rotation",
            _randomRotation
        );

        _randomScale = EditorGUILayout.Toggle(
            "Random Scale",
            _randomScale
        );

        if (_randomScale)
        {
            EditorGUI.indentLevel++;

            _minScale = EditorGUILayout.FloatField(
                "Min Scale",
                _minScale
            );

            _maxScale = EditorGUILayout.FloatField(
                "Max Scale",
                _maxScale
            );

            EditorGUI.indentLevel--;

            _minScale = Mathf.Max(
                0.01f,
                _minScale
            );

            _maxScale = Mathf.Max(
                _minScale,
                _maxScale
            );
        }
    }

    private void DrawSelectionInfo()
    {
        EditorGUILayout.LabelField(
            "Selection",
            EditorStyles.boldLabel
        );

        int selectedCount =
            Selection.gameObjects.Length;

        EditorGUILayout.LabelField(
            "Selected Objects",
            selectedCount.ToString()
        );

        int columns =
            CalculateColumnCount(selectedCount);

        if (selectedCount == 0)
        {
            EditorGUILayout.HelpBox(
                "Select objects in the Scene first.",
                MessageType.Warning
            );

            return;
        }

        EditorGUILayout.LabelField(
            "Grid",
            $"{_rows} Rows × {columns} Columns"
        );

        EditorGUILayout.LabelField(
            "Grid Capacity",
            (columns * _rows).ToString()
        );
    }

    private void DrawButtons()
    {
        bool canDistribute =
            _parent != null &&
            Selection.gameObjects.Length > 0;

        GUI.enabled = canDistribute;

        if (GUILayout.Button(
                "DISTRIBUTE",
                GUILayout.Height(40)))
        {
            Distribute();
        }

        GUI.enabled = true;
    }

    private int CalculateColumnCount(
        int selectedCount)
    {
        if (selectedCount <= 0)
            return 0;

        return Mathf.CeilToInt(
            selectedCount / (float)_rows
        );
    }

    private void Distribute()
    {
        if (_parent == null)
            return;

        GameObject[] selectedObjects =
            Selection.gameObjects;

        if (selectedObjects.Length == 0)
            return;

        int columns =
            CalculateColumnCount(
                selectedObjects.Length
            );

        Undo.SetCurrentGroupName(
            "Distribute Props"
        );

        int undoGroup =
            Undo.GetCurrentGroup();

        for (int i = 0; i < selectedObjects.Length; i++)
        {
            Transform target =
                selectedObjects[i].transform;

            // Don't allow the parent itself to be
            // reparented to itself.
            if (target == _parent)
                continue;

            float originalWorldY =
                target.position.y;

            Undo.RecordObject(
                target,
                "Distribute Prop"
            );

            Undo.SetTransformParent(
                target,
                _parent,
                true,

                "Setting parent for distribution"
            );

            int row =
                i / columns;

            int column =
                i % columns;

            Vector3 localPosition =
                CalculateLocalPosition(
                    row,
                    column,
                    columns
                );

            Vector3 worldPosition =
                _parent.TransformPoint(
                    localPosition
                );

            // Preserve the object's original
            // world-space Y position.
            worldPosition.y =
                originalWorldY;

            target.position =
                worldPosition;

            ApplyRandomization(target);
        }

        Undo.CollapseUndoOperations(
            undoGroup
        );

        SceneView.RepaintAll();
    }

    private Vector3 CalculateLocalPosition(
        int row,
        int column,
        int columns)
    {
        float x =
            column * _spacingX;

        float z =
            row * _spacingZ;

        if (_centerGrid)
        {
            float gridWidth =
                (columns - 1) * _spacingX;

            float gridDepth =
                (_rows - 1) * _spacingZ;

            x -= gridWidth * 0.5f;

            z -= gridDepth * 0.5f;
        }

        return new Vector3(
            x,
            0f,
            z
        );
    }

    private void ApplyRandomization(
        Transform target)
    {
        if (_randomRotation)
        {
            Vector3 rotation =
                target.eulerAngles;

            rotation.y =
                Random.Range(
                    0f,
                    360f
                );

            target.eulerAngles =
                rotation;
        }

        if (_randomScale)
        {
            float multiplier =
                Random.Range(
                    _minScale,
                    _maxScale
                );

            // Multiply existing scale instead
            // of replacing it.
            target.localScale *=
                multiplier;
        }
    }
}