using Framework.CustomEventBus;
using Framework.Services;
using Game.Misc;
using Game.Systems.CameraSystem;
using Game.Systems.SoundSystem;
using Game.UI;
using Game.UI.Screens.PauseMenu;
using Game.Utils;
using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Game.Managers
{
    /// <summary>
    /// Provides centralized management of game state, logic, and coordination within the Unity scene.
    /// </summary>
    /// <remarks>Derive from this class to implement game-specific management functionality, such as handling
    /// game flow, player progress, or global events. Attach the GameManager to a GameObject in the scene to enable its
    /// behavior through Unity's component system.</remarks>
    public class GameManager : MonoBehaviour, IGlobalService
    {
        [SerializeField] private GameConfig _gameConfig;

        private bool _isPaused = false;
        private Coroutine _sceneLoadCoroutine;

        public GameConfig GameConfig => _gameConfig;

        public void Init()
        {
            if (_gameConfig == null)
            {
                LogX.LogError("Game config is not set in GameManager");

                return;
            }

            SceneManager.sceneLoaded += SceneManager_SceneLoaded;
        }

        public void AfterInit()
        {
            ServiceLocator.Global.GetService<CustomSceneManager>().OnBeforeSceneLoad += CustomSceneManager_OnBeforeSceneLoad;
            ServiceLocator.Global.GetService<InputManager>().GameInput.Game.Pause.performed += GameInput_Pause_performed;
            ServiceLocator.Global.GetService<InputManager>().GameInput.Game.ChangeTrack.performed += GameInput_ChangeTrack_performed;
        }

        public void Dispose()
        {
            ServiceLocator.Global.GetService<CustomSceneManager>().OnBeforeSceneLoad -= CustomSceneManager_OnBeforeSceneLoad;
            SceneManager.sceneLoaded -= SceneManager_SceneLoaded;
            ServiceLocator.Global.GetService<InputManager>().GameInput.Game.Pause.performed -= GameInput_Pause_performed;
            ServiceLocator.Global.GetService<InputManager>().GameInput.Game.ChangeTrack.performed -= GameInput_ChangeTrack_performed;
        }

        public void LoadMainMenuSceneAsync()
        {
            LoadScene(CustomSceneManager.MainMenuSceneName);
        }

        public void LoadGameSceneAsync()
        {
            LoadScene(CustomSceneManager.GameSceneName);
        }

        public void LoadSummarySceneAsync()
        {
            LoadScene(CustomSceneManager.SummarySceneName);
        }

        private void LoadScene(string sceneName)
        {
            if (_sceneLoadCoroutine != null)
            {
                StopCoroutine(_sceneLoadCoroutine);
                _sceneLoadCoroutine = null;
            }

            _sceneLoadCoroutine =
                StartCoroutine(ServiceLocator.Global.GetService<CustomSceneManager>().LoadSceneAsync(sceneName));
        }

        public void ResumeGame()
        {
            _isPaused = false;

            DoResumeLogic();
        }

        public void QuitGame()
        {
            Application.Quit();
        }

        private void DoPauseLogic()
        {
            ServiceLocator.Global.GetService<InputManager>().DisableAllInput();
            ServiceLocator.Global.GetService<InputManager>().EnableUIInput();
            ServiceLocator.Global.GetService<TimeManager>().SetPause(_isPaused);
            ServiceLocator.Global.GetService<UIManager>()
                .PushUI(ServiceLocator.Scene.GetService<UIPauseMenuScreen>());
            ServiceLocator.Global.GetService<SoundMixerManager>().TransitionToPause();
        }

        private void DoResumeLogic()
        {
            ServiceLocator.Global.GetService<TimeManager>().SetPause(_isPaused);
            ServiceLocator.Global.GetService<UIManager>()
                        .PopUI();
            ServiceLocator.Global.GetService<InputManager>().DisableAllInput();
            ServiceLocator.Global.GetService<InputManager>().EnableGameInput();
            ServiceLocator.Global.GetService<InputManager>().EnableCarInput();
            ServiceLocator.Global.GetService<SoundMixerManager>().TransitionToGameplay();
        }


        #region Handling Events

        private void SceneManager_SceneLoaded(Scene scene, LoadSceneMode _)
        {
            //* only does the following if there is no scene controller
            //* might be useful for other non-special scenes like test scene
            //* right now there is no use for me
            //ServiceLocator.Global.GetService<InputManager>().EnableGameInput();
            //ServiceLocator.Global.GetService<CameraManager>().ActivateHighestPriorityCamera();
            LogX.LogInfo($"Scene loaded: {scene.name}. Executed scene-specific logic.");
        }

        private void CustomSceneManager_OnBeforeSceneLoad()
        {
            // do cleanups here
            ServiceLocator.Global.GetService<InputManager>().DisableAllInput();
            ServiceLocator.Global.GetService<CameraManager>().ClearCameras();
            _isPaused = false;
            ServiceLocator.Scene.ClearServices();
            ServiceLocator.Global.GetService<TimeManager>().ResetTimeScale();
            ServiceLocator.Global.GetService<MusicManager>().ClearPlaylist();
            ServiceLocator.Global.GetService<SoundMixerManager>().ResetParams();
        }

        private void GameInput_ChangeTrack_performed(UnityEngine.InputSystem.InputAction.CallbackContext _)
        {
            if (_isPaused)
            {
                LogX.LogInfo("Change Track input detected, but game is paused. Ignoring input.");

                return;
            }
            ServiceLocator.Global.GetService<MusicManager>().NextTrackOnPlaylist();
        }

        private void GameInput_Pause_performed(UnityEngine.InputSystem.InputAction.CallbackContext _)
        {
            LogX.LogInfo("Pause input detected. Toggling pause state...");

            _isPaused = !_isPaused;

            if (_isPaused)
            {
                DoPauseLogic();

                LogX.LogInfo("Game paused.");
            }
            else
            {
                DoResumeLogic();

                LogX.LogInfo("Game resumed.");
            }
        }
        #endregion
    }

#if UNITY_EDITOR
    [UnityEditor.CustomEditor(typeof(GameManager))]
    public class GameManagerEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            GameManager gameManager = (GameManager)target;

            EditorGUILayout.Space(10);

            if (GUILayout.Button("Initialize"))
            {
                gameManager.Init();
            }
        }
    }
#endif
}
