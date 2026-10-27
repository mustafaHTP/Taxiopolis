using UnityEngine;

namespace Game.Common
{
    [RequireComponent(typeof(Animator))]
    public class DummyNpc : MonoBehaviour
    {
        [SerializeField] private string _animationName;
        [SerializeField] private float _normalizedtransitionDuration;
        [SerializeField] private Animator _animator;

        private void Start()
        {
            _animator.CrossFade(_animationName, _normalizedtransitionDuration);
        }
    }
}
