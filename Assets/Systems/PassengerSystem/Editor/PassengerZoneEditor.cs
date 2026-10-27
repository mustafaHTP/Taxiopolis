using Game.Systems.PassengerSystem;
using UnityEditor;
using UnityEngine;

namespace Game.PassengerSystem
{
    [CustomEditor(typeof(PassengerZone))]
    public class PassengerZoneEditor : Editor
    {
        private SerializedProperty _idProperty;

        private void OnEnable()
        {
            _idProperty = serializedObject.FindProperty("_id");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            base.OnInspectorGUI();

            GUILayout.Space(10);
            GUILayout.BeginHorizontal();

            if (GUILayout.Button("Generate ID"))
            {
                _idProperty.stringValue = System.Guid.NewGuid().ToString();
            }
            GUILayout.EndHorizontal();

            serializedObject.ApplyModifiedProperties();
        }
    }
}
