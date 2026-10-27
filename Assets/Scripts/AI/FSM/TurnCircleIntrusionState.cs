namespace Game.AI.FSM
{
    public class TurnCircleIntrusionState : BaseState
    {
        public TurnCircleIntrusionState(string name, CarAIContext carAIContext) : base(name, carAIContext)
        {
        }

        public override void OnUpdate()
        {
            base.OnUpdate();

            float gasInput = 0f;
            float brakeInput = 0.5f;

            float steerInput =
                _carAIContext.IsInsideLeftTurningCircle
                ? 1f
                : -1f;

            _carAIContext.CarController.UseBrakeToGoBack = true;

            _carAIContext.CarAIHandler.SetInputs(gasInput, brakeInput, steerInput);
        }
    }
}
