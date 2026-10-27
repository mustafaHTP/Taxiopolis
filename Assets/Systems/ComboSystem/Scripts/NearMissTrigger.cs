using System;
using Game.Utils;
using UnityEngine;
using UnityEngine.UI;

namespace Game.Systems.ComboSystem
{
    public class NearMissTrigger : MonoBehaviour
    {
        public event Action OnVehicleExited;

        private void OnTriggerExit(Collider other)
        {
            OnVehicleExited?.Invoke();
        }
    }
}
