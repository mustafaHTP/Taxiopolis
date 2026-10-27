namespace Framework.FSM
{
    /// <summary>
    /// Interface for defining predicates used in finite state machines (FSM).
    /// Predicates are conditions that determine transitions between states.
    /// </summary>

    public interface IPredicate
    {
        bool Evaluate();
    }
}
