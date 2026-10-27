using DG.Tweening;
using Framework.Services;
using Game.Common;
using Game.Systems.PassengerSystem;
using TMPro;
using UnityEngine;

namespace Game.UI.Float
{
    public class UI_Float_PassengerTimer : MonoBehaviour
    {
        [Header("Settings")]
        [SerializeField] private float _animationDuration = 1f;
        [SerializeField] private Ease _animationEase = Ease.InOutElastic;
        [SerializeField] private float _animationScale = 1.3f;

        [Header("References")]
        [SerializeField] private GameObject _timeDisplayObject;
        [SerializeField] private TextMeshProUGUI _timeText;

        private CarPassengerHandler _carPassengerHandler;

        private void ShowText()
        {
            _timeDisplayObject.SetActive(true);
            _timeDisplayObject.transform.DOKill();
            _timeDisplayObject.transform
                .DOScale(_animationScale, _animationDuration)
                .SetEase(_animationEase)
                .OnComplete(() =>
                {
                    _timeDisplayObject.transform.DOScale(1f, _animationDuration)
                    .SetEase(_animationEase);
                });
            _timeText.SetText($"{(int)_carPassengerHandler.CurrentJob.TimeLeft}");
        }

        private void HideText()
        {
            _timeDisplayObject.transform.DOKill();
            _timeDisplayObject.transform
                .DOScale(0f, _animationDuration)
                .SetEase(_animationEase)
                .OnComplete(() =>
                {
                    _timeDisplayObject.SetActive(false);
                    _timeText.SetText("0");
                });
        }

        private void Awake()
        {
            _carPassengerHandler = ServiceLocator
                .Scene
                .GetService<Player>()
                .GetComponent<CarPassengerHandler>();

            _timeDisplayObject.SetActive(false);
            _timeText.SetText("0");
        }

        private void OnEnable()
        {
            _carPassengerHandler.OnPassengerPickedUp += CarPassengerHandler_OnPassengerPickedUp;
            _carPassengerHandler.OnPassengerDroppedOff += CarPassengerHandler_OnPassengerDroppedOff;
        }

        private void OnDisable()
        {
            _carPassengerHandler.OnPassengerPickedUp -= CarPassengerHandler_OnPassengerPickedUp;
            _carPassengerHandler.OnPassengerDroppedOff -= CarPassengerHandler_OnPassengerDroppedOff;
        }

        private void CarPassengerHandler_OnPassengerDroppedOff(PassengerJob job)
        {
            HideText();
        }

        private void CarPassengerHandler_OnPassengerPickedUp(PassengerJob job)
        {
            ShowText();
        }


        private void Update()
        {
            if (_carPassengerHandler == null || !_carPassengerHandler.HasJob()) return;

            if (_timeDisplayObject.activeInHierarchy)
            {
                _timeText.SetText("{0}", (int)_carPassengerHandler.CurrentJob.TimeLeft);
            }
        }
    }
}
