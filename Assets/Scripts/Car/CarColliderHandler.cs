using UnityEngine;

namespace Game.Car
{
    public class CarColliderHandler : MonoBehaviour
    {
        [SerializeField] private BoxCollider _bodyCollider;

        public BoxCollider BodyCollider => _bodyCollider;
    }
}
