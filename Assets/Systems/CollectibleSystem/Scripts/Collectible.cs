using DG.Tweening;
using Game.Utils;
using System;
using UnityEngine;

namespace Game.Systems.CollectibleSystem
{
    public class Collectible : MonoBehaviour, ICollectible
    {
        [Header("Debug")]
        [SerializeField] private bool _debugDrawColliderSphere = true;
        [SerializeField] private Color _debugColliderSphereColor = Color.yellow;

        [Header("Data")]
        [SerializeField] private CollectibleData _collectibleData;

        [Header("Settings")]
        [SerializeField] private OnCollectAction _onCollectAction = OnCollectAction.Destroy;
        [SerializeField] private float _collectAnimationDuration = 0.5f;
        [SerializeField] private Ease _collectAnimationEasing = Ease.InBack;

        [Header("References")]
        [SerializeField] private Transform _visualTransform;
        [SerializeField] private SphereCollider _collider;

        [Header("Rotation Settings")]
        [SerializeField] private bool _rotateX = true;
        [SerializeField] private bool _rotateY = true;
        [SerializeField] private bool _rotateZ = true;
        [SerializeField] private float _rotationDuration = 5f;
        [SerializeField] private Ease _easingType = Ease.Linear;

        private enum OnCollectAction
        {
            None,
            Destroy,
            Disable
        }

        public CollectibleData CollectibleData => _collectibleData;

        public void Collect()
        {
            LogX.LogDebug("Collectible collected.");

            DOTween.Kill(_visualTransform);

            _visualTransform.DOScale(Vector3.zero, _collectAnimationDuration)
                .SetEase(_collectAnimationEasing)
                .OnComplete(OnCollectAnimationComplete);
        }

        private void OnCollectAnimationComplete()
        {
            switch (_onCollectAction)
            {
                case OnCollectAction.None:
                    break;
                case OnCollectAction.Destroy:
                    Destroy(gameObject);
                    break;
                case OnCollectAction.Disable:
                    gameObject.SetActive(false);
                    break;
            }
        }

        private void OnDrawGizmos()
        {
            if (_debugDrawColliderSphere && _collider != null)
            {
                Gizmos.color = _debugColliderSphereColor;

                // Add a small offset to make the sphere more visible
                Gizmos.DrawWireSphere(
                    transform.position,
                    _collider.radius + 0.1f);
            }
        }

        private void Start()
        {
            StartRotateVisual();
        }

        private void StartRotateVisual()
        {
            if (_visualTransform == null)
            {
                LogX.LogWarning($"Mesh Transform is not assigned in {name}");

                return;
            }

            Vector3 rotationAxes = new(
                _rotateX ? 360f : 0f,
                _rotateY ? 360f : 0f,
                _rotateZ ? 360f : 0f
            );

            _visualTransform
                .DORotate(rotationAxes, _rotationDuration, RotateMode.LocalAxisAdd)
                .SetEase(_easingType)
                .SetLoops(-1, LoopType.Restart);
        }
    }
}
