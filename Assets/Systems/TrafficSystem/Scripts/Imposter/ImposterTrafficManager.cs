using Game.Utils;
using UnityEngine;

namespace Game.Systems.TrafficSystem.Imposter
{
    // The reason i write this imposter class is normal traffic system is too resource-hungry 
    // and makes lots checks which is completely unnecessary for main menu
    public class ImposterTrafficManager : MonoBehaviour
    {
        [SerializeField] private ImposterTrafficRoad[] _imposterTrafficRoads;

        [ContextMenu("Start Traffic")]
        public void StartTraffic()
        {
            if (_imposterTrafficRoads == null || _imposterTrafficRoads.Length == 0)
            {
                LogX.LogError($"{nameof(ImposterTrafficRoad)} is not set");

                return;
            }

            for (int i = 0; i < _imposterTrafficRoads.Length; i++)
            {
                _imposterTrafficRoads[i].StartTraffic();
            }
        }

        [ContextMenu("Clear Traffic")]
        public void ClearTraffic()
        {
            if (_imposterTrafficRoads == null || _imposterTrafficRoads.Length == 0)
            {
                LogX.LogError($"{nameof(ImposterTrafficRoad)} is not set");

                return;
            }

            for (int i = 0; i < _imposterTrafficRoads.Length; i++)
            {
                _imposterTrafficRoads[i].ClearTraffic();
            }
        }
    }
}
