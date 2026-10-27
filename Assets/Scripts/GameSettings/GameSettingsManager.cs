using Framework.Services;
using Game.Settings;
using UnityEditor;
using UnityEngine;

namespace Game.Settings
{
    public class GameSettingsManager : MonoBehaviour, IGlobalService
    {
        [SerializeField] private ScreenResolutionContainer _screenResolutionContainer;

        public GameSettings Settings { get; private set; }

        public void AfterInit()
        {
            // noop
        }

        public void Dispose()
        {
            // noop
            Settings = null;
        }

        public void Init()
        {
            Settings = new(_screenResolutionContainer);
            Settings.ApplyAllSettings();
        }
    }

#if UNITY_EDITOR
    [CustomEditor(typeof(GameSettingsManager))]
    public class GameSettingsManagerEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            if (GUILayout.Button("Reset Settings"))
            {
                if (EditorUtility.DisplayDialog(
                    "Reset Game Settings",
                    "Are you sure you want to reset all game settings to their default values? This action cannot be undone.",
                    "Yes, Reset",
                    "Cancel"))
                {
                    PlayerPrefs.DeleteAll();
                }
            }
        }
    }
#endif
}