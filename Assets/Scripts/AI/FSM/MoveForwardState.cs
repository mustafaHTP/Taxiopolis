namespace Game.AI.FSM
{
    public class MoveForwardState : BaseState
    {
        public MoveForwardState(string name, CarAIContext carAIContext) : base(name, carAIContext)
        {
        }

        public override void OnUpdate()
        {
            base.OnUpdate();

            float gasInput = 1f;
            float brakeInput = 0f;
            float steerInput = _carAIContext.RightDotResult;

            _carAIContext.CarController.UseBrakeToGoBack = false;

            _carAIContext.CarAIHandler.SetInputs(gasInput, brakeInput, steerInput);
        }
    }
}
