namespace Game.Input
{
    public readonly struct FrameInput
    {
        public FrameInput(
            float gas,
            float brake,
            float steer,
            float handbrake)
        {
            Gas = gas;
            Brake = brake;
            Steer = steer;
            Handbrake = handbrake;
        }

        public float Gas { get; }
        public float Brake { get; }
        public float Steer { get; }
        public float Handbrake { get; }
    }
}
