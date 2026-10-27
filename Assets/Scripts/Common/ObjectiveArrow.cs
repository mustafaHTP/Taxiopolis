using DG.Tweening;
using Framework.Services;
using Game.Common;
using Game.Systems.PassengerSystem;
using UnityEngine;

public class ObjectiveArrow : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform _visualTransform;
    [Space(5)]
    [SerializeField]
    [Tooltip("The transform that the arrow should point towards.")]
    private Transform _targetTransform;
    [SerializeField]
    [Tooltip("The transform that serves as the base for rotation.")]
    private Transform _baseTransform;
    [Header("Rotation Settings")]
    [SerializeField] private float _rotationSpeed = 10f;
    [SerializeField] private Vector3 _rotationOffset = new(45f, 0f, 0f);

    [Header("Animation Settings")]
    [Space(5)]
    [SerializeField] private float _activationDuration = 1f;
    [SerializeField] private float _deactivationDuration = 1f;
    [SerializeField] private Ease _activationEase = Ease.OutElastic;
    [SerializeField] private Ease _deactivationEase = Ease.InElastic;

    /// <summary>
    /// The base rotation the arrow should always keep (local).
    /// </summary>
    private Quaternion _baseRotation;
    private Vector3 _initialScale;
    private CarPassengerHandler _carPassengerHandler;

    private void Awake()
    {
        _baseRotation = _visualTransform.localRotation;
        _initialScale = _visualTransform.localScale;

        _carPassengerHandler = ServiceLocator.Scene.GetService<Player>().GetComponent<CarPassengerHandler>();
    }

    private void OnEnable()
    {
        Debug.Log($"ObjectiveArrow enabled. Handler: {_carPassengerHandler}");

        _carPassengerHandler.OnPassengerPickedUp += CarPassengerHandler_OnPassengerPickedUp;
        _carPassengerHandler.OnPassengerDroppedOff += CarPassengerHandler_OnPassengerDroppedOff;
    }

    private void Start()
    {
        Deactivate(true);
    }

    private void OnDisable()
    {
        _carPassengerHandler.OnPassengerPickedUp -= CarPassengerHandler_OnPassengerPickedUp;
        _carPassengerHandler.OnPassengerDroppedOff -= CarPassengerHandler_OnPassengerDroppedOff;
    }

    private void CarPassengerHandler_OnPassengerPickedUp(PassengerJob job)
    {
        Activate(
            job.Car.transform,
            job.DestinationPoint.transform,
            false);
    }

    private void CarPassengerHandler_OnPassengerDroppedOff(PassengerJob job)
    {
        Deactivate(false);
    }

    private void Activate(Transform carTransform, Transform destinationPointTransform, bool isInstantActivation)
    {
        _targetTransform = destinationPointTransform;
        _baseTransform = carTransform;

        if (isInstantActivation)
        {
            _visualTransform.gameObject.SetActive(true);
            _visualTransform.localScale = _initialScale;
        }
        else
        {
            _visualTransform.gameObject.SetActive(true);
            _visualTransform
                    .DOScale(_initialScale, _activationDuration)
                    .From(Vector3.zero)
                    .SetEase(_activationEase);
        }
    }

    private void Deactivate(bool isInstantDeactivation)
    {
        _targetTransform = null;
        _baseTransform = null;

        if (isInstantDeactivation)
        {
            _visualTransform.localScale = Vector3.zero;
            _visualTransform.gameObject.SetActive(false);
        }
        else
        {
            _visualTransform
                .DOScale(Vector3.zero, _deactivationDuration)
                .SetEase(_deactivationEase)
                .OnComplete(() => _visualTransform.gameObject.SetActive(false));
        }
    }

    private void Update()
    {
        if (!_targetTransform || !_baseTransform) return;

        Vector3 worldDir = _targetTransform.position - _baseTransform.position;
        worldDir.y = 0f;

        if (worldDir.sqrMagnitude < 0.001f) return;

        // Convert world direction into the player's local space.
        Vector3 localDir = _baseTransform.InverseTransformDirection(worldDir);

        // Calculate the angle relative to player's forward direction.
        float angle = Mathf.Atan2(localDir.x, localDir.z) * Mathf.Rad2Deg;

        Quaternion targetRotation =
            _baseRotation *
            Quaternion.Euler(_rotationOffset.x, _rotationOffset.y + angle, _rotationOffset.z);

        _visualTransform.localRotation = Quaternion.Slerp(
            _visualTransform.localRotation,
            targetRotation,
            _rotationSpeed * Time.deltaTime);
    }
}