using Framework.Services;
using Game.Car;
using Game.Common;
using Game.Managers;
using Game.Misc;
using Game.SceneServiceInstallers;
using Game.Stats;
using Game.Systems.CameraSystem;
using Game.Systems.CollectibleSystem;
using Game.Systems.PassengerSystem;
using Game.Systems.PedestrianSystem;
using Game.Systems.SoundSystem;
using Game.Systems.TrafficSystem;
using Game.UI;
using Game.UI.Screens.GameOver;
using Game.UI.Screens.Gameplay;
using Game.Utils;
using RoadSystem;
using System.Collections;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.Playables;

namespace Game.SceneControllers
{
    public class GameSceneController : BaseSceneController
    {
        [Header("References")]
        [SerializeField] private PlayableDirector _gameIntroDirector;
        [SerializeField] private SoundData _gameOverSound;
        [SerializeField] private SoundData[] _gameplayMusics;

        private CarPassengerHandler _carPassengerHandler;
        private CarWaterDetector _carWaterDetector;

        private Coroutine _gameStartSequenceCoroutine;
        private Coroutine _gameOverSequenceCoroutine;

        public override void Init()
        {
            _carPassengerHandler = ServiceLocator
                .Scene
                .GetService<Player>()
                .GetComponent<CarPassengerHandler>();

            _carWaterDetector = ServiceLocator
                .Scene
                .GetService<Player>()
                .GetComponent<CarWaterDetector>();

            _carPassengerHandler.OnPassengerPickedUp += CarPassengerHandler_OnPassengerPickedUp;
            _carPassengerHandler.OnPassengerDroppedOff += CarPassengerHandler_OnPassengerDroppedOff;
            _carWaterDetector.OnWaterDetected += CarWaterDetector_OnWaterDetected;
            ServiceLocator.Scene.GetService<GameTimeManager>().OnGameTimeEnd += GameTimeManager_OnGameTimeEnd;
            _gameIntroDirector.stopped += PlayableDirector_Stopped;

            // !! Order of initialization matters !!
            // because most of managers here depends road manager
            ServiceLocator.Scene.GetService<GameTimeManager>().Init();
            ServiceLocator.Scene.GetService<RoadManager>().Init();
            ServiceLocator.Scene.GetService<TrafficManager>().Init();
            ServiceLocator.Scene.GetService<PassengerManager>().Init();
            ServiceLocator.Scene.GetService<CollectibleManager>().Init();
            ServiceLocator.Scene.GetService<PedestrianManager>().Init();
            ServiceLocator.Global.GetService<CameraManager>().RegisterCameras();
        }

        public override void Dispose()
        {
            _carPassengerHandler.OnPassengerPickedUp -= CarPassengerHandler_OnPassengerPickedUp;
            _carPassengerHandler.OnPassengerDroppedOff -= CarPassengerHandler_OnPassengerDroppedOff;
            _carWaterDetector.OnWaterDetected -= CarWaterDetector_OnWaterDetected;
        }

        public override void StartScene()
        {
            ServiceLocator.Global.GetService<InputManager>().DisableAllInput();
            ServiceLocator.Scene.GetService<SceneUIInstaller>().Install();
            LogX.LogDebug("[Intro] Before Play");
            // Game intro cutscene (timeline) stutter on smart update
            // So i changed to late update to remove stutters
            Camera.main.GetComponent<CinemachineBrain>().UpdateMethod = CinemachineBrain.UpdateMethods.LateUpdate;
            _gameIntroDirector.Play();

            LogX.LogDebug(
        $"[Intro] After Play. State: {_gameIntroDirector.state}, " +
                $"Time: {_gameIntroDirector.time}");
        }

        private void CarWaterDetector_OnWaterDetected()
        {
            ServiceLocator
                .Scene
                .GetService<Player>()
                .GetComponent<CarRecoveryHandler>().Recover(false);
        }

