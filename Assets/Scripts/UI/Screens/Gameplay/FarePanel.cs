using DG.Tweening;
using Framework.Services;
using Game.Common;
using Game.Systems.PassengerSystem;
using Game.UI.Common;
using Game.Utils;
using TMPro;
using UnityEngine;

namespace Game.UI.Screens.Gameplay
{
    public class FarePanel : UIPanel
    {
        [Header("References")]
        [SerializeField] private TextMeshProUGUI _moneyText;
        [SerializeField] private RectTransform _visual;

        [Header("Settings")]
        [SerializeField] private int _displayedDigits = 9;

        [Header("Slide Animation")]
        [SerializeField] private float _offscreenMargin = 50f;
        [SerializeField] private float _slideInDuration = 0.35f;
        [SerializeField] private float _slideOutDuration = 0.25f;
        [SerializeField] private Ease _slideInEase = Ease.OutBack;
        [SerializeField] private Ease _slideOutEase = Ease.InCubic;

        [Header("Money Pop Animation")]
        [SerializeField] private float _popScale = 1.3f;
        [SerializeField] private float _popDuration = 0.12f;

        private CarPassengerHandler _carPassengerHandler;
        private Vector2 _shownPosition;
        private Tween _slideTween;
        private Tween _popTween;

        private float SlideDistance => _visual.rect.width + _offscreenMargin;

        public override void Init()
        {
            _shownPosition = _visual.anchoredPosition;

            _carPassengerHandler = ServiceLocator
                .Scene
                .GetService<Player>()
                .GetComponent<CarPassengerHandler>();

            _carPassengerHandler.OnPassengerPickedUp += CarPassengerHandler_OnPassengerPickedUp;
            _carPassengerHandler.OnPassengerJobFinished += CarPassengerHandler_OnPassengerJobFinished;
            _carPassengerHandler.OnFareBonusEarned += CarPassengerHandler_OnFareBonusEarned;
        }

        public override void Dispose()
        {
            _carPassengerHandler.OnPassengerPickedUp -= CarPassengerHandler_OnPassengerPickedUp;
            _carPassengerHandler.OnPassengerJobFinished -= CarPassengerHandler_OnPassengerJobFinished;
            _carPassengerHandler.OnFareBonusEarned -= CarPassengerHandler_OnFareBonusEarned;

            _carPassengerHandler = null;

            _slideTween?.Kill();
            _popTween?.Kill();
        }

        public override void Show()
        {
            _slideTween?.Kill();
            _visual.gameObject.SetActive(false);
        }

        public override void Hide()
        {
            _slideTween?.Kill();
            _popTween?.Kill();
            _visual.gameObject.SetActive(false);
        }

        private void SlideIn()
        {
            _slideTween?.Kill();

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
            if (!_visual.gameObject.activeSelf)
                return;

            _slideTween?.Kill();

            // leave off screen to the left
            _slideTween = _visual
                .DOAnchorPos(_shownPosition + new Vector2(SlideDistance, 0f), _slideOutDuration)
                .SetEase(_slideOutEase)
                .SetUpdate(true)
                .OnComplete(() => _visual.gameObject.SetActive(false));
        }

        private void PopMoneyText()
        {
            _popTween?.Kill();

            Transform t = _moneyText.transform;
            t.localScale = Vector3.one;

            // scale up, then back down
            _popTween = t
                .DOScale(_popScale, _popDuration)
                .SetEase(Ease.OutQuad)
                .SetLoops(2, LoopType.Yoyo)
                .SetUpdate(true)
                .OnKill(() => t.localScale = Vector3.one);
        }

        private void UpdateMoney(PassengerJob job)
        {
            _moneyText.SetText(MoneyUtils.FormatMoney(job.TotalFare, _displayedDigits));
        }

        private void CarPassengerHandler_OnFareBonusEarned(PassengerJob job, int reward)
        {
            UpdateMoney(job);
            PopMoneyText();
        }

        private void CarPassengerHandler_OnPassengerJobFinished(PassengerJob job)
        {
            SlideOut();
        }

        private void CarPassengerHandler_OnPassengerPickedUp(PassengerJob job)
        {
            UpdateMoney(job);
            SlideIn();
        }
    }
}