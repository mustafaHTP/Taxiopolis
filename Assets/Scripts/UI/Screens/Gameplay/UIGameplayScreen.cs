using DG.Tweening;
using Game.UI.Common;
using Game.Utils;
using UnityEngine;

namespace Game.UI.Screens.Gameplay
{
    public class UIGameplayScreen : UIScreen
    {
        [Header("Sections")]
        [SerializeField] private AbilityPanel _abilityPanel;
        [SerializeField] private ComboPanel _comboPanel;
        [SerializeField] private FarePanel _farePanel;
        [SerializeField] private MoneyPanel _moneyPanel;
        [SerializeField] private PromptPanel _promptPanel;
        [SerializeField] private TimePanel _timePanel;
        [SerializeField] private JobResultPanel _jobResultPanel;
        [SerializeField] private FareBonusRewardPanel _fareBonusRewardPanel;
        [SerializeField] private Canvas _mobileControlsCanvas;

        public override void Init()
        {
            _abilityPanel.Init();
            _comboPanel.Init();
            _farePanel.Init();
            _moneyPanel.Init();
            _promptPanel.Init();
            _timePanel.Init();
            _jobResultPanel.Init();
            _fareBonusRewardPanel.Init();
        }

        public override void Dispose()
        {
            _abilityPanel.Dispose();
            _farePanel.Dispose();
            _comboPanel.Dispose();
            _moneyPanel.Dispose();
            _promptPanel.Dispose();
            _timePanel.Dispose();
            _jobResultPanel.Dispose();
            _fareBonusRewardPanel.Dispose();
        }

        public override void Show()
        {
            ActivateObject();

            _abilityPanel.Show();
            _comboPanel.Show();
            _jobResultPanel.Show();
            _moneyPanel.Show();
            _timePanel.Show();
            _promptPanel.Show();
            _farePanel.Show();
            _fareBonusRewardPanel.Show();

            if (PlatformUtils.GetPlatform() == CustomPlatform.Mobile)
            {
                _mobileControlsCanvas.gameObject.SetActive(true);
            }
        }

        public override void Hide()
        {
            _abilityPanel.Hide();
            _comboPanel.Hide();
            _farePanel.Hide();
            _moneyPanel.Hide();
            _promptPanel.Hide();
            _timePanel.Hide();
            _jobResultPanel.Hide();
            _fareBonusRewardPanel.Hide();

            if (PlatformUtils.GetPlatform() == CustomPlatform.Mobile)
            {
                _mobileControlsCanvas.gameObject.SetActive(false);
            }

            DeactivateObject();
        }
    }
}