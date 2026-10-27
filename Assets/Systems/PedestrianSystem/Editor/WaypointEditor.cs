using Game.Systems.PedestrianSystem;
using Game.Utils;
using UnityEditor;
using UnityEngine;

[CanEditMultipleObjects]
[CustomEditor(typeof(PedestrianPoint))]
public class PedestrianPointEditor : Editor
{
    private PedestrianManager _pedestrianManager;

    [DrawGizmo(GizmoType.Selected | GizmoType.NonSelected | GizmoType.Pickable)]
    public static void DrawGizmos(PedestrianPoint waypoint, GizmoType gizmoType)
    {
        if ((gizmoType & GizmoType.Selected) != 0)
        {
            Gizmos.color = Color.yellow;
        }
        else
        {
            Gizmos.color = Color.yellow * 0.5f;
        }

        Gizmos.DrawSphere(waypoint.transform.position, 0.2f);

        Gizmos.color = Color.white;
        Vector3 leftBound = waypoint.transform.position - waypoint.transform.right * (waypoint.Width / 2);
        Vector3 rightBound = waypoint.transform.position + waypoint.transform.right * (waypoint.Width / 2);

        Gizmos.DrawLine(leftBound, rightBound);

        if (waypoint.Next != null)
        {
            Vector3 nextLeftBound =
                waypoint.Next.transform.position - waypoint.Next.transform.right * (waypoint.Next.Width / 2);

            Gizmos.color = Color.green;
            Gizmos.DrawLine(leftBound, nextLeftBound);

            // Draw arrow
            Vector3 arrowBase = waypoint.transform.position;
            Vector3 arrowTip = waypoint.Next.transform.position;

            // Draw left side of arrowhead
            // Build a rotation that points in the direction of the arrow
            Vector3 directionNormalized = (arrowTip - arrowBase).normalized;
            Quaternion rot = Quaternion.LookRotation(directionNormalized);

            // Arrowhead directions in local space
            Vector3 leftHead = rot * Quaternion.Euler(0, 20f, 0) * -Vector3.forward;
            Vector3 rightHead = rot * Quaternion.Euler(0, -20f, 0) * -Vector3.forward;

            Gizmos.color = Color.cyan;
            Gizmos.DrawLine(arrowTip, arrowTip + leftHead * 0.5f);
            Gizmos.DrawLine(arrowTip, arrowTip + rightHead * 0.5f);
            Gizmos.DrawLine(arrowBase, arrowTip);
        }

        if (waypoint.Previous != null)
        {
            Vector3 prevRightBound =
                waypoint.Previous.transform.position + waypoint.Previous.transform.right * (waypoint.Previous.Width / 2);
            Gizmos.color = Color.red;
            Gizmos.DrawLine(rightBound, prevRightBound);
        }

        if (waypoint.Branches != null && waypoint.Branches.Count > 0)
        {
            Gizmos.color = Color.blue;
            for (int i = 0; i < waypoint.Branches.Count; i++)
            {
                if (waypoint.Branches[i] == null) continue;
                Gizmos.DrawLine(
                    waypoint.transform.position,
                    waypoint.Branches[i].transform.position);
            }
        }
    }

