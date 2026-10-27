using System;
using Framework.Services;
using Game.Common;
using Game.Systems.CollectibleSystem;
using Game.Systems.SoundSystem;
using Game.Utils;
using UnityEngine;

namespace Game.Managers
{
    public class GameTimeManager : MonoBehaviour
    {
        [SerializeField] private SoundData _beepSound;

        public event Action OnGameTimeEnd;
        public event Action<int> OnSecondChanged;

        private float _gameTime;
        private float _gameElapsedTime;
        private bool _isGameTimeRunning;
        private int _hurryUpTime;
        private int _lastSecond = int.MaxValue;

        public float GameTime => _gameTime;
        public float GameElapsedTime => _gameElapsedTime;
        public int RemainingSeconds => Mathf.CeilToInt(_gameTime);
        public bool IsHurryUp => RemainingSeconds <= _hurryUpTime;

        public void Init()
        {
            ServiceLocator
                .Scene
                .GetService<Player>()
                .GetComponent<CarCollectibleHandler>()
                .OnCollected += data =>
                {
                    if (data.Type == CollectibleType.TimeBonus)
                    {
                        AddGameTime(data.Amount);
                    }
                };

            _hurryUpTime = Mathf.CeilToInt(ServiceLocator.Global.GetService<GameManager>().GameConfig.HurryUpTime);
        }

        public void AddGameTime(float timeToAdd)
        {
            if (!_isGameTimeRunning) return;

            _gameTime += timeToAdd;
            // bonus may change the displayed second immediately
            NotifySecondChange();
        }

        public void StartGameTime()
        {
            if (_isGameTimeRunning) return;

            _gameTime = ServiceLocator.Global.GetService<GameManager>().GameConfig.StartTimeInSeconds;
            _gameElapsedTime = 0f;
            _isGameTimeRunning = true;

            _lastSecond = RemainingSeconds;
            OnSecondChanged?.Invoke(_lastSecond);
        }

        private void Update()
        {
            if (!_isGameTimeRunning) return;

            _gameTime -= Time.deltaTime;
            _gameElapsedTime += Time.deltaTime;

            if (_gameTime <= 0f)
            {
                _gameTime = 0f;
                _isGameTimeRunning = false;
                OnSecondChanged?.Invoke(0);
                LogX.LogInfo("Game Timer has reached zero.");
                OnGameTimeEnd?.Invoke();
                return;
            }

            NotifySecondChange();
        }

        private void NotifySecondChange()
        {
            int current = RemainingSeconds;
            if (current == _lastSecond) return;

            _lastSecond = current;
            OnSecondChanged?.Invoke(current);

            if (current <= _hurryUpTime)
            {
                ServiceLocator.Global.GetService<SoundManager>().PlaySound(_beepSound);
            }
        }

    }
}
