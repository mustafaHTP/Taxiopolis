using System;
using Framework.CustomEventBus;
using Framework.Services;
using Game.Systems.SoundSystem;
using UnityEngine;

namespace Game.Systems.CollectibleSystem
{
    public class CarCollectibleHandler : MonoBehaviour
    {
        public event Action<CollectibleData> OnCollected;

        private void OnTriggerEnter(Collider other)
        {
            if (other.TryGetComponent<ICollectible>(out var collectible))
            {
                collectible.Collect();

                ServiceLocator.Global.GetService<SoundManager>().PlaySound(collectible.CollectibleData.SoundData);

                OnCollected?.Invoke(collectible.CollectibleData);
            }
        }
    }
}
