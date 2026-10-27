using System.Collections.Generic;
using UnityEngine;

namespace RoadSystem
{
    /// <summary>
    /// Simple container for road points.
    /// </summary>
    public class Road : MonoBehaviour
    {
        [SerializeField]
        [Tooltip("DO NOT Manipulate manually. Use Editor Tools! If you face errors use Refresh Road Points On Tools Window")]
        private List<RoadPoint> _roadPoints;

        public List<RoadPoint> RoadPoints => _roadPoints ??= new();
    }
}
