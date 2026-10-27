using Framework.Services;
using Game.Input;
using Game.Managers;
using Game.Systems.SoundSystem;
using Game.UI.Common;
using Game.UI.Screens.Loading;
using Game.UI.Screens.SceneTransition;
using Game.Utils;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace Game.UI
{
    public class UIManager : MonoBehaviour, IGlobalService
    {
        [Header("UI References")]
        [SerializeField] private UILoadingScreen _loadingScreen;
        [SerializeField] private UISceneTransitionScreen _sceneTransitionScreen;
        [SerializeField] private SoundData _confirmSound;
        [SerializeField] private SoundData _cancelSound;
        [SerializeField] private SoundData _navigateSound;

        [Header("Scene Transition Settings")]
        [SerializeField] private float _fadeDuration = 1f;

        private readonly Stack<BaseUIElement> _uiStack = new();

        private Coroutine _currentFadeCoroutine;

        public void Init()
        {
            _sceneTransitionScreen.Show();
        }

        public void AfterInit()
        {
            ServiceLocator.Global.GetService<InputManager>().OnDeviceChanged += InputManager_OnDeviceChanged;
            ServiceLocator.Global.GetService<InputManager>()
                .GameInput
                .UI
                .Navigate
                .performed += InputManager_UI_NavigatePerformed;
            ServiceLocator.Global.GetService<InputManager>()
                .GameInput
                .UI
                .Cancel
                .performed += InputManager_UI_Cancel_Performed;
            ServiceLocator.Global.GetService<InputManager>()
                .GameInput
                .UI
                .Submit
                .performed += InputManager_UI_Submit_Performed;
        }

        public void Dispose()
        {
            ServiceLocator.Global.GetService<InputManager>().OnDeviceChanged -= InputManager_OnDeviceChanged;
            ServiceLocator.Global.GetService<InputManager>().GameInput.UI.Cancel.performed -= InputManager_UI_Cancel_Performed;
            ServiceLocator.Global.GetService<InputManager>().GameInput.UI.Navigate.performed -= InputManager_UI_NavigatePerformed;
            ServiceLocator.Global.GetService<InputManager>().GameInput.UI.Submit.performed -= InputManager_UI_Submit_Performed;
        }

        public void PushUI(BaseUIElement uiElement)
        {
            if (uiElement == null)
            {
                LogX.LogWarning("UI element is null");

                return;
            }

            if (_uiStack.Count > 0)
            {
                var topUI = _uiStack.Peek();
                topUI.Hide();
            }

            _uiStack.Push(uiElement);
            uiElement.Show();

            SetSelectedGameObject(uiElement.FirstSelected);
        }

        public void PopUI()
        {
            if (_uiStack.Count == 0) return;

            var topUI = _uiStack.Pop();
            topUI.Hide();

            // play sound here

            if (_uiStack.Count > 0)
            {
                var nextUI = _uiStack.Peek();
                nextUI.Show();

                SetSelectedGameObject(nextUI.FirstSelected);
            }
        }

        public void ClearUIStack()
        {
            while (_uiStack.Count > 0)
            {
                var uiElement = _uiStack.Pop();
                uiElement.Hide();
            }
        }

        public IEnumerator StartFadeIn()
        {
            if (_currentFadeCoroutine != null)
            {
                StopCoroutine(_currentFadeCoroutine);
                _currentFadeCoroutine = null;
            }

            _currentFadeCoroutine = _sceneTransitionScreen
                .StartCoroutine(
                _sceneTransitionScreen
                .StartFade(true, _fadeDuration));

            yield return _currentFadeCoroutine;
        }

        public IEnumerator StartFadeOut()
        {
            if (_currentFadeCoroutine != null)
            {
                StopCoroutine(_currentFadeCoroutine);
                _currentFadeCoroutine = null;
            }

            _currentFadeCoroutine = StartCoroutine(
                _sceneTransitionScreen
                .StartFade(false, _fadeDuration));

            yield return _currentFadeCoroutine;
        }

        public void ShowLoadingScreen()
        {
            _loadingScreen.Show();
        }

        public void HideLoadingScreen()
        {
            _loadingScreen.Hide();
        }

        private void SetSelectedGameObject(GameObject selectedObject)
        {
            if (EventSystem.current == null) return;
            if (selectedObject == null) return;

            EventSystem.current.SetSelectedGameObject(null);
            EventSystem.current.SetSelectedGameObject(selectedObject);
        }

        #region Handing Events

        private void InputManager_UI_Submit_Performed(InputAction.CallbackContext context)
        {
            ServiceLocator
                .Global.GetService<SoundManager>()
                .PlaySound(_confirmSound);
        }

        private void InputManager_UI_NavigatePerformed(InputAction.CallbackContext context)
        {
            ServiceLocator
                .Global.GetService<SoundManager>()
                .PlaySound(_navigateSound);
        }

        private void InputManager_UI_Cancel_Performed(InputAction.CallbackContext context)
        {
            // When cancel is performed, pop the current UI
            if (_uiStack.Count == 0) return;

            var topUI = _uiStack.Peek();
            if (topUI.CanBeHiddenOnCancel)
            {
                PopUI();
                ServiceLocator
                    .Global.GetService<SoundManager>()
                    .PlaySound(_cancelSound);
            }
        }

        private void InputManager_OnDeviceChanged(CustomDeviceType customDeviceType, bool isMouseDetected)
        {
            // ? clear current selection when mouse detected
            // if (isMouseDetected)
            // {
            //     EventSystem.current.SetSelectedGameObject(null);

            //     return;
            // }

            // check there is already selected item so do not select
            bool hasAlreadySelectedGameObject = EventSystem.current.currentSelectedGameObject != null;
            // if there is no item so select item based on top of ui stack
            if (!hasAlreadySelectedGameObject)
            {
                if (_uiStack.Count > 0)
                {
                    var topUI = _uiStack.Peek();
                    SetSelectedGameObject(topUI.FirstSelected);
                }
            }
        }
        #endregion
    }
}
