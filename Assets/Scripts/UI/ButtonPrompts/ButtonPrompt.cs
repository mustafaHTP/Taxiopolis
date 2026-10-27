using Framework.Services;
using Game.Input;
using Game.Managers;
using Game.Utils;
using System;
using UnityEngine;
using UnityEngine.UI;

namespace Game.UI.ButtonPrompts
{
    public class ButtonPrompt : MonoBehaviour
    {
        [SerializeField] private Image _image;
        [SerializeField] private ButtonPromptData[] _buttonPromptDataArray;

        private void Awake()
        {
            if (_buttonPromptDataArray == null || _buttonPromptDataArray.Length == 0)
            {
                LogX.LogError("ButtonPromptData array is null or empty.", this);

                return;
            }

            // Use first element default
            _image.sprite = _buttonPromptDataArray[0].ButtonSprite;
        }

        private void OnEnable()
        {
            ServiceLocator.Global.GetService<InputManager>().OnDeviceChanged += InputManager_OnDeviceChanged;

            // Update Button Prompt Sprite on enable
            UpdateButtonPromptSprite(
                ServiceLocator.Global.GetService<InputManager>().CurrentCustomDeviceType);
        }

        private void OnDisable()
        {
            ServiceLocator.Global.GetService<InputManager>().OnDeviceChanged -= InputManager_OnDeviceChanged;
        }

        private Sprite GetButtonSprite(CustomDeviceType customDeviceType)
        {
            if (_buttonPromptDataArray == null || _buttonPromptDataArray.Length == 0)
            {
                LogX.LogError("ButtonPromptData array is null or empty.", this);
                return null;
            }

            for (int i = 0; i < _buttonPromptDataArray.Length; i++)
            {
                if (_buttonPromptDataArray[i].CustomDeviceType == customDeviceType)
                {
                    return _buttonPromptDataArray[i].ButtonSprite;
                }
            }

            return null;
        }

        private void InputManager_OnDeviceChanged(CustomDeviceType customDeviceType, bool isMouseDetected)
        {
            UpdateButtonPromptSprite(customDeviceType);
        }

        private void UpdateButtonPromptSprite(CustomDeviceType customDeviceType)
        {
            Sprite buttonSprite = GetButtonSprite(customDeviceType);
            if (buttonSprite == null)
            {
                LogX.LogError($"No ButtonPromptData found for device type '{customDeviceType}'.", this);

                return;
            }

            _image.sprite = buttonSprite;
        }
    }

    [Serializable]
    public class ButtonPromptData
    {
        [SerializeField] private CustomDeviceType _customDeviceType;
        [SerializeField] private Sprite _buttonSprite;

        public CustomDeviceType CustomDeviceType => _customDeviceType;
        public Sprite ButtonSprite => _buttonSprite;
    }
}
