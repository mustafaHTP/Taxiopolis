namespace Framework.FSM
{
    /// <summary>
    /// Transition takes care of moving from one state to another based on a predicate.
    /// </summary>
    public interface ITransition
    {
        IState To { get; }
        IPredicate Predicate { get; }
    }
}
