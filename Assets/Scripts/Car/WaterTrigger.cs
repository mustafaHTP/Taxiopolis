using System;
using Game.Utils;
using UnityEngine;

namespace Game.Car
{
    /// <summary>
    /// Detects when the car enters the water.
    /// </summary>
    /// <remarks>
    /// Scene-specific note:
    /// In the city level, water sits just below ground level and covers
    /// most of the map. A large trigger would overlap the ground and
    /// trigger unintentionally during normal driving.
    /// Therefore, this trigger should be slightly smaller than the
    /// car's collider to detect actual falls into the water.
    /// </remarks>
    public class WaterTrigger : MonoBehaviour
    {
        public event Action OnWaterDetected;

        private void OnTriggerEnter(Collider other)
        {
            //! Water detection relies on the configured physics layers.
            //! Ensure the trigger only interacts with the intended water layer.
            OnWaterDetected?.Invoke();
        }
    }
}
