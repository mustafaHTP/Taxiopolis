using System.Collections;
using Game.Utils;
using UnityEngine;

namespace RoadSystem.TrafficLightControl
{
    public class TrafficLightController : MonoBehaviour
    {
        [SerializeField] private TrafficLightCycle _trafficLightCycle;

        public TrafficLightCycle TrafficLightCycle => _trafficLightCycle;

        private void Start()
        {
            if (_trafficLightCycle == null)
            {
                LogX.LogWarning($"{nameof(_trafficLightCycle)} is null");

                return;
            }
            
            if(_trafficLightCycle.TrafficLights == null || _trafficLightCycle.TrafficLights.Length == 0)
            {
                LogX.LogWarning("There is no Traffic Lights in cycle");

                return;
            }

            StartTrafficLightCycle();
        }

        private void StartTrafficLightCycle()
        {
            StartCoroutine(StartTrafficLightCycleCoroutine());
        }

        private IEnumerator StartTrafficLightCycleCoroutine()
        {
            while (true)
            {
                for (int i = 0; i < _trafficLightCycle.TrafficLights.Length; i++)
                {
                    var greenLight = _trafficLightCycle.TrafficLights[i];

                    // Turn all others to red
                    for (int j = 0; j < _trafficLightCycle.TrafficLights.Length; j++)
                    {
                        if (i != j)
                        {
                            _trafficLightCycle.TrafficLights[j].ChangeTrafficLightState(TrafficLightState.Red);
                        }
                    }

                    // Set current to green
                    greenLight.ChangeTrafficLightState(TrafficLightState.Green);
                    yield return new WaitForSeconds(_trafficLightCycle.GreenLightDuration);

                    // Switch to yellow
                    greenLight.ChangeTrafficLightState(TrafficLightState.Yellow);
                    yield return new WaitForSeconds(_trafficLightCycle.YellowLightDuration);

                    // Finally, set it to red (before moving to next)
                    greenLight.ChangeTrafficLightState(TrafficLightState.Red);
                }
            }
        }
    }
}
