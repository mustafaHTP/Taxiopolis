using Game.Car;

namespace Game.AI
{
    public class CarAIContext
    {
        public CarAIContext(CarController carController, CarAIHandler carAIHandler)
        {
            CarController = carController;
            CarAIHandler = carAIHandler;
        }

        public CarController CarController { get; }
        public CarAIHandler CarAIHandler { get; }

        public float ForwardDotResult { get; set; }
        public float RightDotResult { get; set; }
        public bool IsOutSideLeftTurningCircle { get; set; }
        public bool IsOutSideRightTurningCircle { get; set; }
        public bool IsInsideLeftTurningCircle { get; set; }
        public bool IsInsideRightTurningCircle { get; set; }
        public bool IsTrafficLightStop { get; set; }
        public bool IsTrafficBlock { get; set; }
    }
}
