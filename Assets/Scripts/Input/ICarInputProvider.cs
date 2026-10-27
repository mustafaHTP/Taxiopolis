using UnityEngine.InputSystem;

namespace Game.Input
{
    public interface ICarInputProvider
    {
        FrameInput FrameInput { get; }

        InputAction GasAction { get; }
        InputAction BrakeAction { get; }
        InputAction SteerAction { get; }
        InputAction HandbrakeAction { get; }
        InputAction HornAction { get; }
        InputAction UseNitroAction { get; }
        InputAction UseShockwaveAction { get; }
        InputAction UseSpeedbreakerAction { get; }
        InputAction RecoverAction { get; }

        InputAction FindAction(string actionMap, string actionName);
    }
}
