using RoadSystem;
using RoadSystem.Options;
using RoadSystem.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using Color = UnityEngine.Color;
using LogX = Game.Utils.LogX;
using Quaternion = UnityEngine.Quaternion;
using Vector2 = UnityEngine.Vector2;
using Vector3 = UnityEngine.Vector3;

[CustomEditor(typeof(Road))]
public class RoadEditor : Editor
{
    private event Action<Road> OnRoadChanged;

    // Edit Mode Fields
    private Vector3? _previewAddPosition;
    private Vector3? _previewAddNormal;
    private Vector3? _previewInsertPosition;

    // Link Mode Fields
    private RoadPoint _fromRoadPoint;

    #region Gizmos Methods
    private static void DrawRoadPointConnections(List<RoadPoint> roadPoints)
    {
        if (roadPoints.Count == 0)
            return;

        for (int i = 0; i < roadPoints.Count; ++i)
        {
            DrawRoadPointConnections(roadPoints[i]);
        }
    }

    private static void DrawRoadPointConnections(RoadPoint roadPoint)
    {
        if (roadPoint.Connections.Count == 0)
            return;

        for (int i = 0; i < roadPoint.Connections.Count; i++)
        {
            var connectedRoadPoint = roadPoint.Connections[i];
            var middlePoint = (roadPoint.Position + connectedRoadPoint.Position) * 0.5f;
            var directionToConnectedPoint =
                (connectedRoadPoint.Position - roadPoint.Position).normalized;
            Handles.DrawLine(
                roadPoint.Position,
                connectedRoadPoint.Position,
                RoadSystemOptions.Options.Common.RoadLineThickness);

            RoadDraw.DrawGizmosArrow(
                middlePoint,
                directionToConnectedPoint,
                0.5f,
                45f,
                2f);
        }
    }

    private static void DrawLineBetweenRoadPoints(Road road)
    {
        if (road.RoadPoints.Count == 0) return;

        for (int i = 0; i < road.RoadPoints.Count - 1; i++)
        {
            Vector3 start = road.RoadPoints[i].Position;
            Vector3 end = road.RoadPoints[i + 1].Position;
            Handles.DrawLine(
                start,
                end,
                RoadSystemOptions.Options.Common.RoadLineThickness);
            Gizmos.DrawLine(start, end);
        }
    }

    private static void DrawArrowBetweenRoadPoints(Road road)
    {
        if (road.RoadPoints.Count == 0)
            return;

        for (int i = 0; i < road.RoadPoints.Count - 1; i++)
        {
            Vector3 direction =
                (road.RoadPoints[i + 1].Position - road.RoadPoints[i].Position);
            Vector3 middlePoint = road.RoadPoints[i].Position + direction * 0.5f;

            RoadDraw.DrawGizmosArrow(
                middlePoint,
                direction.normalized,
                0.5f,
                45f,
                2f);
        }
    }

    private static void DrawRoadPointLabels(Road road)
    {
        if (road.RoadPoints.Count == 0)
            return;

        for (int i = 0; i < road.RoadPoints.Count; i++)
        {
            var roadPoint = road.RoadPoints[i];

            Vector3 labelOffset = roadPoint.Position
                + Vector3.up * RoadSystemOptions.Options.Common.LabelOffset.y
                + Vector3.right * RoadSystemOptions.Options.Common.LabelOffset.x
                + Vector3.forward * RoadSystemOptions.Options.Common.LabelOffset.z;

            var style = new GUIStyle(GUI.skin.label)
            {
                fontSize = RoadSystemOptions.Options.Common.LabelFontSize,
                normal = new GUIStyleState { textColor = RoadSystemOptions.Options.Common.LabelTextColor },
                alignment = TextAnchor.MiddleCenter,
                padding = new RectOffset(
                    RoadSystemOptions.Options.Common.LabelPaddingHorizontal.x,
                    RoadSystemOptions.Options.Common.LabelPaddingHorizontal.y,
                    RoadSystemOptions.Options.Common.LabelPaddingVertical.x,
                    RoadSystemOptions.Options.Common.LabelPaddingVertical.y),
            };


            Vector2 labelGUIPoint = HandleUtility.WorldToGUIPoint(labelOffset);
            string labelText = $"Index : {i}";
            Vector2 size = style.CalcSize(new GUIContent(labelText)) * RoadSystemOptions.Options.Common.LabelSize;
            Rect rect = new(labelGUIPoint.x, labelGUIPoint.y, size.x, size.y);

            // Draw background box for label
            Handles.BeginGUI();

            var previousColor = GUI.color;
            GUI.color = RoadSystemOptions.Options.Common.LabelBackgroundColor;
            GUI.DrawTexture(rect, Texture2D.whiteTexture);

            GUI.color = previousColor;
            GUI.Label(rect, labelText, style);

            Handles.EndGUI();
        }
    }

