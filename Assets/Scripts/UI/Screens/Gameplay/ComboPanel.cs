using DG.Tweening;
using Framework.Services;
using Game.Common;
using Game.Systems.ComboSystem;
using Game.UI.Common;
using TMPro;
using UnityEngine;

namespace Game.UI.Screens.Gameplay
{
    public class ComboPanel : UIPanel
    {
        [Header("References")]
        [SerializeField] private TextMeshProUGUI _comboNameText;
        [SerializeField] private TextMeshProUGUI _comboCountText;
        [SerializeField] private RectTransform _comboNameRect;
        [SerializeField] private RectTransform _comboInfoRect;
        [SerializeField] private RectTransform _visual;

        [Header("Settings")]
        [SerializeField] private int _minChainToShowInfo = 2;

        [Header("Combo Count Settings")]
        [SerializeField] private float _comboCountPunchDuration = 0.3f;
        [SerializeField] private float _comboCountPunchScale = 0.3f;
        [SerializeField] private int _comboCountPunchVibrato = 10;
        [SerializeField] private float _comboCountPunchElasticity = 1f;

        [Header("Slide Animation")]
        [SerializeField] private float _offscreenMargin = 50f;
        [SerializeField] private float _slideInDuration = 0.35f;
        [SerializeField] private float _slideOutDuration = 0.25f;
        [SerializeField] private Ease _slideInEase = Ease.OutBack;
        [SerializeField] private Ease _slideOutEase = Ease.InCubic;

        private CarComboHandler _carComboHandler;
        private Vector2 _shownPosition;
        private Tween _slideTween;
        private bool _isShown;

        private float SlideDistance => _visual.rect.width + _offscreenMargin;

        public override void Init()
        {
            _shownPosition = _visual.anchoredPosition;

            _carComboHandler = ServiceLocator
                .Scene
                .GetService<Player>()
                .GetComponent<CarComboHandler>();

            _carComboHandler.OnComboSuccess += CarComboHandler_OnComboSuccess;
            _carComboHandler.OnComboChainBroken += CarComboHandler_OnComboChainBroken;
        }

        public override void Dispose()
        {
            _carComboHandler.OnComboSuccess -= CarComboHandler_OnComboSuccess;
            _carComboHandler.OnComboChainBroken -= CarComboHandler_OnComboChainBroken;

            _carComboHandler = null;

            _slideTween?.Kill();
            _comboCountText.rectTransform.DOKill();
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
            _comboCountText.rectTransform.DOKill();

            _isShown = false;
            _visual.gameObject.SetActive(false);
        }

        private void CarComboHandler_OnComboSuccess(ComboSuccessEventArgs eventArgs)
        {
            _comboNameText.SetText(eventArgs.ComboInfo.CoolName);

            // 1 is not a combo yet: show the name, hide the info rect
            bool showInfo = eventArgs.Chain >= _minChainToShowInfo;
            _comboInfoRect.gameObject.SetActive(showInfo);

            if (showInfo)
            {
                _comboCountText.SetText("{0}", eventArgs.Chain);
                PunchComboCount();
            }

            if (!_isShown)
                SlideIn();
        }

        private void CarComboHandler_OnComboChainBroken()
        {
            SlideOut();
        }

        private void PunchComboCount()
        {
            var rect = _comboCountText.rectTransform;
            rect.DOKill();                  // stop previous tween
            rect.localScale = Vector3.one;  // reset in case it was interrupted

            rect.DOPunchScale(
                    Vector3.one * _comboCountPunchScale,
                    _comboCountPunchDuration,
                    _comboCountPunchVibrato,
                    _comboCountPunchElasticity)
                .SetUpdate(true);
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
                .OnComplete(() =>
                {
                    _visual.gameObject.SetActive(false);
                    _comboNameText.SetText(string.Empty);
                    _comboCountText.SetText(string.Empty);
                });
        }
    }
}
