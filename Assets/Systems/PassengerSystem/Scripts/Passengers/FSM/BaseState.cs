using Framework.FSM;

namespace Game.Systems.PassengerSystem.Passengers.FSM
{
    public class BaseState : IState
    {
        protected readonly Passenger _passenger;

        public string Name { get; }

        public BaseState(Passenger passenger, string name)
        {
            _passenger = passenger;
            Name = name;
        }

        public virtual void OnEnter()
        {
            // noop
        }

        public virtual void OnExit()
        {
            // noop
        }

        public virtual void OnFixedUpdate()
        {
            // noop
        }

        public virtual void OnUpdate()
        {
            // noop
        }
    }
}
