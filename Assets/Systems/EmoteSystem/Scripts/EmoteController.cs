using DG.Tweening;
using Game.Utils;
using UnityEngine;
using UnityEngine.UI;

namespace Game.Systems.EmoteSystem
{
    public class EmoteController : MonoBehaviour
    {
        [SerializeField] private bool _hideOnAwake = true;
        [SerializeField] private Image _emoteImage;
        [SerializeField] private EmoteLibrary _emoteLibrary;

        public void Show()
        {
            _emoteImage
                .DOFade(1f, 1f)
                .SetEase(Ease.OutQuint);
            _emoteImage.transform
                .DOScale(Vector3.one, 1f)
                .SetEase(Ease.OutBounce);
        }

        public void Hide()
        {
            _emoteImage
                .DOFade(0f, 1f)
                .SetEase(Ease.InQuint);
            _emoteImage.transform
                .DOScale(Vector3.zero, 1f)
                .SetEase(Ease.InBack);
        }

        public void SetEmote(EmoteType emoteType)
        {
            if (_emoteLibrary == null)
            {
                LogX.LogError("EmoteLibrary is not assigned!", this);

                return;
            }

            if (_emoteImage == null)
            {
                LogX.LogError("Emote Image is not assigned!", this);

                return;
            }

            EmoteLibrary.EmoteEntry emoteEntry = _emoteLibrary.GetEmoteEntry(emoteType);
            if (emoteEntry == null)
            {
                LogX.LogWarning($"EmoteEntry for EmoteType {emoteType} not found!", this);

                return;
            }

            _emoteImage.sprite = emoteEntry.EmoteSprite;
        }

        public void Clear()
        {
            if (_emoteImage == null)
            {
                LogX.LogError("Emote Image is not assigned!", this);

                return;
            }

            _emoteImage.sprite = null;
        }

        public void ShowInstant()
        {
            if (_emoteImage == null)
            {
                LogX.LogError("Emote Image is not assigned!", this);

                return;
            }

            _emoteImage.color = new Color(
                _emoteImage.color.r,
                _emoteImage.color.g,
                _emoteImage.color.b,
                1f);

            _emoteImage.transform.localScale = Vector3.one;
        }

        public void HideInstant()
        {
            if (_emoteImage == null)
            {
                LogX.LogError("Emote Image is not assigned!", this);

                return;
            }

            _emoteImage.color = new Color(
                _emoteImage.color.r,
                _emoteImage.color.g,
                _emoteImage.color.b,
                0f);

            _emoteImage.transform.localScale = Vector3.zero;
        }

        private void Awake()
        {
            if (_hideOnAwake)
            {
                HideInstant();
            }
        }
    }
}
