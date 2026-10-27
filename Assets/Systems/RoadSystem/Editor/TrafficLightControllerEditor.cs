using RoadSystem.Options;
using RoadSystem.TrafficLightControl;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(TrafficLightController))]
public class TrafficLightControllerEditor : Editor
{
    [DrawGizmo(GizmoType.Selected | GizmoType.NonSelected | GizmoType.Pickable)]
    private static void DrawDefault(TrafficLightController trafficLightController, GizmoType _)
    {
        if (!RoadEditManager.IsTrafficLightGizmoVisible) return;

        Gizmos.color = RoadSystemOptions.Options.Common.TrafficLightControllerColor;
        Gizmos.DrawSphere(
            trafficLightController.transform.position,
            RoadSystemOptions.Options.Common.TrafficLightControllerRadius);

        Gizmos.DrawIcon(
            trafficLightController.transform.position,
            "Assets/Systems/RoadSystem/Icons/lights.png",
            true);

        // Draw the connected traffic lights
        if (trafficLightController.TrafficLightCycle != null
            && trafficLightController.TrafficLightCycle.TrafficLights != null
            && trafficLightController.TrafficLightCycle.TrafficLights.Length > 0)
        {
            foreach (var trafficLight in trafficLightController.TrafficLightCycle.TrafficLights)
            {
                if (trafficLight != null)
                {
                    Gizmos.DrawLine(
                        trafficLightController.transform.position,
                        trafficLight.transform.position);
                }
            }
        }
    }
}
