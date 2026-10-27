using DG.Tweening;
using Framework.Services;
using Game.Common;
using Game.Systems.PassengerSystem;
using Game.UI.Common;
using TMPro;
using UnityEngine;

namespace Game.UI.Screens.Gameplay
{
    public class PromptPanel : UIPanel
    {
        [Header("References")]
        [SerializeField] private TextMeshProUGUI _promptText;
        [SerializeField] private RectTransform _visual;

        [Header("Prompts")]
        [SerializeField] private string _searchPassengerPrompt = "Search Passengers and Horn !!";

        [Header("Settings")]
        [SerializeField] private float _searchPassengerPromptTime = 10f;

        [Header("Slide Animation")]
        [SerializeField] private float _offscreenMargin = 50f;
        [SerializeField] private float _slideInDuration = 0.5f;
        [SerializeField] private float _slideOutDuration = 0.4f;
        [SerializeField] private Ease _slideInEase = Ease.OutCubic;
        [SerializeField] private Ease _slideOutEase = Ease.InCubic;

        private CarPassengerHandler _carPassengerHandler;
        private Vector2 _shownPosition;
        private Tween _slideTween;
        private Tween _idleTween;
        private bool _isShown;

        private float SlideDistance => _visual.rect.width + _offscreenMargin;

        public override void Init()
        {
            // Position set in the editor = final on-screen position
            _shownPosition = _visual.anchoredPosition;

            _carPassengerHandler = ServiceLocator
                .Scene
                .GetService<Player>()
                .GetComponent<CarPassengerHandler>();

            _carPassengerHandler.OnPassengerPickedUp += CarPassengerHandler_OnPassengerPickedUp;
            _carPassengerHandler.OnPassengerDroppedOff += CarPassengerHandler_OnPassengerDroppedOff;
        }

        public override void Dispose()
        {
            _slideTween?.Kill();
            StopIdleTimer();

            _carPassengerHandler.OnPassengerPickedUp -= CarPassengerHandler_OnPassengerPickedUp;
            _carPassengerHandler.OnPassengerDroppedOff -= CarPassengerHandler_OnPassengerDroppedOff;

            _carPassengerHandler = null;
        }

        public override void Show()
        {
            _slideTween?.Kill();

            _isShown = false;
            _visual.gameObject.SetActive(false);

            ActivateObject();
        }

        public override void Hide()
        {
            _slideTween?.Kill();
            StopIdleTimer();

            _isShown = false;
            _visual.gameObject.SetActive(false);
        }

        #region Event handlers

        private void CarPassengerHandler_OnPassengerPickedUp(PassengerJob job)
        {
            StopIdleTimer();
            SlideOut();
        }

        private void CarPassengerHandler_OnPassengerDroppedOff(PassengerJob job)
        {
            StartIdleTimer();
        }

        #endregion

        #region Idle timer

        private void StartIdleTimer()
        {
            StopIdleTimer();

            // Tween-based timer: works even if this GameObject is inactive.
            // Pass ignoreTimeScale: true as the 3rd argument if it should run while paused.
            _idleTween = DOVirtual.DelayedCall(
                _searchPassengerPromptTime,
                () =>
                {
                    _idleTween = null;
                    SlideIn(_searchPassengerPrompt);
                });
        }

        private void StopIdleTimer()
        {
            _idleTween?.Kill();
            _idleTween = null;
        }

        #endregion

        #region Slide

        private void SlideIn(string prompt)
        {
            if (_isShown)
                return;

            _slideTween?.Kill();

            _isShown = true;
            _promptText.SetText(prompt);

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
                .OnComplete(() => _visual.gameObject.SetActive(false));
        }

        #endregion
    }
}