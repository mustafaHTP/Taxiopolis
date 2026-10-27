using Game.Systems.PedestrianSystem;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(PedestrianManager))]
public class PedestrianManagerEditor : Editor
{
    private PedestrianManager manager;

    private void OnEnable()
    {
        manager = (PedestrianManager)target;
    }

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        PedestrianManager pedestrianManager = (PedestrianManager)target;

        EditorGUILayout.Space(5);
        EditorGUILayout.BeginVertical("box");
        DrawButtons();
        EditorGUILayout.EndVertical();

        if (GUILayout.Button("Initialize Pedestrian Manager"))
        {
            pedestrianManager.Init();
        }
        if (GUILayout.Button("Enable Pedestrian Simulation"))
        {
            pedestrianManager.EnableSim();
        }
        if (GUILayout.Button("Disable Pedestrian Simulation"))
        {
            pedestrianManager.DisableSim();
        }
    }

    private void DrawButtons()
    {
        if (GUILayout.Button("Create Waypoint"))
            CreateWaypoint();
    }

    private void CreateWaypoint()
    {
        Transform root = manager.transform;

        GameObject newWaypoint =
            new($"Waypoint-{root.childCount}", typeof(PedestrianPoint));
        newWaypoint.transform.SetParent(root);

        PedestrianPoint waypoint = newWaypoint.GetComponent<PedestrianPoint>();

        waypoint.transform.position = root.position;
        waypoint.transform.forward = root.forward;

        Selection.activeGameObject = newWaypoint;
    }
}
