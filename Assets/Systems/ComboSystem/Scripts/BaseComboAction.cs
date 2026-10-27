using System;
using UnityEngine;

namespace Game.Systems.ComboSystem
{
    public abstract class BaseComboAction : MonoBehaviour
    {
        [SerializeField] private ComboData _comboData;

        public ComboData ComboData => _comboData;

        public event Action<ComboInfo> OnComboSuccess;

        public virtual void Init() { }

        public virtual void Dispose() { }

        protected void Success()
        {
            OnComboSuccess?.Invoke(new ComboInfo(_comboData.Type, _comboData.CoolName, _comboData.Reward));
        }
    }
}