    [DrawGizmo(GizmoType.NonSelected | GizmoType.Selected | GizmoType.Pickable)]
    private static void DrawGizmos(Road road, GizmoType gizmoType)
    {
        if (!RoadEditManager.IsRoadGizmoVisible) return;

        switch (RoadEditManager.RoadEditMode)
        {
            case RoadEditMode.None:
            case RoadEditMode.SpawnTrafficLightController:
            case RoadEditMode.SpawnRoad:
                if ((gizmoType & GizmoType.Selected) != 0)
                {
                    Gizmos.color = RoadSystemOptions.Options.NoneMode.SelectedColor;
                    Handles.color = RoadSystemOptions.Options.NoneMode.SelectedColor;
                    DrawRoadPointLabels(road);
                }
                else if ((gizmoType & GizmoType.NonSelected) != 0)
                {
                    Gizmos.color = RoadSystemOptions.Options.NoneMode.NonSelectedColor;
                    Handles.color = RoadSystemOptions.Options.NoneMode.NonSelectedColor;
                }

                DrawLineBetweenRoadPoints(road);
                DrawArrowBetweenRoadPoints(road);
                break;
            case RoadEditMode.RoadEdit:
                if ((gizmoType & GizmoType.Selected) != 0)
                {
                    DrawRoadPointLabels(road);
                    Gizmos.color = RoadSystemOptions.Options.RoadEditMode.SelectedLineColor;
                }
                else if ((gizmoType & GizmoType.NonSelected) != 0)
                {
                    Gizmos.color = RoadSystemOptions.Options.RoadEditMode.NonSelectedLineColor;
                }

                DrawLineBetweenRoadPoints(road);
                DrawArrowBetweenRoadPoints(road);
                break;
            case RoadEditMode.RoadLink:
                Gizmos.color = (gizmoType & GizmoType.Selected) != 0
                    ? RoadSystemOptions.Options.RoadLinkMode.SelectedLineColor
                    : RoadSystemOptions.Options.RoadLinkMode.NonSelectedLineColor;

                DrawLineBetweenRoadPoints(road);
                DrawArrowBetweenRoadPoints(road);

                Gizmos.color = (gizmoType & GizmoType.Selected) != 0
                    ? RoadSystemOptions.Options.RoadLinkMode.SelectedRoadPointConnectionColor
                    : RoadSystemOptions.Options.RoadLinkMode.NonSelectedRoadPointConnectionColor;

                DrawRoadPointConnections(road.RoadPoints);

                if ((gizmoType & GizmoType.Selected) != 0) DrawRoadPointLabels(road);
                break;
            case RoadEditMode.RoadSpawnPoint:
                Gizmos.color = (gizmoType & GizmoType.Selected) != 0
                    ? RoadSystemOptions.Options.RoadSpawnPointMode.SelectedLineColor
                    : RoadSystemOptions.Options.RoadSpawnPointMode.NonSelectedLineColor;

                DrawLineBetweenRoadPoints(road);
                DrawArrowBetweenRoadPoints(road);
                break;
            case RoadEditMode.TrafficLightLink:
                Gizmos.color = RoadSystemOptions.Options.TrafficLightLinkMode.LineColor;
                DrawLineBetweenRoadPoints(road);
                DrawArrowBetweenRoadPoints(road);
                break;
        }
    }
    #endregion

