using Framework.FSM;

namespace Game.AI.FSM
{
    public class BaseState : IState
    {
        protected CarAIContext _carAIContext;

        public BaseState(string name, CarAIContext carAIContext)
        {
            Name = name;
            _carAIContext = carAIContext;
        }

        public string Name { get; }

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

