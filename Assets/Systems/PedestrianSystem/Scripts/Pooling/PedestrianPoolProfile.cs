using UnityEngine;

namespace Game.Systems.PedestrianSystem.Pooling
{
    [CreateAssetMenu(
        fileName = "New Pedestrian Pool Profile",
        menuName = "Game/Pedestrian System/Pooling/Pedestrian Pool Profile")]
    public class PedestrianPoolProfile : ScriptableObject
    {
        [SerializeField] private Pedestrian[] _pedestrianPrefabs;

        public Pedestrian[] PedestrianPrefabs => _pedestrianPrefabs;
    }
}
