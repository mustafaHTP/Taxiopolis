using RoadSystem;
using RoadSystem.Options;
using RoadSystem.TrafficLightControl;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(TrafficLight))]
public class TrafficLightEditor : Editor
{
    private RoadPoint _selectedRoadPoint;

    [DrawGizmo(GizmoType.NonSelected | GizmoType.Selected | GizmoType.Pickable)]
    private static void DrawDefault(TrafficLight trafficLight, GizmoType gizmoType)
    {
        if (!RoadEditManager.IsTrafficLightGizmoVisible) return;

        Color trafficLightGizmosColor = Color.clear;

        switch (trafficLight.TrafficLightState)
        {
            case TrafficLightState.Green:
                trafficLightGizmosColor = Color.green;
                break;
            case TrafficLightState.Yellow:
                trafficLightGizmosColor = Color.yellow;
                break;
            case TrafficLightState.Red:
                trafficLightGizmosColor = Color.red;
                break;
        }

        if ((gizmoType & GizmoType.NonSelected) != 0)
        {
            trafficLightGizmosColor.a = 0.4f;
        }

        Gizmos.color = trafficLightGizmosColor;

        // Draw a line to the connected road point if there is one
        if (trafficLight.ConnectedRoadPoint != null)
        {
            Gizmos.DrawLine(
                trafficLight.transform.position,
                trafficLight.ConnectedRoadPoint.transform.position);
        }

        Gizmos.DrawSphere(
            trafficLight.transform.position,
            RoadSystemOptions.Options.Common.TrafficLightRadius);
        Gizmos.DrawIcon(
            trafficLight.transform.position,
            "Assets/Systems/RoadSystem/Icons/TrafficLight.png",
            true);
    }

    private void OnSceneGUI()
    {
        var trafficLight = (TrafficLight)target;

        if (RoadEditManager.RoadEditMode == RoadEditMode.TrafficLightLink)
        {
            HandleUtility.AddDefaultControl(GUIUtility.GetControlID(FocusType.Passive));
            float handleSize = HandleUtility.GetHandleSize(trafficLight.transform.position);

            Handles.color = Color.green;
            Handles.SphereHandleCap(
                0,
                trafficLight.transform.position,
                Quaternion.identity,
                handleSize * 0.3f,
                EventType.Repaint);

            Ray ray = HandleUtility.GUIPointToWorldRay(Event.current.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hitInfo))
            {
                Handles.color = RoadSystemOptions.Options.TrafficLightLinkMode.ConnectionColor;
                Handles.DrawLine(
                    trafficLight.transform.position,
                    trafficLight.ConnectedRoadPoint != null
                    ?
                    trafficLight.ConnectedRoadPoint.transform.position
                    : hitInfo.point,
                    1f);

                SceneView.RepaintAll();
            }

            RoadPoint[] roadPoints = GameObject.FindObjectsByType<RoadPoint>(
                FindObjectsInactive.Exclude,
                FindObjectsSortMode.None);

            for (int i = 0; i < roadPoints.Length; i++)
            {
                var currentRoadPoint = roadPoints[i];
                Handles.color = trafficLight.ConnectedRoadPoint == currentRoadPoint
                    ? RoadSystemOptions.Options.TrafficLightLinkMode.RoadPointSelectedButtonColor
                    : RoadSystemOptions.Options.TrafficLightLinkMode.RoadPointNonSelectedButtonColor;
                if (Handles.Button(
                    currentRoadPoint.transform.position,
                    Quaternion.identity,
                    handleSize * RoadSystemOptions.Options.TrafficLightLinkMode.RoadPointButtonSize,
                    handleSize * RoadSystemOptions.Options.TrafficLightLinkMode.RoadPointButtonSize,
                    Handles.SphereHandleCap))
                {
                    _selectedRoadPoint = currentRoadPoint;
                }
            }

            // If the traffic light is already connected to a road point
            // show a button to remove the connection
            if (trafficLight.ConnectedRoadPoint != null)
            {
                Vector3 middlePoint =
                    (trafficLight.transform.position + trafficLight.ConnectedRoadPoint.transform.position)
                    / 2;
                Vector2 guiPosition = HandleUtility.WorldToGUIPoint(middlePoint);
                Rect removeConnectionRect = new(
                    guiPosition + RoadSystemOptions.Options.TrafficLightLinkMode.RemoveConnectionButtonOffset,
                    RoadSystemOptions.Options.TrafficLightLinkMode.RemoveConnectionButtonSize);

                GUIContent removeConnectionRectContent = new(
                    "X",
                    "Remove connection between traffic light and road point");

                Handles.BeginGUI();

                if (GUI.Button(removeConnectionRect, removeConnectionRectContent))
                {
                    trafficLight.ConnectedRoadPoint = null;
                    EditorUtility.SetDirty(trafficLight);
                    _selectedRoadPoint = null;
                }

                Handles.EndGUI();
            }
            // If a road point is selected and it's not the same as the currently connected one
            else if (_selectedRoadPoint != null
                && _selectedRoadPoint != trafficLight.ConnectedRoadPoint)
            {
                trafficLight.ConnectedRoadPoint = _selectedRoadPoint;
                _selectedRoadPoint = null;
                EditorUtility.SetDirty(trafficLight);
            }
        }
    }
}
