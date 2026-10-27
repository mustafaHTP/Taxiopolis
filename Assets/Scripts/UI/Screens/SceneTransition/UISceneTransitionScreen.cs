using Game.UI.Common;
using System.Collections;
using UnityEngine;

namespace Game.UI.Screens.SceneTransition
{
    public class UISceneTransitionScreen : UIScreen
    {
        [SerializeField] private CanvasGroup _canvasGroup;

        private const float FadeInTargetAlpha = 1f;
        private const float FadeOutTargetAlpha = 0f;

        public IEnumerator StartFade(bool isFadeIn, float fadeDuration)
        {
            // Make sure target alpha is between 0 and 1
            float targetAlpha = isFadeIn ? FadeInTargetAlpha : FadeOutTargetAlpha;
            float startAlpha = isFadeIn ? FadeOutTargetAlpha : FadeInTargetAlpha;
            float elapsedTime = 0f;

            _canvasGroup.alpha = startAlpha;

            while (elapsedTime < fadeDuration)
            {
                elapsedTime += Time.unscaledDeltaTime;
                _canvasGroup.alpha = Mathf.Lerp(
                    _canvasGroup.alpha,
                    targetAlpha,
                    elapsedTime / fadeDuration);

                yield return null;
            }

            _canvasGroup.alpha = targetAlpha;
        }

        // no need to register in scene, because it is under global ui manager
        public override void RegisterInScene()
        {
            // noop
        }
    }
}
