using System;
using UnityEngine;

namespace Game.Systems.AbilitySystem
{
    public enum AbilityState
    {
        Ready,
        Cooldown,
        Use
    }

    public abstract class BaseAbility : MonoBehaviour
    {
        public Action OnActivation;
        public Action OnDeactivation;
        public Action OnCooldownStarted;
        public Action OnCooldownFinished;

        [field: SerializeField] public AbilityState State { get; private set; }
        [SerializeField] private AbilityType _abilityType;
        [SerializeField] private float _cooldownDuration;

        public AbilityType AbilityType => _abilityType;
        public float CooldownDuration => _cooldownDuration;

        protected float _cooldownEndTime;

        public virtual void Init() { }
        public virtual void Dispose() { }
        public virtual void OnButtonPressed() { }
        public virtual void OnButtonReleased() { }
        public virtual void Tick() { }

        /// <summary>
        /// Useful for ui if you want to show how much amount left
        /// </summary>
        /// <returns></returns>
        public virtual float GetAmountNormalized() => 1f;

        protected void ChangeState(AbilityState abilityState)
        {
            State = abilityState;
        }
    }
}
