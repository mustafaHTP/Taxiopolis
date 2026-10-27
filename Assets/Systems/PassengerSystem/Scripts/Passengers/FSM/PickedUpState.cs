using UnityEngine;

namespace Game.Systems.PassengerSystem.Passengers.FSM
{
    public class PickedUpState : BaseState
    {
        public PickedUpState(Passenger passenger, string name) : base(passenger, name)
        {
        }

        public override void OnEnter()
        {
            base.OnEnter();

            _passenger.NotifyingCar.GetIn(_passenger);

            _passenger.PassengerMovementHandler.DisableAgent();
            _passenger.Collider.enabled = false;
            _passenger.MeshesGameObject.SetActive(false);
            _passenger.transform.SetParent(_passenger.NotifyingCar.transform);
            _passenger.transform.localPosition = Vector3.zero;
        }
    }
}
