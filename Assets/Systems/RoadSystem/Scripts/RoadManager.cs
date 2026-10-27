using UnityEditor;
using UnityEngine;

namespace RoadSystem
{
    public class RoadManager : MonoBehaviour
    {
        [SerializeField] private RoadNetwork _roadNetwork;

        private bool _isInitialized = false;

        public void Init()
        {
            if (_isInitialized)
            {
                Debug.LogWarning("RoadManager is already initialized!");

                return;
            }

            _roadNetwork.ConstructRoadGraph();

            _isInitialized = true;
        }
    }

#if UNITY_EDITOR
    [CustomEditor(typeof(RoadManager))]
    public class RoadManagerEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();
            RoadManager roadManager = (RoadManager)target;
            if (GUILayout.Button("Initialize Road Manager"))
            {
                roadManager.Init();
            }
        }
    }
#endif
}