    private void OnEnable()
    {
        // Get pedestrian manager from scene
        var pedestrianManagers = FindObjectsByType<PedestrianManager>(
            FindObjectsSortMode.None);
        if (pedestrianManagers.Length > 1)
        {
            LogX.LogError(
                "WaypointEditor: Multiple PedestrianManager instances found in scene. " +
                "Ensure only one instance exists.");

            return;
        }

        _pedestrianManager = pedestrianManagers[0];
    }

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        EditorGUILayout.Space(5);
        EditorGUILayout.BeginVertical("box");
        DrawButtons();
        EditorGUILayout.EndVertical();
    }

    private void OnSceneGUI()
    {
        // When press ctrl, align the waypoint with ground normal
        Event e = Event.current;
        if (e.control && e.shift && e.type == EventType.MouseMove)
        {
            Ray ray = HandleUtility.GUIPointToWorldRay(e.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                Undo.RecordObject(Selection.activeGameObject.transform, "Align Waypoint with Ground");
                // Do not touch forward direction, only position and up
                Selection.activeGameObject.transform.position = hit.point;
                Quaternion targetRotation =
                    Quaternion.FromToRotation(
                        Selection.activeGameObject.transform.up,
                        hit.normal) * Selection.activeGameObject.transform.rotation;
                Selection.activeGameObject.transform.rotation = targetRotation;

                e.Use();
            }
        }
    }

    private void DrawButtons()
    {
        if (GUILayout.Button("Create Branch Waypoint"))
            CreateBranchWaypoint();

        if (GUILayout.Button("Create Waypoint After"))
            CreateWaypointAfter();

        if (GUILayout.Button("Create Waypoint Before"))
            CreateWaypointBefore();

        if (GUILayout.Button("Remove Selected Waypoint"))
            RemoveSelectedWaypoint();
    }

    private void CreateBranchWaypoint()
    {
        Transform root = _pedestrianManager.transform;

        GameObject branchWaypointObject =
            new($"Waypoint-{root.childCount}", typeof(PedestrianPoint));

        branchWaypointObject.transform.SetParent(root);

        PedestrianPoint branchWaypoint = branchWaypointObject.GetComponent<PedestrianPoint>();
        PedestrianPoint branchedFrom = Selection.activeGameObject.GetComponent<PedestrianPoint>();

        branchedFrom.Branches.Add(branchWaypoint);

        branchWaypoint.transform.position = branchedFrom.transform.position;
        branchWaypoint.transform.forward = branchedFrom.transform.forward;

        Selection.activeGameObject = branchWaypointObject;
    }

    private void RemoveSelectedWaypoint()
    {
        PedestrianPoint selectedWaypoint = Selection.activeGameObject.GetComponent<PedestrianPoint>();

        if (selectedWaypoint.Previous != null)
            selectedWaypoint.Previous.Next = selectedWaypoint.Next;

        if (selectedWaypoint.Next != null)
            selectedWaypoint.Next.Previous = selectedWaypoint.Previous;

        DestroyImmediate(selectedWaypoint.gameObject);
    }

    private void CreateWaypointBefore()
    {
        Transform root = _pedestrianManager.transform;

        GameObject newWaypoint =
            new($"Waypoint-{root.childCount}", typeof(PedestrianPoint));
        newWaypoint.transform.SetParent(root);

        PedestrianPoint waypoint = newWaypoint.GetComponent<PedestrianPoint>();
        PedestrianPoint selected = Selection.activeGameObject.GetComponent<PedestrianPoint>();

        if (selected.Previous != null)
        {
            waypoint.Previous = selected.Previous;
            waypoint.Previous.Next = waypoint;
        }

        waypoint.Next = selected;
        selected.Previous = waypoint;

        waypoint.transform.position = selected.transform.position;
        waypoint.transform.forward = selected.transform.forward;

        waypoint.transform.SetSiblingIndex(selected.transform.GetSiblingIndex() + 1);

        Selection.activeGameObject = newWaypoint;
    }

    private void CreateWaypointAfter()
    {
        Transform root = _pedestrianManager.transform;

        GameObject newWaypoint =
            new($"Waypoint-{root.childCount}", typeof(PedestrianPoint));
        newWaypoint.transform.SetParent(root);

        PedestrianPoint waypoint = newWaypoint.GetComponent<PedestrianPoint>();
        PedestrianPoint selected = Selection.activeGameObject.GetComponent<PedestrianPoint>();

        if (selected.Next != null)
        {
            waypoint.Next = selected.Next;
            waypoint.Next.Previous = waypoint;
        }

        waypoint.Previous = selected;
        selected.Next = waypoint;

        waypoint.transform.position = selected.transform.position;
        waypoint.transform.forward = selected.transform.forward;

        waypoint.transform.SetSiblingIndex(selected.transform.GetSiblingIndex() + 1);

        Selection.activeGameObject = newWaypoint;
    }
}
