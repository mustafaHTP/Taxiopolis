using Game.PassengerSystem;
using Game.Systems.PassengerSystem;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(PassengerWaitingPoint))]
public class PassengerWaitingPointEditor : Editor
{
    [DrawGizmo(GizmoType.Pickable | GizmoType.Selected | GizmoType.NonSelected)]
    private static void DrawDefaultGizmos(PassengerWaitingPoint passengerPoint, GizmoType _)
    {
        Color color = passengerPoint.HasZone ? passengerPoint.Zone.DebugZoneColor : Color.cyan;

        Gizmos.color = color;
        Gizmos.DrawSphere(passengerPoint.transform.position, 0.5f);
    }
}
