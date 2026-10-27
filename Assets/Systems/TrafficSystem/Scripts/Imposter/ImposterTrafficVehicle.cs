using UnityEngine;
using UnityEngine.Splines;

namespace Game.Systems.TrafficSystem.Imposter
{
    [RequireComponent(typeof(SplineAnimate))]
    public class ImposterTrafficVehicle : MonoBehaviour
    {
        private SplineAnimate _splineAnimate;

        public void Init(SplineContainer splineContainer)
        {
            if (_splineAnimate == null) return;
            if (splineContainer == null) return;

            _splineAnimate.Container = splineContainer;
        }

        public void Play()
        {
            _splineAnimate.Play();
        }

        private void Awake()
        {
            _splineAnimate = GetComponent<SplineAnimate>();
        }
    }
}
