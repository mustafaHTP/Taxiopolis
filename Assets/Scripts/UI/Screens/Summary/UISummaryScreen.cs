using DG.Tweening;
using Framework.Services;
using Game.Managers;
using Game.Stats;
using Game.Systems.SoundSystem;
using Game.UI.Common;
using Game.Utils;
using System;
using System.Collections;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Game.UI.Screens.Summary
{
    public class UISummaryScreen : UIScreen
    {
        [Header("Options")]
        [SerializeField] private float _stampAnimationDuration;
        [SerializeField] private float _stampAnimationStartScale = 1.3f;
        [SerializeField] private float _stampAnimationFinishScale = 1.3f;
        [SerializeField] private Ease _stampAnimationEase = Ease.InCubic;
        [SerializeField] private SoundData _stampSoundData;
        [SerializeField] private SoundData _stampFinishSoundData;
        [SerializeField] private float _shakeDuration = 0.5f;
        [SerializeField] private float _shakeStrength = 10f;
        [SerializeField] private int _shakeVibrato = 10;
        [SerializeField] private float _shakeRandomness = 90f;

        [Header("References")]
        [SerializeField] private Button _returnMainMenuButton;
        [SerializeField] private GameObject _stampObject;
        [SerializeField] private SummaryEntry _totalMoney;
        [SerializeField] private SummaryEntry _transportedPassengers;
        [SerializeField] private SummaryEntry _gameTime;

        private Coroutine _finishSummaryCoroutine;

        public override void Show()
        {
            ActivateObject();

            StartCoroutine(PlaySummary());
        }

        public override void Init()
        {
            _returnMainMenuButton
                .onClick
                .AddListener(() => ServiceLocator.Global.GetService<GameManager>().LoadMainMenuSceneAsync());

            _totalMoney.HideInstant();
            _transportedPassengers.HideInstant();
            _gameTime.HideInstant();
            _stampObject.SetActive(false);
            _returnMainMenuButton.gameObject.SetActive(false);
        }

        public override void Dispose()
        {
            _returnMainMenuButton.onClick.RemoveAllListeners();
        }

        private IEnumerator PlaySummary()
        {
            GameStats gameStats = ServiceLocator.Global.GetService<GameStatsManager>().GameStats;
            if (gameStats == null)
            {
                LogX.LogError($"{nameof(GameStats)} is null!");

                yield break;
            }

            yield return _totalMoney.PlaySummary(gameStats.MoneyGained);
            yield return _transportedPassengers.PlaySummary(gameStats.TransportedPassengersCount);
            yield return _gameTime.PlaySummary(gameStats.GameElapsedTime);

            yield return PlayFinishSummary();
        }

        private IEnumerator PlayFinishSummary()
        {
            if (_finishSummaryCoroutine != null)
            {
                StopCoroutine(_finishSummaryCoroutine);
                _finishSummaryCoroutine = null;
            }

            _finishSummaryCoroutine = StartCoroutine(PlayFinishSummaryCoroutine());

            yield return _finishSummaryCoroutine;
        }

        private IEnumerator PlayFinishSummaryCoroutine()
        {
            _stampObject.SetActive(true);

            _stampObject.transform
                .DOScale(Vector3.one * _stampAnimationFinishScale, _stampAnimationDuration)
                .From(Vector3.one * _stampAnimationStartScale)
                .SetEase(_stampAnimationEase)
                .OnComplete(() =>
                {
                    ServiceLocator.Global.GetService<SoundManager>().PlaySound(_stampSoundData);
                    ServiceLocator.Global.GetService<SoundManager>().PlaySound(_stampFinishSoundData);
                    ShakeAllSummaryEntries();
                });

            yield return new WaitForSeconds(_stampAnimationDuration);

            // after all things done on the scene enable button
            _returnMainMenuButton.gameObject.SetActive(true);
            EventSystem.current.SetSelectedGameObject(_returnMainMenuButton.gameObject);
        }

        private void ShakeAllSummaryEntries()
        {
            _totalMoney.transform.DOShakePosition(_shakeDuration, _shakeStrength, _shakeVibrato, _shakeRandomness);
            _transportedPassengers.transform.DOShakePosition(_shakeDuration, _shakeStrength, _shakeVibrato, _shakeRandomness);
            _gameTime.transform.DOShakePosition(_shakeDuration, _shakeStrength, _shakeVibrato, _shakeRandomness);
        }
    }
}
