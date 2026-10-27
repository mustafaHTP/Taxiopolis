using System;
using Framework.CustomEventBus;
using Framework.Services;
using Game.Systems.SoundSystem;
using Game.Utils;
using UnityEngine;

namespace Game.Systems.ComboSystem
{
    public class CarComboHandler : MonoBehaviour
    {
        public event Action<ComboSuccessEventArgs> OnComboSuccess;
        public event Action OnComboChainBroken;

        [Header("References")]
        [SerializeField]
        [Tooltip("Instead of assigning combo manually, just give combo actions holder object and search under its children.")]
        private Transform _comboActionsHolder;
        [SerializeField] private BaseComboAction[] _comboActions;
        [SerializeField] private SoundData _comboSuccessSoundData;

        [Header("Settings")]
        [SerializeField] private float _comboWindow;

        private int _chain;
        private float _lastComboTime;

        [ContextMenu("Init")]
        public void Init()
        {
            if (_comboActionsHolder == null)
            {
                LogX.LogError("Combo actions holder are not set!");

                return;
            }

            _comboActions = GetComponentsInChildren<BaseComboAction>();
            if (_comboActions == null || _comboActions.Length == 0)
            {
                LogX.LogError("Combo actions are not found!");

                return;
            }

            // register combo actions and listens them
            for (int i = 0; i < _comboActions.Length; i++)
            {
                _comboActions[i].Init();
                _comboActions[i].OnComboSuccess += ComboAction_OnComboSuccess;
            }
        }

        private void Awake()
        {
            Init();
        }

        private void OnDisable()
        {
            for (int i = 0; i < _comboActions.Length; i++)
            {
                _comboActions[i].Dispose();
            }
        }

        private void Update()
        {
            if (_chain > 0 && Time.time > _lastComboTime)
            {
                // broke chain
                _chain = 0;
                OnComboChainBroken?.Invoke();
            }
        }

        private void ComboAction_OnComboSuccess(ComboInfo comboInfo)
        {
            // handle combo chain
            ++_chain;
            _lastComboTime = Time.time + _comboWindow;
            // calculate reward
            int reward = CalculateReward(comboInfo);

            ServiceLocator
                .Global.GetService<SoundManager>()
                .PlaySound(_comboSuccessSoundData);

            OnComboSuccess?.Invoke(new ComboSuccessEventArgs(comboInfo, _chain, reward));
            LogX.LogInfo($"{comboInfo.Type} combo is success");
        }

        private int CalculateReward(ComboInfo comboInfo)
        {
            // Per every 10 combo, multiplies base reward
            return comboInfo.BaseReward * ((_chain / 10) + 1);
        }
    }

    public readonly struct ComboSuccessEventArgs
    {
        public ComboSuccessEventArgs(ComboInfo comboInfo, int chain, int reward)
        {
            ComboInfo = comboInfo;
            Chain = chain;
            Reward = reward;
        }

        public ComboInfo ComboInfo { get; }
        public int Chain { get; }
        public int Reward { get; }
    }

    public readonly struct ComboInfo
    {
        public ComboInfo(ComboType type, string CoolName, int baseReward)
        {
            Type = type;
            this.CoolName = CoolName;
            BaseReward = baseReward;
        }

        public ComboType Type { get; }
        public string CoolName { get; }
        public int BaseReward { get; }
    }
}
