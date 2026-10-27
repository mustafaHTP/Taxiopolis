using Framework.Services;
using Game.Car;
using Game.Common;
using Game.Managers;
using Game.Systems.PassengerSystem;
using UnityEngine;

namespace Game.Stats
{
    public class GameStats
    {
        // Stats
        private int _moneyGained;
        private int _transportedPassengersCount;
        private int _gameElapsedTime;

        // references
        private CarMoneyHandler _carMoneyHandler;
        private CarPassengerHandler _carPassengerHandler;

        public int MoneyGained => _moneyGained;
        public int TransportedPassengersCount => _transportedPassengersCount;
        public int GameElapsedTime => _gameElapsedTime;

        public void SetGameTime(float gameTime)
        {
            _gameElapsedTime = Mathf.RoundToInt(gameTime);
        }

        public void RegisterEvents()
        {
            Player player = ServiceLocator.Scene.GetService<Player>();
            _carPassengerHandler = player.GetComponent<CarPassengerHandler>();
            _carMoneyHandler = player.GetComponent<CarMoneyHandler>();

            _carPassengerHandler.OnPassengerJobFinished += CarPassengerHandler_OnPassengerJobFinished;
            _carMoneyHandler.OnMoneyGained += CarMoneyHandler_OnMoneyGained;
        }

        public void FinalizeStats()
        {
            float elapsedTime = ServiceLocator.Scene.GetService<GameTimeManager>().GameElapsedTime;
            _gameElapsedTime = Mathf.CeilToInt(elapsedTime);
        }

        public void UnregisterEvents()
        {
            // todo: check car comp. still lives
            _carPassengerHandler.OnPassengerJobFinished -= CarPassengerHandler_OnPassengerJobFinished;
            _carMoneyHandler.OnMoneyGained -= CarMoneyHandler_OnMoneyGained;
        }

        private void CarPassengerHandler_OnPassengerJobFinished(PassengerJob job)
        {
            if (job.State != JobState.Completed) return;
            _transportedPassengersCount++;
        }

        private void CarMoneyHandler_OnMoneyGained(OnMoneyGainedEventParams evt)
        {
            _moneyGained = evt.CurrentMoney;
        }
    }
}
