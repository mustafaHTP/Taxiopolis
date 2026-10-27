using RoadSystem;
using RoadSystem.Options;
using RoadSystem.Utils;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(RoadPoint)), CanEditMultipleObjects]
public class RoadPointEditor : Editor
{
    private static void DrawRoadPointLabel(RoadPoint roadPoint)
    {
        Vector3 labelOffset = roadPoint.Position
            + Vector3.up * RoadSystemOptions.Options.Common.LabelOffset.y
            + Vector3.right * RoadSystemOptions.Options.Common.LabelOffset.x
            + Vector3.forward * RoadSystemOptions.Options.Common.LabelOffset.z;

        var style = new GUIStyle(GUI.skin.label)
        {
            fontSize = RoadSystemOptions.Options.Common.LabelFontSize,
            normal = new GUIStyleState { textColor = Color.green },
            alignment = TextAnchor.MiddleCenter,
            padding = new RectOffset(
                RoadSystemOptions.Options.Common.LabelPaddingHorizontal.x,
                RoadSystemOptions.Options.Common.LabelPaddingHorizontal.y,
                RoadSystemOptions.Options.Common.LabelPaddingVertical.x,
                RoadSystemOptions.Options.Common.LabelPaddingVertical.y),
        };

        style.normal.textColor = Color.green;

        // Draw background box for label
        Handles.BeginGUI();

        Vector2 labelGUIPoint = HandleUtility.WorldToGUIPoint(labelOffset);
        string labelText = $"Index : {roadPoint.GetIndex()}";
        Vector2 size = style.CalcSize(new GUIContent(labelText)) * RoadSystemOptions.Options.Common.LabelSize;
        Rect rect = new(labelGUIPoint.x, labelGUIPoint.y, size.x, size.y);

        var previousColor = GUI.color;
        GUI.color = RoadSystemOptions.Options.Common.LabelBackgroundColor;
        GUI.DrawTexture(rect, Texture2D.whiteTexture);

        GUI.color = previousColor;
        GUI.Label(rect, labelText, style);

        Handles.EndGUI();
    }

    [DrawGizmo(GizmoType.NonSelected | GizmoType.Selected | GizmoType.Pickable)]
    private static void DrawDefault(RoadPoint roadPoint, GizmoType _)
    {
        if (!RoadEditManager.IsRoadGizmoVisible) return;

        Gizmos.color = RoadSystemOptions.Options.Common.RoadPointColor;
        Gizmos.DrawSphere(roadPoint.transform.position, RoadSystemOptions.Options.Common.RoadPointRadius);
    }

    [DrawGizmo(GizmoType.Selected)]
    private static void DrawSelectedGizmos(RoadPoint roadPoint, GizmoType _)
    {
        if (!RoadEditManager.IsRoadGizmoVisible) return;

        DrawRoadPointLabel(roadPoint);

        Gizmos.color = RoadSystemOptions.Options.RoadLinkMode.SelectedRoadPointConnectionColor;
        DrawConnections(roadPoint);
    }

    private static void DrawConnections(RoadPoint roadPoint)
    {
        if (roadPoint.ConnectionsCount == 0) return;

        for (int i = 0; i < roadPoint.Connections.Count; i++)
        {
            var connectedPoint = roadPoint.Connections[i];
            var middlePoint = (roadPoint.Position + connectedPoint.Position) / 2;
            var directionToConnectedPoint = (connectedPoint.Position - roadPoint.Position).normalized;

            Gizmos.DrawLine(roadPoint.Position, connectedPoint.Position);

            RoadDraw.DrawGizmosArrow(
                middlePoint,
                directionToConnectedPoint,
               0.5f,
                45f,
                2f);
        }
    }

    private void OnSceneGUI()
    {
        var roadPoint = (RoadPoint)target;

        // Ensure road point is selected
        if (Selection.activeTransform == roadPoint.transform)
        {
            RoadSystemUtils.EvaluateRotations(roadPoint.Parent);

            if (Event.current.control && Event.current.alt)
            {
                Ray ray = HandleUtility.GUIPointToWorldRay(Event.current.mousePosition);
                if (Physics.Raycast(ray, out RaycastHit hitInfo))
                {
                    Handles.color = Color.green;
                    Handles.DrawWireDisc(hitInfo.point, hitInfo.normal, 2f);

                    SceneView.RepaintAll();

                    roadPoint.transform.position = hitInfo.point;
                }
            }
        }
    }
}
