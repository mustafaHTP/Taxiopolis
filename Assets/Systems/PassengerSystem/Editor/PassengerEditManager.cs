using Game.PassengerSystem;
using Game.Systems.PassengerSystem;
using Game.Utils;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;

public static class PassengerEditManager
{
    public static PassengerEditMode CurrentMode { get; private set; } = PassengerEditMode.None;

    public static void SetMode(PassengerEditMode mode)
    {
        CurrentMode = mode;

        LogX.LogDebug($"Passenger Edit Mode changed to {mode}");
    }

    static PassengerEditManager()
    {
        RegisterEvents();
    }

    private static void RegisterEvents()
    {
        // Register necessary events here
        Selection.selectionChanged += Selection_OnSelectionChanged;
    }

    private static void UnregisterEvents()
    {
        // Unregister events here
        Selection.selectionChanged -= Selection_OnSelectionChanged;
    }

    private static void Selection_OnSelectionChanged()
    {
        if (Selection.activeObject == null) return;

        if (Selection.activeObject is not GameObject)
        {
            SetMode(PassengerEditMode.None);
        }

        if (Selection.activeObject is GameObject
            && !Selection.activeObject.GetComponent<PassengerManager>())
        {
            SetMode(PassengerEditMode.None);
        }
    }
}

public enum PassengerEditMode
{
    None,
    AddPoint,
}
