using DG.Tweening;
using Framework.Services;
using Game.Common;
using Game.Systems.PassengerSystem;
using Game.UI.Common;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Game.UI.Screens.Gameplay
{
    public class JobResultPanel : UIPanel
    {
        [Header("References")]
        [SerializeField] private RectTransform _visual;
        [SerializeField] private TextMeshProUGUI _resultText;
        [SerializeField] private Image _resultImage;

        [Header("Settings")]
        [SerializeField] private float _displayDuration = 2.5f;

        [Header("Slide Animation")]
        [SerializeField] private float _offscreenMargin = 50f;
        [SerializeField] private float _slideInDuration = 0.4f;
        [SerializeField] private float _slideOutDuration = 0.3f;
        [SerializeField] private Ease _slideInEase = Ease.OutBack;
        [SerializeField] private Ease _slideOutEase = Ease.InCubic;

        private CarPassengerHandler _carPassengerHandler;
        private Vector2 _shownPosition;
        private Tween _slideTween;
        private Tween _holdTween;
        private bool _isShown;

        private float SlideDistance => _visual.rect.height + _offscreenMargin;

        public override void Init()
        {
            // Position set in the editor = final on-screen position
            _shownPosition = _visual.anchoredPosition;

            _carPassengerHandler = ServiceLocator
                .Scene
                .GetService<Player>()
                .GetComponent<CarPassengerHandler>();

            _carPassengerHandler.OnPassengerJobFinished += CarPassengerHandler_OnPassengerJobFinished;
        }

        public override void Dispose()
        {
            _slideTween?.Kill();
            _holdTween?.Kill();

            _carPassengerHandler.OnPassengerJobFinished -= CarPassengerHandler_OnPassengerJobFinished;

            _carPassengerHandler = null;
        }

        public override void Show()
        {
            _slideTween?.Kill();
            _holdTween?.Kill();

            _isShown = false;
            _visual.gameObject.SetActive(false);

            ActivateObject();
        }

        public override void Hide()
        {
            _holdTween?.Kill();
            SlideOut();
        }

        private void CarPassengerHandler_OnPassengerJobFinished(PassengerJob job)
        {
            _resultImage.color = job.PassengerJobResultTier.Color;
            _resultText.SetText($"{job.PassengerJobResultTier.CoolName}");

            if (_isShown)
            {
                // already visible: just restart the stay timer
                StartHoldTimer();
            }
            else
            {
                SlideIn();
            }
        }

        private void SlideIn()
        {
            _slideTween?.Kill();
            _holdTween?.Kill();

            _isShown = true;
            _visual.gameObject.SetActive(true);

            // start off screen at the bottom
            _visual.anchoredPosition = _shownPosition - new Vector2(0f, SlideDistance);

            _slideTween = _visual
                .DOAnchorPos(_shownPosition, _slideInDuration)
                .SetEase(_slideInEase)
                .SetUpdate(true)
                .OnComplete(StartHoldTimer);
        }

        private void StartHoldTimer()
        {
            _holdTween?.Kill();

            _holdTween = DOVirtual.DelayedCall(_displayDuration, SlideOut, true);
        }

        private void SlideOut()
        {
            if (!_isShown)
                return;

            _slideTween?.Kill();
            _holdTween?.Kill();

            _isShown = false;

            // leave off screen to the bottom (back where it came from)
            _slideTween = _visual
                .DOAnchorPos(_shownPosition - new Vector2(0f, SlideDistance), _slideOutDuration)
                .SetEase(_slideOutEase)
                .SetUpdate(true)
                .OnComplete(() => _visual.gameObject.SetActive(false));
        }
    }
}