    #region Unity Editor Methods
    private void OnEnable()
    {
        RoadEditManager.OnModeChanged += RoadEditManager_OnModeChanged;
        Selection.selectionChanged += Selection_SelectionChanged;
        OnRoadChanged += RoadEditor_OnChanged;
    }

    private void OnDisable()
    {
        RoadEditManager.OnModeChanged -= RoadEditManager_OnModeChanged;
        Selection.selectionChanged -= Selection_SelectionChanged;
        OnRoadChanged -= RoadEditor_OnChanged;
    }

    private void OnSceneGUI()
    {
        var road = (Road)target;

        Event e = Event.current;

        switch (RoadEditManager.RoadEditMode)
        {
            case RoadEditMode.RoadEdit:
                // Prevent Unity from hijacking the mouse event
                HandleUtility.AddDefaultControl(GUIUtility.GetControlID(FocusType.Passive));
                HandleEdit(e, road);
                break;
            case RoadEditMode.RoadSpawnPoint:
                HandleUtility.AddDefaultControl(GUIUtility.GetControlID(FocusType.Passive));
                HandleSpawn(e, road);
                break;
            case RoadEditMode.RoadLink:
                HandleUtility.AddDefaultControl(GUIUtility.GetControlID(FocusType.Passive));
                HandleLink(e, road);
                break;
        }
    }
    #endregion

    private void RoadEditor_OnChanged(Road road)
    {
        RoadSystemUtils.EvaluateRotations(road);
    }

    private void Selection_SelectionChanged()
    {
        ResetFields();
    }

    private void RoadEditManager_OnModeChanged()
    {
        ResetFields();
    }

