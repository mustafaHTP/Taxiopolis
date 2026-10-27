using Game.PassengerSystem;
using Game.Systems.PassengerSystem;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(PassengerManager))]
public class PassengerManagerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        var passengerManager = (PassengerManager)target;

        base.OnInspectorGUI();

        EditorGUILayout.Space(10);

        if (GUILayout.Button("Initialize"))
        {
            passengerManager.Init();
        }

        EditorGUILayout.Space(5);

        if (GUILayout.Button("Enable Passenger Sim"))
        {
            passengerManager.EnableSim();
        }

        EditorGUILayout.Space(5);

        if (GUILayout.Button("Disable Passenger Sim"))
        {
            passengerManager.DisableSim();
        }

        EditorGUILayout.Space();

        EditorGUILayout.HelpBox(
            "Hold Control key and Left Click in the Scene view to add Passenger Waiting Points. " +
            "Hold Control and Alt key and Left Click in the Scene view to add Passenger Destination Points",
            MessageType.Info);
    }

    private void OnSceneGUI()
    {
        var e = Event.current;
        var passengerManager = (PassengerManager)target;

        // Spawn Passenger Destination Point when Control + Alt keys are pressed and Left Click
        if (e.control && e.alt && e.button == 0 && e.type == EventType.MouseDown)
        {
            HandleUtility.AddDefaultControl(GUIUtility.GetControlID(FocusType.Passive));
            var ray = HandleUtility.GUIPointToWorldRay(e.mousePosition);
            bool isHit = Physics.Raycast(ray, out RaycastHit hitInfo);
            if (isHit)
            {
                Quaternion rotation = Quaternion.FromToRotation(Vector3.up, hitInfo.normal);
                var passengerDestinationPointInstance = PrefabUtility.InstantiatePrefab(
                    passengerManager.PassengerDestinationPointPrefab,
                    passengerManager.transform) as GameObject;
                passengerDestinationPointInstance.transform.SetParent(passengerManager.transform);
                passengerDestinationPointInstance.transform.SetPositionAndRotation(
                    hitInfo.point,
                    rotation);
            }
        }
        // Spawn Passenger Waiting Point when Control key is pressed and Left Click
        else if (e.control && e.button == 0 && e.type == EventType.MouseDown)
        {
            HandleUtility.AddDefaultControl(GUIUtility.GetControlID(FocusType.Passive));
            var ray = HandleUtility.GUIPointToWorldRay(e.mousePosition);
            bool isHit = Physics.Raycast(ray, out RaycastHit hitInfo);

            if (isHit)
            {
                Quaternion rotation = Quaternion.FromToRotation(Vector3.up, hitInfo.normal);
                var passengerWaitingPointInstance =
                    PrefabUtility.InstantiatePrefab(
                        passengerManager.PassengerWaitingPointPrefab,
                        passengerManager.transform) as GameObject;
                passengerWaitingPointInstance.transform.SetPositionAndRotation(
                    hitInfo.point,
                    rotation);
            }

            e.Use();
        }


        // Passenger Destination Point Preview
        if (e.control && e.alt)
        {
            HandleUtility.AddDefaultControl(GUIUtility.GetControlID(FocusType.Passive));
            var ray = HandleUtility.GUIPointToWorldRay(e.mousePosition);
            bool isHit = Physics.Raycast(ray, out RaycastHit hitInfo);
            if (isHit)
            {
                Quaternion rotation = Quaternion.LookRotation(hitInfo.normal);

                Handles.color = Color.red;
                Handles.SphereHandleCap(
                    0,
                    hitInfo.point,
                    rotation,
                    1f,
                    EventType.Repaint);
                Handles.ArrowHandleCap(
                    0,
                    hitInfo.point,
                    rotation,
                    3f,
                    EventType.Repaint);

                SceneView.RepaintAll();
            }
        }
        // Passenger Waiting Point Preview
        else if (e.control)
        {
            HandleUtility.AddDefaultControl(GUIUtility.GetControlID(FocusType.Passive));
            var ray = HandleUtility.GUIPointToWorldRay(e.mousePosition);
            bool isHit = Physics.Raycast(ray, out RaycastHit hitInfo);

            if (isHit)
            {
                Quaternion rotation = Quaternion.LookRotation(hitInfo.normal);

                Handles.color = Color.green;
                Handles.SphereHandleCap(
                    0,
                    hitInfo.point,
                    rotation,
                    1f,
                    EventType.Repaint);
                Handles.ArrowHandleCap(
                    0,
                    hitInfo.point,
                    rotation,
                    3f,
                    EventType.Repaint);

                SceneView.RepaintAll();
            }
        }
    }
}
