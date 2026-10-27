using DG.Tweening;
using Game.Input;
using UnityEngine;

namespace Game.Car
{
    public class CarVisualController : MonoBehaviour
    {
        [Header("Tilt settings")]
        [SerializeField] private float _maxPitchAngle = 10f;
        [SerializeField] private float _maxRollAngle = 10f;
        [SerializeField] private float _tiltSpeed = 5f;
        [SerializeField] private Transform _carVisual;

        [Header("Bobbing settings")]
        [SerializeField] private float _bobHeight;
        [SerializeField] private float _bobDuration;
        [SerializeField] private Ease _bobEase;

        private ICarInputProvider _carInputProvider;

        private void Start()
        {
            _carInputProvider = GetComponentInParent<ICarInputProvider>();

            _carVisual
                .DOLocalMoveY(_carVisual.localPosition.y + _bobHeight, _bobDuration)
                .SetEase(_bobEase)
                .SetLoops(-1, LoopType.Yoyo);
        }

        private void Update()
        {
            TiltCarBody();
        }

        private void TiltCarBody()
        {
            float gasInput = _carInputProvider.FrameInput.Gas;
            float brakeInput = _carInputProvider.FrameInput.Brake;
            float steerInput = _carInputProvider.FrameInput.Steer;

            float pitch = (gasInput - brakeInput) * _maxPitchAngle * -1f;
            float roll = steerInput * _maxRollAngle;

            Quaternion targetRotation = Quaternion.Euler(pitch, 0f, roll);
            _carVisual.localRotation = Quaternion.Slerp(
                _carVisual.localRotation,
                targetRotation,
                Time.deltaTime * _tiltSpeed);
        }
    }
}
