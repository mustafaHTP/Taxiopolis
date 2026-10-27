using Coffee.UIEffects;
using DG.Tweening;
using Framework.Services;
using Game.Car;
using Game.Common;
using Game.Systems.AbilitySystem;
using Game.UI.Common;
using System;
using UnityEngine;
using UnityEngine.UI;

namespace Game.UI.Screens.Gameplay
{
    public class SpeedbreakerAbilityView : BaseUIElement
    {
        [SerializeField] private Image _abilityImage;
        [SerializeField] private UIEffect _abilityMaskUIEffect;
        [SerializeField] private SpeedBreakerAbility _ability;
        [SerializeField] private float _abilityImageScaleOnPerform = 1.2f;
        [SerializeField] private float _abilityImageAnimDurationOnPerform = 0.5f;

        private Vector3 _initialImageScale;
        private Tween _abilityImageScaleTween;

        public override void Init()
        {
            _initialImageScale = _abilityImage.rectTransform.localScale;

            _ability = (SpeedBreakerAbility)ServiceLocator
                .Scene
                .GetService<Player>()
                .GetComponent<CarAbilityHandler>()
                .GetAbility(AbilityType.Speedbreaker);

            _ability.OnCooldownStarted += Ability_OnCooldownStarted;
            _ability.OnCooldownFinished += Ability_OnCooldownFinished;
            _ability.OnActivation += Ability_OnPerformed;
            _ability.OnDeactivation += Ability_OnCanceled;
        }

        public override void Dispose()
        {
            _ability.OnCooldownStarted -= Ability_OnCooldownStarted;
            _ability.OnCooldownFinished -= Ability_OnCooldownFinished;
            _ability.OnActivation -= Ability_OnPerformed;
            _ability.OnDeactivation -= Ability_OnCanceled;
        }

        private void Ability_OnCanceled()
        {
            _abilityMaskUIEffect.toneIntensity = 1;
            _abilityMaskUIEffect.gradationIntensity = 1;

            _abilityImageScaleTween.Kill();
            _abilityImage.rectTransform.localScale = _initialImageScale;
        }

        private void Ability_OnPerformed()
        {
            _abilityMaskUIEffect.toneIntensity = 0;
            _abilityMaskUIEffect.gradationIntensity = 1;

            _abilityImageScaleTween = _abilityImage.transform
                .DOScale(_abilityImageScaleOnPerform, _abilityImageAnimDurationOnPerform)
                .SetLoops(-1, LoopType.Yoyo)
                .SetUpdate(true);
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

        private void Update()
        {
            _abilityImage.fillAmount = _ability.GetAmountNormalized();
        }
    }
}
