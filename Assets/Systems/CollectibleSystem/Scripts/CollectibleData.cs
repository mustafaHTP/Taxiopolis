using Game.Systems.SoundSystem;
using UnityEngine;

namespace Game.Systems.CollectibleSystem
{
    [CreateAssetMenu(fileName = "New Collectible Data", menuName = "Game/Collectible System/Collectible Data")]
    public class CollectibleData : ScriptableObject
    {
        [SerializeField] private string _name;
        [SerializeField] private CollectibleType _type;
        [SerializeField] private int _amount;
        [SerializeField] private Vector3 _spawnOffset;
        [SerializeField] private SoundData _soundData;

        public string Name => _name;
        public CollectibleType Type => _type;
        public int Amount => _amount;
        public Vector3 SpawnOffset => _spawnOffset;
        public SoundData SoundData => _soundData;
    }
}
