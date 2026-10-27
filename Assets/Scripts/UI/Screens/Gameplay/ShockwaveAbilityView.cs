using Coffee.UIEffects;
using DG.Tweening;
using Framework.Services;
using Game.Car;
using Game.Common;
using Game.Systems.AbilitySystem;
using Game.UI.Common;
using UnityEngine;
using UnityEngine.UI;

namespace Game.UI.Screens.Gameplay
{
    public class ShockwaveAbilityView : BaseUIElement
    {
        [SerializeField] private Image _abilityImage;
        [SerializeField] private UIEffect _abilityMaskUIEffect;
        [SerializeField] private ShockwaveAbility _ability;
        [SerializeField] private float _abilityImageScaleOnPerform = 1.2f;
        [SerializeField] private float _abilityImageAnimDurationOnPerform = 0.5f;

        private Vector3 _initialImageScale;
        private Tween _abilityImageScaleTween;

        public override void Init()
        {
            _initialImageScale = _abilityImage.rectTransform.localScale;

            _ability = (ShockwaveAbility)ServiceLocator
                .Scene
                .GetService<Player>()
                .GetComponent<CarAbilityHandler>()
                .GetAbility(AbilityType.Shockwave);

            _ability.OnCooldownStarted += Ability_OnCooldownStarted;
            _ability.OnCooldownFinished += Ability_OnCooldownFinished;
            _ability.OnActivation += Ability_OnPerformed;
        }

        public override void Dispose()
        {
            _ability.OnCooldownStarted -= Ability_OnCooldownStarted;
            _ability.OnCooldownFinished -= Ability_OnCooldownFinished;
            _ability.OnActivation -= Ability_OnPerformed;
        }

        private void Ability_OnPerformed()
        {
            _abilityMaskUIEffect.toneIntensity = 0;
            _abilityMaskUIEffect.gradationIntensity = 1;

            _abilityImageScaleTween = _abilityImage.transform
                .DOScale(_abilityImageScaleOnPerform, _abilityImageAnimDurationOnPerform);
        }

        private void Ability_OnCooldownStarted()
        {
            _abilityMaskUIEffect.toneIntensity = 1;
            _abilityMaskUIEffect.gradationIntensity = 0;

            _abilityImageScaleTween.Kill();
        }

        private void Ability_OnCooldownFinished()
        {
            _abilityMaskUIEffect.toneIntensity = 1;
            _abilityMaskUIEffect.gradationIntensity = 1;

            _abilityImageScaleTween.Kill();
        }
    }
}
