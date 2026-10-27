using UnityEngine;

namespace Game.Systems.PassengerSystem.Passengers
{
    public class PassengerAnimationHandler : MonoBehaviour
    {
        private const string _idleAnimationName = "idle";
        private const string _walkAnimationName = "walk";

        public readonly int IdleAnimationHash = Animator.StringToHash(_idleAnimationName);
        public readonly int WalkAnimationHash = Animator.StringToHash(_walkAnimationName);

        [SerializeField]
        [Range(0f, 1f)]
        private float _normalizedTransitionTime = 0.1f;

        private Animator _animator;

        public void Play(int animationHash)
        {
            _animator.CrossFade(animationHash, _normalizedTransitionTime);
        }

        private void Awake()
        {
            _animator = GetComponentInChildren<Animator>();
        }
    }
}
