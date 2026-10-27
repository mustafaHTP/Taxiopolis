namespace Game.AI.FSM
{
    public class StopState : BaseState
    {
        public StopState(string name, CarAIContext carAIContext) : base(name, carAIContext)
        {
        }

        public override void OnUpdate()
        {
            base.OnUpdate();

            float gasInput = 0f;
            float brakeInput;
            float steerInput = 0f;

            // Stop strategy is a little different, because to stopping car completely
            // rigidbody drag is used and it is set too high value in CarController when target speed is about 0.
            if (_carAIContext.CarController.CarState.Speed < _carAIContext.CarController.StoppingDragSpeedThreshold)
            {
                brakeInput = 0f;
            }
            else
            {
                brakeInput = 1f;
            }

            _carAIContext.CarController.UseBrakeToGoBack = false;

            _carAIContext.CarAIHandler.SetInputs(gasInput, brakeInput, steerInput);
        }
    }
}
