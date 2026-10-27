using DG.Tweening;
using Game.UI.Common;
using UnityEngine;

namespace Game.UI.Screens.Gameplay
{
    public class AbilityPanel : UIPanel
    {
        [Header("References")]
        [SerializeField] private RectTransform _visual;
        [SerializeField] private NitroAbilityView _nitroAbilityPanel;
        [SerializeField] private ShockwaveAbilityView _shockwaveAbilityPanel;
        [SerializeField] private SpeedbreakerAbilityView _speedbreakerAbilityPanel;

        [Header("Slide Animation")]
        [SerializeField] private float _offscreenMargin = 50f;
        [SerializeField] private float _slideInDuration = 0.35f;
        [SerializeField] private float _slideOutDuration = 0.25f;
        [SerializeField] private Ease _slideInEase = Ease.OutBack;
        [SerializeField] private Ease _slideOutEase = Ease.InCubic;

        private Vector2 _shownPosition;
        private Tween _slideTween;
        private bool _isShown;

        private float SlideDistance => _visual.rect.width + _offscreenMargin;

        public override void Init()
        {
            // Position set in the editor = final on-screen position
            _shownPosition = _visual.anchoredPosition;

            _nitroAbilityPanel.Init();
            _shockwaveAbilityPanel.Init();
            _speedbreakerAbilityPanel.Init();
        }

        public override void Dispose()
        {
            _slideTween?.Kill();

            _nitroAbilityPanel.Dispose();
            _shockwaveAbilityPanel.Dispose();
            _speedbreakerAbilityPanel.Dispose();
        }

        public override void Show()
        {
            ActivateObject();
            SlideIn();
        }

        public override void Hide()
        {
            SlideOut();
        }

        private void SlideIn()
        {
            _slideTween?.Kill();

            _isShown = true;
            _visual.gameObject.SetActive(true);

            // start off screen on the left
            _visual.anchoredPosition = _shownPosition - new Vector2(SlideDistance, 0f);

            _slideTween = _visual
                .DOAnchorPos(_shownPosition, _slideInDuration)
                .SetEase(_slideInEase)
                .SetUpdate(true);
        }

        private void SlideOut()
        {
            if (!_isShown)
                return;

            _slideTween?.Kill();

            _isShown = false;

            // leave off screen to the left (back where it came from)
            _slideTween = _visual
                .DOAnchorPos(_shownPosition - new Vector2(SlideDistance, 0f), _slideOutDuration)
                .SetEase(_slideOutEase)
                .SetUpdate(true)
                .OnComplete(() => DeactivateObject());
        }
    }
}