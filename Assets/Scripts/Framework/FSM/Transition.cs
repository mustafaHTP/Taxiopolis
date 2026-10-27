namespace Framework.FSM
{
    public class Transition : ITransition
    {
        public Transition(IState to, IPredicate predicate)
        {
            To = to;
            Predicate = predicate;
        }

        public IState To { get; }
        public IPredicate Predicate { get; }
    }
}
