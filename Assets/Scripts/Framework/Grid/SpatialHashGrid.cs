using System.Collections.Generic;
using UnityEngine;

namespace Framework.Grid
{
    /// <summary>
    /// Buckets items into square cells on the XZ plane so that "what is near this position?"
    /// only looks at nearby cells instead of every item. Y is ignored by the grid,
    /// so results are a coarse superset: do the exact distance check yourself.
    /// </summary>
    public class SpatialHashGrid<T>
    {
        private readonly float _cellSize;
        private readonly Dictionary<Vector2Int, List<T>> _grid = new();

        public SpatialHashGrid(float cellSize)
        {
            _cellSize = Mathf.Max(0.01f, cellSize);
        }

        public float CellSize => _cellSize;

        /// <summary>
        /// The one place that defines which cell a world position belongs to.
        /// Static so debug drawing can use the exact same math without building a grid.
        /// </summary>
        public static Vector2Int WorldToCell(Vector3 position, float cellSize)
        {
            return new Vector2Int(
                Mathf.FloorToInt(position.x / cellSize),
                Mathf.FloorToInt(position.z / cellSize));
        }

        public void Insert(Vector3 position, T item)
        {
            Vector2Int cell = WorldToCell(position, _cellSize);

            if (!_grid.TryGetValue(cell, out List<T> items))
            {
                items = new List<T>();
                _grid[cell] = items;
            }

            items.Add(item);
        }

        public void Clear()
        {
            _grid.Clear();
        }

        /// <summary>
        /// Fills <paramref name="results"/> (cleared first) with every item in the cells that
        /// overlap the square around <paramref name="center"/>. Works with any cell size.
        /// Pass a reused list so the query does not allocate.
        /// </summary>
        public void QueryRadius(Vector3 center, float radius, List<T> results)
        {
            results.Clear();

            Vector2Int min = WorldToCell(new Vector3(center.x - radius, 0f, center.z - radius), _cellSize);
            Vector2Int max = WorldToCell(new Vector3(center.x + radius, 0f, center.z + radius), _cellSize);

            for (int x = min.x; x <= max.x; x++)
            {
                for (int y = min.y; y <= max.y; y++)
                {
                    if (_grid.TryGetValue(new Vector2Int(x, y), out List<T> items))
                    {
                        results.AddRange(items);
                    }
                }
            }
        }

        /// <summary>
        /// Convenience version that allocates a new list. Fine for on-demand code
        /// (pathfinding), not for per-tick code. Use QueryRadius with a reused list there.
        /// </summary>
        public List<T> QueryNeighbours(Vector3 position, float radius)
        {
            List<T> results = new();
            QueryRadius(position, radius, results);

            return results;
        }
    }
}