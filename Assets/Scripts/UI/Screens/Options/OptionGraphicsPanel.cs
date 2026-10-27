using Framework.Services;
using Game.Settings;
using Game.UI.Common;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Game.UI.Screens.Options
{
    public class OptionGraphicsPanel : UIPanel
    {
        [SerializeField] private TMP_Dropdown _resolutionDropdown;
        [SerializeField] private Toggle _vsyncToggle;

        public override void Show()
        {
            InitResolutionDropdown();
            InitVsyncToggle();

            base.Show();
        }

        private void InitVsyncToggle()
        {
            _vsyncToggle.isOn = ServiceLocator
                .Global.GetService<GameSettingsManager>()
                .Settings.IsVsyncOn;
        }

        private void InitResolutionDropdown()
        {
            _resolutionDropdown.ClearOptions();

            List<Vector2Int> availableResolutions = ServiceLocator
                .Global.GetService<GameSettingsManager>()
                .Settings.AvailableResolutions;

            for (int i = 0; i < availableResolutions.Count; i++)
            {
                float x = availableResolutions[i].x;
                float y = availableResolutions[i].y;
                string dropdownText = $"{x} x {y}";

                _resolutionDropdown.options.Add(new TMP_Dropdown.OptionData(dropdownText));
            }

            // Set the dropdown's value to the current resolution index
            Vector2Int currentResolution = ServiceLocator
                .Global.GetService<GameSettingsManager>()
                .Settings.Resolution;

            int currentResolutionIndex = availableResolutions.IndexOf(currentResolution);
            if (currentResolutionIndex >= 0)
            {
                _resolutionDropdown.value = currentResolutionIndex;
            }
            else
            {
                Debug.LogWarning("Current resolution not found in available resolutions list.");
                _resolutionDropdown.value = 0; // Default to the first option if current resolution is not found
            }

            _resolutionDropdown.RefreshShownValue();
        }

        private void Awake()
        {
            _resolutionDropdown.onValueChanged.AddListener(OnResolutionDropdownValueChanged);
            _vsyncToggle.onValueChanged.AddListener(OnVsyncToggled);
        }

        private void OnVsyncToggled(bool isOn)
        {
            ServiceLocator
                .Global.GetService<GameSettingsManager>()
                .Settings.IsVsyncOn = isOn;
        }

        private void OnResolutionDropdownValueChanged(int value)
        {
            List<Vector2Int> availableResolutions = ServiceLocator
                .Global.GetService<GameSettingsManager>()
                .Settings.AvailableResolutions;
            Vector2Int selectedResolution = availableResolutions[value];

            ServiceLocator
                .Global.GetService<GameSettingsManager>()
                .Settings.Resolution = selectedResolution;
        }
    }
}
