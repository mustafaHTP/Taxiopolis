using System;
using UnityEditor;
using UnityEngine;

public static class RoadEditManager
{
    public static RoadEditMode RoadEditMode { get; private set; } = RoadEditMode.None;
    public static bool IsRoadGizmoVisible { get; set; } = true;
    public static bool IsTrafficLightGizmoVisible { get; set; } = true;
    public static event Action OnModeChanged;

    private static GameObject s_selectedObject;

    static RoadEditManager()
    {
        Register();
    }

    public static void ChangeMode(RoadEditMode roadEditMode)
    {
        RoadEditMode = roadEditMode;
        s_selectedObject = Selection.activeGameObject;
        OnModeChanged?.Invoke();

        //LogX.LogDebug($"Mode changed to {roadEditMode}");
    }

    private static void Register()
    {
        OnModeChanged += RoadEditManager_OnModeChanged;
        Selection.selectionChanged += Selection_SelectionChanged;
    }

    private static void Selection_SelectionChanged()
    {
        if (s_selectedObject == null) return;

        if (Selection.activeObject != null
            && (Selection.activeObject is not GameObject
            || s_selectedObject != Selection.activeObject))
        {
            ChangeMode(RoadEditMode.None);
        }
    }

    private static void RoadEditManager_OnModeChanged()
    {
        SceneView.RepaintAll();
    }

    private static void Unregister()
    {
        OnModeChanged -= RoadEditManager_OnModeChanged;
        Selection.selectionChanged -= Selection_SelectionChanged;
    }
}

public enum RoadEditMode
{
    None = 0,
    RoadEdit = 1,
    RoadLink = 2,
    RoadSpawnPoint = 3,
    SpawnRoad = 4,
    TrafficLightLink = 5,
    SpawnTrafficLightController = 6,
}
