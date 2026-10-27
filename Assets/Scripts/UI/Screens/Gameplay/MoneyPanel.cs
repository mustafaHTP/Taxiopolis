using DG.Tweening;
using Framework.Services;
using Game.Car;
using Game.Common;
using Game.UI.Common;
using Game.Utils;
using TMPro;
using UnityEngine;

namespace Game.UI.Screens.Gameplay
{
    public class MoneyPanel : UIPanel
    {
        [Header("References")]
        [SerializeField] private TextMeshProUGUI _moneyText;
        [SerializeField] private RectTransform _visual;

        [Header("Settings")]
        [SerializeField] private int _displayedDigits = 9;
        [SerializeField] private float _countDuration = 0.6f;
        [SerializeField] private Ease _countEase = Ease.OutQuad;

        [Header("Slide Animation")]
        [SerializeField] private float _offscreenMargin = 50f;
        [SerializeField] private float _slideInDuration = 0.35f;
        [SerializeField] private float _slideOutDuration = 0.25f;
        [SerializeField] private Ease _slideInEase = Ease.OutBack;
        [SerializeField] private Ease _slideOutEase = Ease.InCubic;

        [Header("Money Pop Animation")]
        [SerializeField] private float _popScale = 1.3f;
        [SerializeField] private float _popDuration = 0.12f;

        private Tween _countTween;
        private Tween _slideTween;
        private Tween _popTween;
        private int _displayedMoney;
        private Vector2 _shownPosition;
        private CarMoneyHandler _carMoneyHandler;

        private float SlideDistance => _visual.rect.width + _offscreenMargin;

        public override void Init()
        {
            _shownPosition = _visual.anchoredPosition;

            _carMoneyHandler = ServiceLocator
                .Scene
                .GetService<Player>()
                .GetComponent<CarMoneyHandler>();
            _carMoneyHandler.OnMoneyGained += CarMoneyHandler_OnMoneyGained;
        }

        public override void Dispose()
        {
            _countTween?.Kill();
            _slideTween?.Kill();
            _popTween?.Kill();

            _carMoneyHandler.OnMoneyGained -= CarMoneyHandler_OnMoneyGained;
            _carMoneyHandler = null;
        }

        public override void Show()
        {
            SetInstant(_carMoneyHandler.CurrentMoney);

            ActivateObject();
            SlideIn();
        }

        public override void Hide()
        {
            _countTween?.Kill();
            _popTween?.Kill();

            SlideOut();
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

        private void CarMoneyHandler_OnMoneyGained(OnMoneyGainedEventParams evt)
        {
            AnimateTo(evt.CurrentMoney);
            PopMoneyText();
        }

        private void AnimateTo(int targetMoney)
        {
            _countTween?.Kill();

            _countTween = DOTween.To(
                    () => _displayedMoney,
                    value =>
                    {
                        _displayedMoney = value;
                        _moneyText.SetText(MoneyUtils.FormatMoney(value, _displayedDigits));
                    },
                    targetMoney,
                    _countDuration)
                .SetEase(_countEase)
                .SetUpdate(true); // keeps counting if timeScale is 0
        }

        private void SetInstant(int money)
        {
            _countTween?.Kill();
            _displayedMoney = money;
            _moneyText.SetText(MoneyUtils.FormatMoney(money, _displayedDigits));
        }
    }
}