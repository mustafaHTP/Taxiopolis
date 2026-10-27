using UnityEngine;

namespace Game.UI.Common
{
    /// <summary>
    /// Base class for UI elements that control their <see cref="GameObject"/> active state
    /// and provide optional input navigation support (a default first-selected element).
    /// </summary>
    public abstract class BaseUIElement : MonoBehaviour
    {
        [SerializeField]
        private GameObject _firstSelected;

        [SerializeField]
        [Tooltip("If true, the UI will be hidden when a cancel/back action is performed; otherwise the cancel action is ignored. Helps gamepad navigation.")]
        private bool _canBeHiddenOnCancel;

        /// <summary>
        /// The default <see cref="GameObject"/> that should be selected when this UI element is shown.
        /// Can be null if there is no default selection.
        /// </summary>
        public GameObject FirstSelected => _firstSelected;

        /// <summary>
        /// Indicates whether this UI element should hide itself in response to a cancel/back action.
        /// </summary>
        public bool CanBeHiddenOnCancel => _canBeHiddenOnCancel;

        public virtual void Show()
        {
            ActivateObject();
        }

        public virtual void ShowInstant()
        {
            ActivateObject();
        }

        public virtual void Hide()
        {
            DeactivateObject();
        }

        public virtual void HideInstant()
        {
            DeactivateObject();
        }

        public virtual void Init() { }
        public virtual void Dispose() { }

        protected void ActivateObject()
        {
            gameObject.SetActive(true);
        }

        protected void DeactivateObject()
        {
            gameObject.SetActive(false);
        }
    }
}