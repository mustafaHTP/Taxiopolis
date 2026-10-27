using Game.PassengerSystem;
using Game.Systems.PassengerSystem;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(PassengerDestinationPoint))]
public class PassengerDestinationPointEditor : Editor
{
    [DrawGizmo(GizmoType.Selected | GizmoType.NonSelected | GizmoType.Pickable)]
    private static void DrawDefaultGizmos(PassengerDestinationPoint passengerPoint, GizmoType _)
    {
        Color color = passengerPoint.HasZone ? passengerPoint.Zone.DebugZoneColor : Color.red;
        Gizmos.color = color;
        Gizmos.DrawSphere(passengerPoint.transform.position, 2f);
    }
}
