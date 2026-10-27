using DG.Tweening;
using Framework.Services;
using Game.Systems.SoundSystem;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Game.UI.Common
{
    public class CustomButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
    {
        [Header("Button On Pointer Enter Settings")]
        [SerializeField] private bool _playSoundOnHover = true;
        [SerializeField] private float _hoverAnimationScale = 1.3f;
        [SerializeField] private float _hoverAnimationDuration = 0.5f;
        [SerializeField] private Ease _hoverAnimationEase;

        [Header("References")]
        [SerializeField] private SoundData _hoverSound;
        [SerializeField] private SoundData _clickSound;

        public void OnPointerClick(PointerEventData eventData)
        {
            ServiceLocator
                .Global
                .GetService<SoundManager>()
                .PlaySound(_clickSound);
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            if (_playSoundOnHover)
            {
                ServiceLocator
                    .Global
                    .GetService<SoundManager>()
                    .PlaySound(_hoverSound);
            }

            transform.DOKill();
            transform
                .DOScale(Vector3.one * _hoverAnimationScale, _hoverAnimationDuration)
                .SetEase(_hoverAnimationEase)
                .SetUpdate(true);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            transform.DOKill();
            transform
                .DOScale(Vector3.one, _hoverAnimationDuration)
                .SetEase(_hoverAnimationEase)
                .SetUpdate(true);
        }

        private void OnDisable()
        {
            // Kill any running tween to avoid stacking
            transform.DOKill();

            // Reset scale
            transform.localScale = Vector3.one;
        }
    }
}
