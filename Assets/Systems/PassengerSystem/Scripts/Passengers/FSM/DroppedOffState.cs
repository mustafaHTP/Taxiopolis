using Game.Systems.EmoteSystem;

namespace Game.Systems.PassengerSystem.Passengers.FSM
{
    public class DroppedOffState : BaseState
    {
        public DroppedOffState(Passenger passenger, string name) : base(passenger, name)
        {
        }

        public override void OnEnter()
        {
            base.OnEnter();

            _passenger.PassengerMovementHandler.DisableAgent();
            _passenger.MeshesGameObject.SetActive(true);
            _passenger.transform.SetParent(null);
            _passenger.transform.SetPositionAndRotation(
                _passenger.DestinationPoint.transform.position,
                _passenger.DestinationPoint.transform.rotation);

            _passenger.PassengerAnimationHandler.Play(_passenger.PassengerAnimationHandler.IdleAnimationHash);

            _passenger.EmoteController.SetEmote(EmoteType.FaceHappy);
            _passenger.EmoteController.Show();
        }

        public override void OnExit()
        {
            base.OnExit();

            _passenger.EmoteController.Hide();
        }
    }
}
