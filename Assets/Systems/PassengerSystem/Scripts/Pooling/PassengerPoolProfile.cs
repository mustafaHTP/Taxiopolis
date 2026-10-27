using Game.Systems.PassengerSystem.Passengers;
using UnityEngine;

namespace Game.Systems.PassengerSystem.Pooling
{
    [CreateAssetMenu(fileName = "New Passenger Pool Profile", menuName = "Game/Passenger System/Pool/Passenger Pool Profile", order = 1)]
    public class PassengerPoolProfile : ScriptableObject
    {
        [SerializeField] private Passenger[] _passengerPrefabs;

        public Passenger[] PassengerPrefabs => _passengerPrefabs;
    }
}
