using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Game.Systems.PedestrianSystem
{
    public class PedestrianPoint : MonoBehaviour
    {
        public PedestrianPoint Previous;
        public PedestrianPoint Next;
        [Range(0f, 1f)]
        public float BranchProbability;
        public List<PedestrianPoint> Branches = new();

        [SerializeField] private float _pedestrianSpawnCooldownInSeconds = 0.5f;
        [SerializeField]
        [Range(0f, 5f)]
        private float _width = 1.0f;

        public float Width => _width;
        public float NextSpawnTime { get; private set; }

        public void InformSpawn()
        {
            NextSpawnTime = Time.time + _pedestrianSpawnCooldownInSeconds;
        }

        public Vector3 GetRandomPosition()
        {
            Vector3 leftBound = transform.position - (transform.right * (_width / 2f));
            Vector3 rightBound = transform.position + (transform.right * (_width / 2f));

            return Vector3.Lerp(leftBound, rightBound, Random.value);
        }
    }
}