        private void PlayableDirector_Stopped(PlayableDirector director)
        {
            if (_gameStartSequenceCoroutine != null)
            {
                StopCoroutine(_gameStartSequenceCoroutine);
            }

            _gameStartSequenceCoroutine = StartCoroutine(DoGameStartSequence());
        }

        private IEnumerator DoGameStartSequence()
        {
            // After cutscene restore the update method
            Camera.main.GetComponent<CinemachineBrain>().UpdateMethod = CinemachineBrain.UpdateMethods.SmartUpdate;
            CameraManager cameraManager = ServiceLocator.Global.GetService<CameraManager>();

            // Activate the default cam for player
            //? finding camera by name is error-prone find diff. way
            CameraIdentity defaultCamera = cameraManager.FindCameraByName("ChaseCamera");
            if (defaultCamera == null)
            {
                LogX.LogError("Chase camera not registered in camera manager");

                yield return null;
            }
            cameraManager.ActivateCamera(defaultCamera);

            ServiceLocator.Global.GetService<UIManager>()
                .PushUI(ServiceLocator.Scene.GetService<UIGameplayScreen>());
            ServiceLocator.Global.GetService<GameStatsManager>().CreateGameStats();

            ServiceLocator.Scene.GetService<TrafficManager>().EnableSim();
            ServiceLocator.Scene.GetService<PassengerManager>().EnableSim();
            ServiceLocator.Scene.GetService<PedestrianManager>().EnableSim();
            ServiceLocator.Scene.GetService<CollectibleManager>().SpawnCollectibles();
            ServiceLocator.Scene.GetService<GameTimeManager>().StartGameTime();

            ServiceLocator.Global.GetService<InputManager>().EnableGameInput();
            ServiceLocator.Global.GetService<InputManager>().EnableCarInput();
            ServiceLocator
                .Global.GetService<MusicManager>()
                .AddSoundsRandomToPlaylist(_gameplayMusics);
            ServiceLocator
                .Global.GetService<MusicManager>()
                .StartPlaylist();
        }
        private IEnumerator DoGameOverSequence()
        {
            LogX.LogInfo("Game Time Ended. Handling end of game logic...");

            ServiceLocator.Global.GetService<InputManager>().DisableAllInput();
            ServiceLocator.Global.GetService<MusicManager>().ClearPlaylist();

            ServiceLocator.Global.GetService<UIManager>().ClearUIStack();
            UIGameOverScreen uiGameOverScreen = ServiceLocator.Scene.GetService<UIGameOverScreen>();
            ServiceLocator.Global.GetService<UIManager>().PushUI(uiGameOverScreen);


            ServiceLocator.Global.GetService<SoundManager>().PlaySound(_gameOverSound);
            ServiceLocator.Global.GetService<GameStatsManager>().FinalizeStats();

            GameConfig gameConfig = ServiceLocator.Global.GetService<GameManager>().GameConfig;

            yield return new WaitForSecondsRealtime(gameConfig.GameOverSequenceDuration);

            ServiceLocator
                .Global.GetService<GameManager>()
                .LoadSummarySceneAsync();
        }

        #region Handling Events

        private void CarPassengerHandler_OnPassengerPickedUp(PassengerJob job)
        {
            ServiceLocator.Scene.GetService<PassengerManager>().DisableSim();
        }

        private void CarPassengerHandler_OnPassengerDroppedOff(PassengerJob job)
        {
            ServiceLocator.Scene.GetService<PassengerManager>().EnableSim();
        }

        private void GameTimeManager_OnGameTimeEnd()
        {
            GameConfig gameConfig = ServiceLocator.Global.GetService<GameManager>().GameConfig;
            if (!gameConfig.EndGameOnGameTimeEnd) return;

            if (_gameOverSequenceCoroutine != null)
            {
                StopCoroutine(_gameOverSequenceCoroutine);
                _gameOverSequenceCoroutine = null;
            }

            _gameOverSequenceCoroutine = StartCoroutine(DoGameOverSequence());
        }
        #endregion
    }
}