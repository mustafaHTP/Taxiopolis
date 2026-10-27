using Framework.Services;
using Game.Common;
using Game.Managers;
using Game.Systems.CollectibleSystem;
using Game.Systems.PassengerSystem;
using Game.Systems.PedestrianSystem;
using Game.Systems.TrafficSystem;
using RoadSystem;
using UnityEngine;

namespace Game.SceneServiceInstallers
{
    public class GameSceneServiceInstaller : SceneServiceInstaller
    {
        [SerializeField] private CollectibleManager _collectibleManager;
        [SerializeField] private GameTimeManager _gameTimeManager;
        [SerializeField] private PassengerManager _passengerManager;
        [SerializeField] private PedestrianManager _pedestrianManager;
        [SerializeField] private Player _player;
        [SerializeField] private RoadNetwork _roadNetwork;
        [SerializeField] private RoadManager _roadManager;
        [SerializeField] private SceneUIInstaller _sceneUIInstaller;
        [SerializeField] private TrafficManager _trafficManager;

        protected override void RegisterDependencies()
        {
            ServiceLocator.Scene.RegisterService(_collectibleManager);
            ServiceLocator.Scene.RegisterService(_gameTimeManager);
            ServiceLocator.Scene.RegisterService(_passengerManager);
            ServiceLocator.Scene.RegisterService(_pedestrianManager);
            ServiceLocator.Scene.RegisterService(_player);
            ServiceLocator.Scene.RegisterService(_roadNetwork);
            ServiceLocator.Scene.RegisterService(_roadManager);
            ServiceLocator.Scene.RegisterService(_sceneUIInstaller);
            ServiceLocator.Scene.RegisterService(_trafficManager);
        }

    }
}
