using Framework.Services;
using Game.Common;
using Game.Systems.PassengerSystem;
using Game.UI.Common;
using UnityEngine;

namespace Game.UI.Screens.Gameplay
{
    public class FareBonusRewardPanel : UIPanel
    {
        [SerializeField] private FareBonusRewardFloatingText _fareBonusRewardFloatingTextPrefab;
        [SerializeField] private Canvas _gameplayScreenUI;
        [SerializeField] private RectTransform _container;

        [Header("Spawn")]
        [SerializeField] private Vector3 _worldOffset = new Vector3(0f, 2f, 0f);
        [SerializeField] private Vector2 _spawnSpread = new Vector2(80f, 40f); // pixels

        private CarPassengerHandler _carPassengerHandler;
        private Transform _player;
        private Camera _camera;

        public override void Init()
        {
            var player = ServiceLocator.Scene.GetService<Player>();
            _player = player.transform;
            _carPassengerHandler = player.GetComponent<CarPassengerHandler>();
            _camera = Camera.main;

            _carPassengerHandler.OnFareBonusEarned += CarPassengerHandler_OnFareBonusEarned;
        }

        public override void Dispose()
        {
            _carPassengerHandler.OnFareBonusEarned -= CarPassengerHandler_OnFareBonusEarned;
        }

        private void CarPassengerHandler_OnFareBonusEarned(PassengerJob job, int reward)
        {
            Vector3 screenPos = _camera.WorldToScreenPoint(_player.position + _worldOffset);
            if (screenPos.z < 0f) return; // behind camera

            screenPos.x += Random.Range(-_spawnSpread.x, _spawnSpread.x);
            screenPos.y += Random.Range(-_spawnSpread.y, _spawnSpread.y);
            screenPos.z = 0f;

            var floatingText = Instantiate(_fareBonusRewardFloatingTextPrefab, _container);
            floatingText.Play(reward, screenPos, _gameplayScreenUI.scaleFactor);
        }
    }
}