using System.Collections;
using DG.Tweening;
using Framework.Services;
using Game.Stats;
using Game.Systems.SoundSystem;
using Game.Utils;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Game.UI.Screens.Summary
{
    public class SummaryEntry : MonoBehaviour
    {
        [Header("Options")]
        [SerializeField] private float _statCountUpTime = 3f;
        [SerializeField] private float _delayBetweenCountUps = 2f;
        [SerializeField] private float _animationStartScale = 1.3f;
        [SerializeField] private float _animationFinishScale = 1f;
        [SerializeField] private float _animationDuration = 1f;
        [SerializeField] private Ease _animationEase;

        [Header("References")]
        [SerializeField] private TextMeshProUGUI _prompText;
        [SerializeField] private TextMeshProUGUI _resultText;
        [SerializeField] private Image _image;
        [SerializeField] private SoundData _totalMoneyCountUpFinishedSoundData;
        [SerializeField] private SoundData _totalMoneyCountUpLoopSoundData;
        [SerializeField] private SoundData _statFailedSoundData;

        private Coroutine _summaryCoroutine;
        private RectTransform _rectTransform;

        public TextMeshProUGUI PrompText => _prompText;
        public TextMeshProUGUI ResultText => _resultText;
        public Image Image => _image;

        public void HideInstant()
        {
            _prompText.gameObject.SetActive(false);
            _resultText.gameObject.SetActive(false);
            _image.gameObject.SetActive(false);
        }

        public IEnumerator PlaySummary(int resultValue)
        {
            if (_summaryCoroutine != null)
            {
                StopCoroutine(_summaryCoroutine);
                _summaryCoroutine = null;
            }

            _summaryCoroutine = StartCoroutine(PlaySummaryCoroutine(resultValue));

            yield return _summaryCoroutine;
        }

        private IEnumerator CountUpCoroutine(TMP_Text text, int startValue, int endValue, float duration)
        {
            float elapsedTime = 0f;
            text.SetText(startValue.ToString());

            while (elapsedTime < duration)
            {
                elapsedTime += Time.deltaTime;
                float currentValue = Mathf.Lerp(startValue, endValue, elapsedTime / duration);
                currentValue = Mathf.RoundToInt(currentValue);
                text.SetText(currentValue.ToString());

                yield return null;
            }

            text.SetText(endValue.ToString());
        }

        private IEnumerator PlaySummaryCoroutine(int resultValue)
        {
            _rectTransform.DOKill();
            _rectTransform
                .DOScale(Vector3.one * _animationFinishScale, _animationDuration)
                .From(Vector3.one * _animationStartScale)
                .SetEase(_animationEase)
                .OnStart(() =>
                {
                    _prompText.gameObject.SetActive(true);
                    _resultText.gameObject.SetActive(true);
                    _image.gameObject.SetActive(true);
                });

            /*
            * Fail
            * Evaluete Smaller or equal values to zero as FAIL
            **/
            if (resultValue <= 0)
            {
                ServiceLocator
                    .Global.GetService<SoundManager>()
                    .PlaySound(_statFailedSoundData);

                _resultText.SetText("0");
            }
            // success
            else
            {
                AudioSource totalMoneyCountUpLoopAS = ServiceLocator
                    .Global.GetService<SoundManager>()
                    .PlaySound(_totalMoneyCountUpLoopSoundData);
                yield return CountUpCoroutine(
                    _resultText,
                    0,
                    resultValue,
                    _statCountUpTime);
                totalMoneyCountUpLoopAS.Stop();

                Destroy(totalMoneyCountUpLoopAS);
                ServiceLocator
                    .Global.GetService<SoundManager>()
                    .PlaySound(_totalMoneyCountUpFinishedSoundData);
            }

            yield return new WaitForSeconds(_delayBetweenCountUps);
        }

        private void Awake()
        {
            _rectTransform = GetComponent<RectTransform>();
        }
    }
}