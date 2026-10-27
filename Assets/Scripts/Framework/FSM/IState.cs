namespace Framework.FSM
{

    public interface IState
    {
        string Name { get; }

        void OnEnter();
        void OnUpdate();
        void OnFixedUpdate();
        void OnExit();
    }
}