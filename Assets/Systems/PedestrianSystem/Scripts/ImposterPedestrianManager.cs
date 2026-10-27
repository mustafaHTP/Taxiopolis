using Game.Utils;
using UnityEngine;

namespace Game.Systems.PedestrianSystem
{
    // The reason i write this imposter class is normal pedestrian system is too resource-hungry 
    // and makes lots checks which is completely unnecessary for main menu
    // It is similar to pedestrian manager but more flexible (not depend on sim sphere and spawner)
    public class ImposterPedestrianManager : MonoBehaviour
    {
        [SerializeField] private PedestrianPoint[] _pedestrianPoints;
        [SerializeField] private Pedestrian[] _pedestrians;
        [SerializeField] private float _mixPedestrianSpeed = 0.5f;
        [SerializeField] private float _maxPedestrianSpeed = 1.3f;

        private Pedestrian[] _spawnedPedestrians;

        [ContextMenu("Start Traffic")]
        public void StartTraffic()
        {
            if (_pedestrianPoints == null || _pedestrianPoints.Length == 0)
            {
                LogX.LogError("Pedestrian points not set!");

                return;
            }

            if (_pedestrians == null || _pedestrians.Length == 0)
            {
                LogX.LogError("Pedestrians not set!");

                return;
            }

            if (_spawnedPedestrians != null)
            {
                ClearTraffic();
            }

            // Init array before spawn
            _spawnedPedestrians = new Pedestrian[_pedestrianPoints.Length];

            // Spawn based on pedestrian points
            // It means try to spawn pedestrian every pedestrian points
            for (int i = 0; i < _pedestrianPoints.Length; i++)
            {
                // get random pedestrian
                int randomPedestrianIndex = Random.Range(0, _pedestrians.Length);
                Pedestrian pedestrian = _pedestrians[randomPedestrianIndex];

                Pedestrian spawnedPedestrian = Instantiate(pedestrian, _pedestrianPoints[i].transform.position, Quaternion.identity);

                spawnedPedestrian.MoveSpeed = Random.Range(_mixPedestrianSpeed, _maxPedestrianSpeed);
                spawnedPedestrian.transform.forward = _pedestrianPoints[i].transform.forward;
                spawnedPedestrian.SetWaypoint(_pedestrianPoints[i]);

                _spawnedPedestrians[i] = spawnedPedestrian;
            }
        }

        [ContextMenu("Clear Traffic")]
        public void ClearTraffic()
        {
            if (_spawnedPedestrians == null || _spawnedPedestrians.Length == 0) return;

            for (int i = 0; i < _spawnedPedestrians.Length; i++)
            {
                Destroy(_spawnedPedestrians[i]);
            }

            _spawnedPedestrians = null;
        }
    }
}
