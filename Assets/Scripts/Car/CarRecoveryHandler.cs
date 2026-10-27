using Framework.Services;
using Game.Input;
using Game.Systems.SoundSystem;
using RoadSystem;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using LogX = Game.Utils.LogX;

namespace Game.Car
{
    [RequireComponent(typeof(CarController))]
    public class CarRecoveryHandler : MonoBehaviour
    {
        [SerializeField] private SoundData _carRecoverySoundData;
        [SerializeField] private float _recoveryHeightOffset = 1.0f;
        [SerializeField] private float _recoveryTime = 1.0f;
        [SerializeField] private float _recoveryForce;

        private CarController _carController;
        private ICarInputProvider _carInputProvider;
        private Coroutine _carRecoveryCoroutine;
        private RoadNetwork _roadNetwork;
        private bool _isRecovering;

        public void Recover(bool playSoundEffect)
        {
            if (_isRecovering) return;

            RoadPoint closestRoadPoint = _roadNetwork.GetClosestRoadPoint(transform.position);
            if (closestRoadPoint == null)
            {
                LogX.LogError($"Cannot find closest road point", this);

                return;
            }

            Vector3 rayOriginOffset = closestRoadPoint.transform.up * _recoveryHeightOffset;
            Vector3 rayOrigin = closestRoadPoint.transform.position + rayOriginOffset;

            bool isCollided = Physics.Raycast(
            rayOrigin,
            -Vector3.up,
            out RaycastHit hitInfo,
            float.PositiveInfinity);
            if (!isCollided)
            {
                LogX.LogWarning("Cannot recover car: no ground detected below.");

                return;
            }

            if (_carRecoveryCoroutine != null)
            {
                StopCoroutine(_carRecoveryCoroutine);
                _carRecoveryCoroutine = null;
            }

            _carRecoveryCoroutine = StartCoroutine(RecoverCarCoroutine(hitInfo, playSoundEffect));
        }

        private void Awake()
        {
            if (!TryGetComponent(out _carInputProvider))
            {
                LogX.LogError("PlayerInputs component not found on the GameObject.", this);

                return;
            }

            if (!TryGetComponent(out _carController))
            {
                LogX.LogError("CarController component not found on the GameObject.", this);

                return;
            }
        }

        private void Start()
        {
            _roadNetwork = ServiceLocator.Scene.GetService<RoadNetwork>();
            if (_roadNetwork == null)
            {
                LogX.LogError($"Road network not found on local service locator", this);

                return;
            }

            _carInputProvider.RecoverAction.performed += CarInputProvider_RecoverAction_performed;
        }

        private void FixedUpdate()
        {
            CarEnvCheckResultContainer carEnvCheckResultContainer = _carController.CarState.CarEnvCheckResultContainer;

            // car leaning to left
            if (carEnvCheckResultContainer.Left.IsHit
                && !carEnvCheckResultContainer.Up.IsHit
                && !carEnvCheckResultContainer.Down.IsHit
                && !carEnvCheckResultContainer.Right.IsHit)
            {
                ApplyRecoverForce();
            }
            // car leaning to right
            else if (carEnvCheckResultContainer.Right.IsHit
                && !carEnvCheckResultContainer.Up.IsHit
                && !carEnvCheckResultContainer.Down.IsHit
                && !carEnvCheckResultContainer.Left.IsHit)
            {
                ApplyRecoverForce();
            }
        }

        private void OnDisable()
        {
            _carInputProvider.RecoverAction.performed -= CarInputProvider_RecoverAction_performed;
        }

        private void ApplyRecoverForce()
        {
            if (_isRecovering) return;

            Vector3 recoveryForce = _carInputProvider.FrameInput.Steer * _recoveryForce * _carController.transform.forward;
            _carController.ApplyTorque(recoveryForce, ForceMode.Acceleration);
        }

        private void CarInputProvider_RecoverAction_performed(InputAction.CallbackContext _)
        {
            Recover(true);
        }

        private IEnumerator RecoverCarCoroutine(RaycastHit hitInfo, bool playSoundEffect)
        {
            _isRecovering = true;

            _carController.ResetLinearVelocity();
            _carController.Rigidbody.isKinematic = true;
            if (playSoundEffect)
            {
                ServiceLocator.Global.GetService<SoundManager>().PlaySound(_carRecoverySoundData);
            }

            Vector3 targetPosition = hitInfo.point + hitInfo.normal * _recoveryHeightOffset;
            transform.position = targetPosition;

            // Align car's up direction with ground normal but also keep current forward direction
            // groundNormal = the normal of the ground you hit via raycast
            Vector3 newUp = hitInfo.normal.normalized;

            // Project forward direction onto plane defined by newUp
            Vector3 projectedForward = Vector3.ProjectOnPlane(transform.forward, newUp).normalized;

            // Build rotation that looks in projected forward direction with newUp as up
            Quaternion targetRotation = Quaternion.LookRotation(projectedForward, newUp);
            float elapsedTime = 0f;
            while (elapsedTime <= _recoveryTime)
            {
                elapsedTime += Time.deltaTime;
                float t = Mathf.Clamp01(elapsedTime / _recoveryTime);
                transform.rotation = Quaternion.Slerp(
                    transform.rotation,
                    targetRotation,
                    t);
                yield return null;
            }

            _carController.Rigidbody.isKinematic = false;
            _isRecovering = false;
        }
    }
}
