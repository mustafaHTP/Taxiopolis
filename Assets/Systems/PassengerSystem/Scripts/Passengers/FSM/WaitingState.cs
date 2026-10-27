using Game.Systems.EmoteSystem;

namespace Game.Systems.PassengerSystem.Passengers.FSM
{
    public class WaitingState : BaseState
    {
        public WaitingState(Passenger passenger, string name) : base(passenger, name)
        {
        }

        public override void OnEnter()
        {
            base.OnEnter();

            _passenger.ResetNotificationState();

            _passenger.PassengerMovementHandler.Stop();
            _passenger.PassengerAnimationHandler.Play(_passenger.PassengerAnimationHandler.IdleAnimationHash);
            _passenger.EmoteController.SetEmote(EmoteType.Cash);
            _passenger.EmoteController.Show();
        }

        public override void OnExit()
        {
            base.OnExit();

            _passenger.EmoteController.Hide();
        }
    }
}
