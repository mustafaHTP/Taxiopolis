using UnityEngine;

namespace Game.Systems.PassengerSystem.Passengers.FSM
{
    public class ChaseUpState : BaseState
    {
        public ChaseUpState(Passenger passenger, string name) : base(passenger, name)
        {
        }

        public override void OnEnter()
        {
            base.OnEnter();

            _passenger.PassengerMovementHandler.EnableAgent();
            _passenger.PassengerAnimationHandler.Play(_passenger.PassengerAnimationHandler.WalkAnimationHash);
        }

        public override void OnUpdate()
        {
            base.OnUpdate();

            Transform closestEntryPoint = _passenger.GetClosestEntryPointOfCar();
            if (closestEntryPoint == null) return;

            _passenger.PassengerMovementHandler.MoveTo(closestEntryPoint.position);
        }

        override public void OnExit()
        {
            base.OnExit();

            _passenger.PassengerMovementHandler.DisableAgent();
        }
    }
}