    private void HandleLink(Event e, Road _)
    {
        var activeRoad = (Road)target;

        var roads = FindObjectsByType<Road>(
            FindObjectsInactive.Exclude,
            FindObjectsSortMode.None);
        if (roads == null || roads.Length == 0)
        {
            LogX.LogWarning("No roads found in the scene.");

            return;
        }

        // Draw all roads as selectable buttons
        for (int i = 0; i < roads.Length; i++)
        {
            var currentRoad = roads[i];

            if (currentRoad.RoadPoints.Count == 0) continue;

            // Draw a button for each road point
            foreach (var currentRoadPoint in currentRoad.RoadPoints)
            {
                var handleButtonSize
                    = HandleUtility.GetHandleSize(currentRoadPoint.Position)
                        * RoadSystemOptions.Options.RoadLinkMode.RoadPointButtonSize;

                // Change color based on whether the road point is hovered or selected
                if (_fromRoadPoint != null && _fromRoadPoint.Equals(currentRoadPoint))
                {
                    Handles.color = Color.magenta;
                }
                else
                {
                    Handles.color = Color.white;
                }

                if (Handles.Button(
                    currentRoadPoint.Position,
                    Quaternion.identity,
                    handleButtonSize,
                    handleButtonSize,
                    Handles.SphereHandleCap))
                {
                    // If the from position is already set, we can link the roads
                    if (_fromRoadPoint != null)
                    {
                        // Check they are not the same road point
                        if (_fromRoadPoint == currentRoadPoint)
                        {
                            LogX.LogWarning("Cannot link a road point to itself.");

                            return;
                        }

                        // Check they are not the same road
                        //if (_fromRoadPoint.Parent == currentRoadPoint.Parent)
                        //{
                        //    LogX.LogWarning("Cannot link a road to itself.");

                        //    return;
                        //}

                        // Check if the knot is already connected to the from road
                        bool isExistingConnection = _fromRoadPoint.Connections
                            .Any(c => c == currentRoadPoint);
                        if (isExistingConnection)
                        {
                            LogX.LogWarning("This road point is already connected to the selected road point.");

                            return;
                        }

                        // Check bidirectional connections
                        bool isBidirectionalConnection = currentRoadPoint.Connections
                            .Any(c => c == _fromRoadPoint);
                        if (isBidirectionalConnection)
                        {
                            LogX.LogWarning("This road point is already connected to the selected road point in the opposite direction.");

                            return;
                        }

                        // Create a connection between the two road points
                        Undo.RecordObject(_fromRoadPoint, "Add Road Connection");

                        _fromRoadPoint.Connections.Add(currentRoadPoint);

                        EditorUtility.SetDirty(_fromRoadPoint);

                        // After linking, clear the preview drag position
                        _fromRoadPoint = null;

                        LogX.LogDebug($"Road linked");
                    }
                    // If we don't select a road point yet, set the from road point
                    else
                    {
                        _fromRoadPoint = currentRoadPoint;
                    }

                    Event.current.Use();
                    SceneView.RepaintAll();
                }

                // Draw connections for each road point
                if (currentRoadPoint.Connections.Count > 0 && currentRoadPoint.Parent == activeRoad)
                {
                    foreach (var connectedRoadPoint in currentRoadPoint.Connections)
                    {
                        // Draw button to remove the connection
                        Vector3 middlePoint =
                            (currentRoadPoint.Position + connectedRoadPoint.Position) * 0.5f;
                        Vector2 guiPosition =
                            HandleUtility.WorldToGUIPoint(middlePoint);
                        Rect removeButtonRect =
                            new(
                                guiPosition.x + RoadSystemOptions.Options.RoadLinkMode.RemoveConnectionButtonOffset.x,
                                guiPosition.y + RoadSystemOptions.Options.RoadLinkMode.RemoveConnectionButtonOffset.y,
                                RoadSystemOptions.Options.RoadLinkMode.RemoveConnectionButtonSize.x,
                                RoadSystemOptions.Options.RoadLinkMode.RemoveConnectionButtonSize.y);

                        Handles.BeginGUI();

                        var removeConnectionButtonContent = new GUIContent("X", "Remove this road connection");
                        if (GUI.Button(removeButtonRect, removeConnectionButtonContent))
                        {
                            Undo.RecordObject(currentRoadPoint, "Remove Road Connection");

                            currentRoadPoint.Connections.Remove(connectedRoadPoint);

                            EditorUtility.SetDirty(currentRoad);

                            LogX.LogDebug("Removed road connection");

                            return;
                        }

                        Handles.EndGUI();
                    }
                }
            }
        }

        // Cancel drag and drop
        if (Event.current.type == EventType.MouseUp && Event.current.button == 0)
        {
            _fromRoadPoint = null;

            e.Use();
            SceneView.RepaintAll();
        }

        // If we have a preview drag position, draw a line from it
        if (_fromRoadPoint != null)
        {
            Ray ray = HandleUtility.GUIPointToWorldRay(Event.current.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hitInfo, float.MaxValue))
            {
                Handles.color = RoadSystemOptions.Options.RoadLinkMode.PreviewLinkLineColor;
                Handles.DrawLine(_fromRoadPoint.Position, hitInfo.point);
            }

            SceneView.RepaintAll();
        }
    }

    private void HandleSpawn(Event e, Road _)
    {
        var roads = FindObjectsByType<Road>(
            FindObjectsInactive.Exclude,
            FindObjectsSortMode.None);
        if (roads == null || roads.Length == 0)
        {
            LogX.LogWarning("No roads found in the scene.");
            return;
        }

        // Show spawn points for each road
        for (int i = 0; i < roads.Length; i++)
        {
            var currentRoad = roads[i];

            // Draw a button to set the spawn point for the road
            for (int j = 0; j < currentRoad.RoadPoints.Count; j++)
            {
                var currentRoadPoint = currentRoad.RoadPoints[j];
                float handleButtonSize
                    = HandleUtility.GetHandleSize(currentRoadPoint.Position)
                        * RoadSystemOptions.Options.RoadSpawnPointMode.ButtonSize;

                bool isSpawnPoint = RoadSystemUtils.IsSpawnPoint(currentRoadPoint);
                if (isSpawnPoint)
                {
                    Handles.color = RoadSystemOptions.Options.RoadSpawnPointMode.SelectedRoadPointButtonColor;
                }
                else
                {
                    Handles.color = RoadSystemOptions.Options.RoadSpawnPointMode.NonSelectedRoadPointButtonColor;
                }

                if (Handles.Button(
                    currentRoadPoint.Position,
                    Quaternion.identity,
                    handleButtonSize,
                    handleButtonSize,
                    Handles.SphereHandleCap))
                {
                    // If the road point is already set as the spawn point, clear it
                    if (isSpawnPoint && currentRoadPoint.TryGetComponent(out RoadSpawnPoint roadPointSpawn))
                    {
                        Undo.RecordObject(currentRoadPoint, "Clear Road Point Spawn");

                        DestroyImmediate(roadPointSpawn);

                        EditorUtility.SetDirty(currentRoadPoint);

                        LogX.LogDebug("Road point spawn cleared");
                    }
                    // Otherwise, add a RoadPointSpawn component to the road point
                    else
                    {
                        Undo.RecordObject(currentRoadPoint, "Set Road Point Spawn");

                        currentRoadPoint.gameObject.AddComponent<RoadSpawnPoint>();

                        EditorUtility.SetDirty(currentRoadPoint);

                        LogX.LogDebug("Road point spawn set");
                    }

                    e.Use();
                    SceneView.RepaintAll();
                }
            }
        }

    }

    private void HandleEdit(Event e, Road road)
    {
        // OP: Delete last
        if (e.type == EventType.KeyDown && e.keyCode == KeyCode.Backspace)
        {
            if (road.RoadPoints.Count > 0)
            {
                var lastRoadPoint = road.RoadPoints.Last();
                int lastRoadPointIndex = road.RoadPoints.Count - 1;

                // If the last road point has connections, clear them
                if (lastRoadPoint.ConnectionsCount > 0)
                {
                    Undo.RecordObject(lastRoadPoint, "Remove Road Point Connections");
                    lastRoadPoint.ClearConnection();
                    EditorUtility.SetDirty(lastRoadPoint);
                }

                // If the last road point is connected to any other road point, remove the connection
                if (RoadSystemUtils.IsConnectedFromAny(lastRoadPoint))
                {
                    RoadSystemUtils.RemoveConnection(lastRoadPoint);
                }

                Undo.RegisterCompleteObjectUndo(road, "Remove Road Point");
                Undo.DestroyObjectImmediate(lastRoadPoint.gameObject);

                road.RoadPoints.Remove(lastRoadPoint);

                OnRoadChanged?.Invoke(road);

                EditorUtility.SetDirty(road);

                LogX.LogDebug("Last road point removed");
            }

            RoadSystemUtils.UseEventIfNeeded(e);
            SceneView.RepaintAll();
        }
        // OP: Insert
        else if (e.control && e.shift && e.type == EventType.MouseDown && e.button == 0)
        {
            Ray ray = HandleUtility.GUIPointToWorldRay(e.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hitInfo))
            {
                int? closestRoadPointIndexOnRoad = RoadSystemUtils.GetClosestIndexOn(road, hitInfo.point);
                Vector3? closestPositionOnRoad = RoadSystemUtils.GetClosestPositionOn(road, hitInfo.point);

                if (!closestRoadPointIndexOnRoad.HasValue || !closestPositionOnRoad.HasValue)
                {
                    LogX.LogWarning("No closest road point or position found on the road.");
                    RoadSystemUtils.UseEventIfNeeded(e);
                    SceneView.RepaintAll();

                    return;
                }

                var newRoadPointPrefab = RoadSystemFactory.LoadPrefab<RoadPoint>(
                    RoadSystemDefaults.RoadPointPrefabPath);
                var newRoadPoint = Instantiate(
                    newRoadPointPrefab.gameObject,
                    closestPositionOnRoad.Value,
                    Quaternion.identity,
                    road.transform);

                newRoadPoint.name = $"RoadPoint_{road.RoadPoints.Count}";

                if (!newRoadPoint.TryGetComponent<RoadPoint>(out var roadPointComponent))
                {
                    LogX.LogError("Failed to get RoadPoint component from the new road point GameObject.");

                    DestroyImmediate(newRoadPoint);

                    RoadSystemUtils.UseEventIfNeeded(e);
                    SceneView.RepaintAll();

                    return;
                }

                Undo.RegisterCompleteObjectUndo(road, "Road point inserted");
                Undo.RegisterCreatedObjectUndo(newRoadPoint, "Insert Road Point");

                int insertIndex = closestRoadPointIndexOnRoad.Value + 1;
                road.RoadPoints.Insert(insertIndex, roadPointComponent);

                OnRoadChanged?.Invoke(road);

                EditorUtility.SetDirty(road);
            }

            RoadSystemUtils.UseEventIfNeeded(e);
            SceneView.RepaintAll();
        }
        // OP: Delete
        else if (e.shift)
        {
            var roadPoints = road.RoadPoints;
            if (roadPoints.Count == 0)
            {
                LogX.LogWarning("No road points to delete.");

                RoadSystemUtils.UseEventIfNeeded(e);
                SceneView.RepaintAll();

                return;
            }

            // For each road point, draw button to delete it
            Handles.color = RoadSystemOptions.Options.RoadEditMode.RemoveRoadPointButtonColor;
            for (int i = 0; i < roadPoints.Count; i++)
            {
                var roadPoint = roadPoints[i];

                var handleButtonSize
                    = HandleUtility.GetHandleSize(roadPoint.Position)
                        * RoadSystemOptions.Options.RoadEditMode.RemoveRoadPointButtonSize;
                if (Handles.Button(
                    roadPoint.Position,
                    Quaternion.identity,
                    handleButtonSize,
                    handleButtonSize,
                    Handles.SphereHandleCap))
                {
                    // If the road point has connections, clear them
                    if (roadPoint.Connections.Count > 0)
                    {
                        Undo.RecordObject(roadPoint, "Remove Road Point Connections");
                        roadPoint.ClearConnection();
                        EditorUtility.SetDirty(roadPoint);
                    }

                    // If the road point is connected to any other road point, remove the connection
                    if (RoadSystemUtils.IsConnectedFromAny(roadPoint))
                    {
                        RoadSystemUtils.RemoveConnection(roadPoint);
                    }

                    Undo.RegisterCompleteObjectUndo(road, "Remove Road Point");
                    Undo.DestroyObjectImmediate(roadPoint.gameObject);

                    road.RoadPoints.RemoveAt(i);

                    OnRoadChanged?.Invoke(road);

                    EditorUtility.SetDirty(road);
                }
            }
        }
        // OP: Add last
        else if (e.control && e.type == EventType.MouseDown && e.button == 0)
        {
            Ray ray = HandleUtility.GUIPointToWorldRay(e.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hitInfo))
            {
                RoadPoint roadPointPrefab = RoadSystemFactory.LoadPrefab<RoadPoint>(
                    RoadSystemDefaults.RoadPointPrefabPath);
                if (roadPointPrefab == null)
                {
                    LogX.LogError("Failed to load RoadPoint prefab.");
                    RoadSystemUtils.UseEventIfNeeded(e);
                    SceneView.RepaintAll();

                    return;
                }

                var newRoadPoint = Instantiate(
                    roadPointPrefab.gameObject,
                    hitInfo.point,
                    Quaternion.identity,
                    road.transform);

                newRoadPoint.name = $"RoadPoint_{road.RoadPoints.Count}";

                if (!newRoadPoint.TryGetComponent(out RoadPoint roadPointComponent))
                {
                    LogX.LogError("Failed to get RoadPoint component from the new road point GameObject.");

                    DestroyImmediate(newRoadPoint);

                    RoadSystemUtils.UseEventIfNeeded(e);
                    SceneView.RepaintAll();

                    return;
                }

                Undo.RegisterCompleteObjectUndo(road, "Add Road Point");
                Undo.RegisterCreatedObjectUndo(newRoadPoint, "Add Road Point");

                road.RoadPoints.Add(roadPointComponent);

                OnRoadChanged?.Invoke(road);

                EditorUtility.SetDirty(road);
                LogX.LogDebug("Road point added to road");

                _previewAddPosition = hitInfo.point;
                _previewAddNormal = hitInfo.normal;
            }
            RoadSystemUtils.UseEventIfNeeded(e);
            SceneView.RepaintAll();
        }

        // Draw insert preview
        if ((e.control && e.shift) || (e.control && e.shift && e.type == EventType.MouseMove))
        {
            if (road.RoadPoints.Count == 0)
            {
                LogX.LogWarning("No road points to insert into.");

                _previewInsertPosition = null;

                RoadSystemUtils.UseEventIfNeeded(e);
                SceneView.RepaintAll();

                return;
            }

            Ray ray = HandleUtility.GUIPointToWorldRay(e.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hitInfo))
            {
                Vector3? closestPositionOnRoad = RoadSystemUtils.GetClosestPositionOn(road, hitInfo.point);
                _previewInsertPosition = closestPositionOnRoad;

                // Draw Insert preview
                if (_previewInsertPosition.HasValue)
                {
                    Handles.color = RoadSystemOptions.Options.RoadEditMode.PreviewInsertRoadPointColor;
                    Handles.SphereHandleCap(
                        0,
                        _previewInsertPosition.Value,
                        Quaternion.identity,
                        RoadSystemOptions.Options.RoadEditMode.PreviewInsertRoadPointSize,
                        EventType.Repaint);
                    if (closestPositionOnRoad.HasValue)
                    {
                        Handles.color = RoadSystemOptions.Options.RoadEditMode.PreviewInsertLineColor;
                        Handles.DrawLine(_previewInsertPosition.Value, hitInfo.point);
                    }
                }
            }
            else
            {
                _previewInsertPosition = null;
            }

            RoadSystemUtils.UseEventIfNeeded(e);
            SceneView.RepaintAll();
        }
        // Draw add preview
        else if ((e.control) || e.control && e.type == EventType.MouseMove)
        {
            Ray ray = HandleUtility.GUIPointToWorldRay(e.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hitInfo))
            {
                _previewAddPosition = hitInfo.point;
                _previewAddNormal = hitInfo.normal;

                // Draw Add preview
                if (_previewAddPosition.HasValue && _previewAddNormal.HasValue)
                {
                    Handles.color = RoadSystemOptions.Options.RoadEditMode.PreviewAddRoadPointColor;
                    Handles.SphereHandleCap(
                        0,
                        _previewAddPosition.Value,
                        Quaternion.identity,
                        RoadSystemOptions.Options.RoadEditMode.PreviewRoadPointSize,
                        EventType.Repaint);

                    Handles.color = RoadSystemOptions.Options.RoadEditMode.PreviewAddRoadPointNormalColor;
                    Handles.ArrowHandleCap(
                        0,
                        _previewAddPosition.Value,
                        Quaternion.LookRotation(_previewAddNormal.Value),
                        RoadSystemOptions.Options.RoadEditMode.PreviewAddRoadPointNormalSize,
                        EventType.Repaint);
                }
            }
            else
            {
                _previewAddPosition = null;
                _previewAddNormal = null;
            }

            RoadSystemUtils.UseEventIfNeeded(e);
            SceneView.RepaintAll();
        }
    }

    private void ResetFields()
    {
        _previewAddPosition = null;
        _previewAddNormal = null;

        _fromRoadPoint = null;

        SceneView.RepaintAll();
    }
}
