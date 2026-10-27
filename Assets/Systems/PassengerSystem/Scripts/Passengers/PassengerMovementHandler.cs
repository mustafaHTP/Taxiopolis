using UnityEngine;
using UnityEngine.AI;

namespace Game.Systems.PassengerSystem.Passengers
{
    public class PassengerMovementHandler : MonoBehaviour
    {
        private NavMeshAgent _agent;

        public void EnableAgent() => _agent.enabled = true;

        public void DisableAgent() => _agent.enabled = false;

        public void MoveTo(Vector3 position)
        {
            if (!_agent.enabled) return;

            _agent.SetDestination(position);
        }

        public void Stop()
        {
            if (!_agent.enabled) return;

            _agent.ResetPath();
        }

        private void Awake()
        {
            _agent = GetComponent<NavMeshAgent>();
        }

        //private void OnEnable()
        //{
        //    _agent.enabled = true;
        //}

        //private void OnDisable()
        //{
        //    _agent.enabled = false;
        //}
    }
}
