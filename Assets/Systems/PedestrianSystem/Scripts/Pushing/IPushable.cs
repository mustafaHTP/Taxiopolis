using UnityEngine;

namespace Game.Systems.PedestrianSystem.Pushing
{
    public interface IPushable
    {
        void Push(Vector3 pushVector);
    }
}
