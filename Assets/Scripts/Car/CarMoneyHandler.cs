using System;
using Framework.CustomEventBus;
using Game.Systems.CollectibleSystem;
using Game.Systems.PassengerSystem;
using UnityEditor;
using UnityEngine;

namespace Game.Car
{
    public struct OnMoneyGainedEventParams
    {
        public OnMoneyGainedEventParams(int currentMoney, int gainedMoney)
        {
            CurrentMoney = currentMoney;
            GainedMoney = gainedMoney;
        }

        public int CurrentMoney { get; }
        public int GainedMoney { get; }
    }

    [RequireComponent(typeof(CarPassengerHandler))]
    public class CarMoneyHandler : MonoBehaviour
    {
        public event Action<OnMoneyGainedEventParams> OnMoneyGained;

        private CarPassengerHandler _carPassengerHandler;
        private CarCollectibleHandler _carCollectibleHandler;

        private int _currentMoney = 0;

        public int CurrentMoney => _currentMoney;

        public void AddMoney(int amount)
        {
            _currentMoney += amount;

            OnMoneyGained?.Invoke(new OnMoneyGainedEventParams(CurrentMoney, amount));
        }

        private void Awake()
        {
            _carPassengerHandler = GetComponent<CarPassengerHandler>();
            _carCollectibleHandler = GetComponent<CarCollectibleHandler>();
        }

        private void OnEnable()
        {
            _carPassengerHandler.OnPassengerJobFinished += CarPassengerHandler_OnPassengerJobFinished;
            _carCollectibleHandler.OnCollected += CarCollectibleHandler_OnCollected;
        }

        private void OnDisable()
        {
            _carPassengerHandler.OnPassengerJobFinished -= CarPassengerHandler_OnPassengerJobFinished;
            _carCollectibleHandler.OnCollected -= CarCollectibleHandler_OnCollected;
        }

        private void CarCollectibleHandler_OnCollected(CollectibleData data)
        {
            switch (data.Type)
            {
                case CollectibleType.Money:
                    AddMoney(data.Amount);
                    break;
                case CollectibleType.GoldBar:
                    AddMoney(data.Amount);
                    break;
                case CollectibleType.GoldPile:
                    AddMoney(data.Amount);
                    break;
            }
        }

        private void CarPassengerHandler_OnPassengerJobFinished(PassengerJob job)
        {
            if (job.State != JobState.Completed) return;

            AddMoney(job.TotalFare);
        }
    }

#if UNITY_EDITOR
    [CustomEditor(typeof(CarMoneyHandler))]
    public class CarMoneyHandlerEditor : Editor
    {
        int _moneyAmount = 0;

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            EditorGUILayout.Space(10);

            var carMoneyHandler = (CarMoneyHandler)target;

            GUILayout.BeginHorizontal();

            _moneyAmount = EditorGUILayout.IntField("Money Amount", _moneyAmount);
            if (GUILayout.Button("Add Money"))
            {
                carMoneyHandler.AddMoney(_moneyAmount);
            }

            GUILayout.EndHorizontal();
        }
    }
#endif
}