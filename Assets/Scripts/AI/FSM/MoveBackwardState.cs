namespace Game.AI.FSM
{
    public class MoveBackwardState : BaseState
    {
        public MoveBackwardState(string name, CarAIContext carAIContext) : base(name, carAIContext)
        {
        }

        public override void OnUpdate()
        {
            base.OnUpdate();

            float gasInput = 0f;
            float brakeInput = 1f;

            float steerInput = _carAIContext.RightDotResult * -1f;

            _carAIContext.CarController.UseBrakeToGoBack = true;

            _carAIContext.CarAIHandler.SetInputs(gasInput, brakeInput, steerInput);
        }
    }
}
