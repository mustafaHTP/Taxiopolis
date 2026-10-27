using Game.PassengerSystem;
using Game.Systems.PassengerSystem.Passengers;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(Passenger))]
public class PassengerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        var passenger = target as Passenger;

        // make text green
        var style = new GUIStyle(GUI.skin.label);
        style.normal.textColor = Color.green;
        GUILayout.Label(
            "Current State: " + (passenger.CurrentState != null ? passenger.CurrentState.GetType().Name : "None"),
            style);
    }
}
