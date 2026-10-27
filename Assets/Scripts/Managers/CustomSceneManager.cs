using Framework.Services;
using Game.Systems.SoundSystem;
using Game.UI;
using System;
using System.Collections;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Game.Managers
{
    public class CustomSceneManager : MonoBehaviour, IGlobalService
    {
        public event Action OnBeforeSceneLoad;

        public const string MainMenuSceneName = "Game_MainMenu";
        public const string GameSceneName = "Game_City";
        public const string SummarySceneName = "Game_Summary";

        public void AfterInit()
        {
            // noop
        }

        public void Dispose()
        {
            // noop
        }

        public void Init()
        {
            // noop
        }

        public IEnumerator LoadSceneAsync(string sceneName)
        {
            // Clear UI Stack to ensure no UI elements from the previous scene remain
            ServiceLocator.Global.GetService<UIManager>().ClearUIStack();

            yield return ServiceLocator.Global.GetService<UIManager>().StartFadeIn();

            OnBeforeSceneLoad?.Invoke();

            ServiceLocator.Global.GetService<UIManager>().ShowLoadingScreen();

            yield return null;

            var scene = SceneManager.LoadSceneAsync(sceneName);
            scene.allowSceneActivation = false;

            while (scene.progress < 0.9f)
            {
                yield return null;
            }

            scene.allowSceneActivation = true;

            while (!scene.isDone)
            {
                yield return null;
            }

            ServiceLocator.Global.GetService<UIManager>().HideLoadingScreen();

            yield return ServiceLocator.Global.GetService<UIManager>().StartFadeOut();
        }

        public void QuitGame()
        {
            Application.Quit();
        }
    }

#if UNITY_EDITOR
    [CustomEditor(typeof(CustomSceneManager))]
    public class CustomSceneManagerEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            var customSceneManager = (CustomSceneManager)target;

            EditorGUILayout.Space(10);
            EditorGUILayout.LabelField("Scene Shortcuts", EditorStyles.boldLabel);

            // Buttons only work in Play mode because they start coroutines on the runtime component
            GUI.enabled = Application.isPlaying;
            if (GUILayout.Button("Load Main Menu Scene"))
            {
                customSceneManager.StartCoroutine(customSceneManager.LoadSceneAsync(CustomSceneManager.MainMenuSceneName));
            }

            if (GUILayout.Button("Load Game Scene"))
            {
                customSceneManager.StartCoroutine(customSceneManager.LoadSceneAsync(CustomSceneManager.GameSceneName));
            }

            if (GUILayout.Button("Load Summary Scene"))
            {
                customSceneManager.StartCoroutine(customSceneManager.LoadSceneAsync(CustomSceneManager.SummarySceneName));
            }
            GUI.enabled = true;

            if (!Application.isPlaying)
            {
                EditorGUILayout.HelpBox("Enter Play Mode to use these buttons.", MessageType.Info);
            }
        }
    }
#endif
}
