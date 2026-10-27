using DG.Tweening;
using Framework.Services;
using Game.Managers;
using Game.UI.Common;
using TMPro;
using UnityEngine;

namespace Game.UI.Screens.Gameplay
{
    public class TimePanel : UIPanel
    {
        [Header("References")]
        [SerializeField] private TextMeshProUGUI _timeText;
        [SerializeField] private RectTransform _visual;

        [Header("Settings")]
        [SerializeField] private Color _hurryUpTimeColor = Color.red;

        [Header("Slide Animation")]
        [SerializeField] private float _offscreenMargin = 50f;
        [SerializeField] private float _slideInDuration = 0.35f;
        [SerializeField] private float _slideOutDuration = 0.25f;
        [SerializeField] private Ease _slideInEase = Ease.OutBack;
        [SerializeField] private Ease _slideOutEase = Ease.InCubic;

        private GameTimeManager _gameTimeManager;
        private int _hurryUpTime;
        private Color _defaultColor;
        private Vector2 _shownPosition;
        private Tween _slideTween;
        private bool _isShown;

        private float SlideDistance => _visual.rect.width + _offscreenMargin;

        public override void Init()
        {
            // Position set in the editor = final on-screen position
            _shownPosition = _visual.anchoredPosition;

            _gameTimeManager = ServiceLocator.Scene.GetService<GameTimeManager>();
            _hurryUpTime = Mathf.CeilToInt(ServiceLocator.Global.GetService<GameManager>().GameConfig.HurryUpTime);
            _gameTimeManager.OnSecondChanged += HandleSecondChanged;
            _defaultColor = _timeText.color;
        }

        public override void Dispose()
        {
            _slideTween?.Kill();

            if (_gameTimeManager != null)
                _gameTimeManager.OnSecondChanged -= HandleSecondChanged;

            _gameTimeManager = null;
        }

        public override void Show()
        {
            HandleSecondChanged(_gameTimeManager.RemainingSeconds);

            ActivateObject();
            SlideIn();
        }

        public override void Hide()
        {
            SlideOut();
        }

        private void HandleSecondChanged(int seconds)
        {
            _timeText.SetText("{0}", seconds);
            _timeText.color = seconds <= _hurryUpTime ? _hurryUpTimeColor : _defaultColor;
        }

        private void SlideIn()
        {
            _slideTween?.Kill();

            _isShown = true;
            _visual.gameObject.SetActive(true);

            // start off screen on the right
            _visual.anchoredPosition = _shownPosition + new Vector2(SlideDistance, 0f);

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

            // leave off screen to the right (back where it came from)
            _slideTween = _visual
                .DOAnchorPos(_shownPosition + new Vector2(SlideDistance, 0f), _slideOutDuration)
                .SetEase(_slideOutEase)
                .SetUpdate(true)
                .OnComplete(() => DeactivateObject());
        }
    }
}