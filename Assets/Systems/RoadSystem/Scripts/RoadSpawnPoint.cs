using RoadSystem;
using UnityEngine;

[RequireComponent(typeof(RoadPoint))]
public class RoadSpawnPoint : MonoBehaviour
{
    public RoadPoint RoadPoint
    {
        get
        {
            if (!TryGetComponent<RoadPoint>(out var roadPoint))
            {
                Debug.LogError($"{name} does not have a RoadPoint component.");
                return null;
            }
            return roadPoint;
        }
    }
}
