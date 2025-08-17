using System.Collections.Generic;
using UnityEngine;
using PuzzleGame.Core.Interfaces;
using PuzzleGame.Core.Helpers;

namespace PuzzleGame.Gameplay.Grid
{
    public class GridManager : Singleton<GridManager>
    {
        [Header("Grid Settings")]
        [SerializeField] private Vector3Int gridSize = new Vector3Int(10, 1, 10);
        [SerializeField] private float cellSize = 1f;
        [SerializeField] private bool showGridGizmos = true;

        private Dictionary<Vector3Int, GridCell> gridCells;
        private Dictionary<Vector3Int, List<IGridObject>> objectsAtPosition;

        public Vector3Int GridSize => gridSize;
        public float CellSize => cellSize;

        protected override void Awake()
        {
            base.Awake();
            InitializeGrid();
        }

        private void InitializeGrid()
        {
            gridCells = new Dictionary<Vector3Int, GridCell>();
            objectsAtPosition = new Dictionary<Vector3Int, List<IGridObject>>();

            for (int x = 0; x < gridSize.x; x++)
            {
                for (int y = 0; y < gridSize.y; y++)
                {
                    for (int z = 0; z < gridSize.z; z++)
                    {
                        Vector3Int gridPos = new Vector3Int(x, y, z);
                        gridCells[gridPos] = new GridCell(gridPos);
                        objectsAtPosition[gridPos] = new List<IGridObject>();
                    }
                }
            }
        }

        public void ClearAll()
        {
            foreach (var kvp in objectsAtPosition)
            {
                kvp.Value.Clear();
            }
        }

        public bool IsValidPosition(Vector3Int gridPosition)
        {
            return MathHelper.IsInBounds(gridPosition, gridSize);
        }

        public Vector3 GridToWorldPosition(Vector3Int gridPosition)
        {
            return MathHelper.GridToWorld(gridPosition, cellSize);
        }

        public Vector3Int WorldToGridPosition(Vector3 worldPosition)
        {
            return MathHelper.WorldToGrid(worldPosition, cellSize);
        }

        public bool CanMoveTo(Vector3Int gridPosition, IGridObject movingObject = null)
        {
            if (!IsValidPosition(gridPosition)) return false;

            GridCell cell = GetCell(gridPosition);
            if (cell == null || cell.IsBlocked) return false;

            var objectsAtPos = GetObjectsAtPosition(gridPosition);
            foreach (var obj in objectsAtPos)
            {
                if (obj != movingObject)
                    return false;
            }

            return true;
        }

        public GridCell GetCell(Vector3Int gridPosition)
        {
            return gridCells.TryGetValue(gridPosition, out GridCell cell) ? cell : null;
        }

        public List<IGridObject> GetObjectsAtPosition(Vector3Int gridPosition)
        {
            return objectsAtPosition.TryGetValue(gridPosition, out List<IGridObject> objects)
                ? new List<IGridObject>(objects)
                : new List<IGridObject>();
        }

        public void RegisterObject(IGridObject gridObject)
        {
            Vector3Int pos = gridObject.GridPosition;
            if (IsValidPosition(pos))
            {
                objectsAtPosition[pos].Add(gridObject);
            }
        }

        public void UnregisterObject(IGridObject gridObject)
        {
            Vector3Int pos = gridObject.GridPosition;
            if (IsValidPosition(pos))
            {
                objectsAtPosition[pos].Remove(gridObject);
            }
        }

        public void MoveObject(IGridObject gridObject, Vector3Int newGridPosition)
        {
            UnregisterObject(gridObject);
            gridObject.SetGridPosition(newGridPosition);
            RegisterObject(gridObject);
        }

        public void SetGridSize(Vector3Int newSize)
        {
            gridSize = newSize;
            InitializeGrid();
        }

        private void OnDrawGizmos()
        {
            if (!showGridGizmos) return;

            Gizmos.color = Color.white;
            Vector3 center = new Vector3(
                (gridSize.x * cellSize) / 2f - cellSize / 2f,
                0f,
                (gridSize.z * cellSize) / 2f - cellSize / 2f
            );

            Vector3 size = new Vector3(gridSize.x * cellSize, 0.1f, gridSize.z * cellSize);
            Gizmos.DrawWireCube(center, size);

            Gizmos.color = Color.gray;
            for (int x = 0; x <= gridSize.x; x++)
            {
                Vector3 start = new Vector3(x * cellSize, 0, 0);
                Vector3 end = new Vector3(x * cellSize, 0, gridSize.z * cellSize);
                Gizmos.DrawLine(start, end);
            }

            for (int z = 0; z <= gridSize.z; z++)
            {
                Vector3 start = new Vector3(0, 0, z * cellSize);
                Vector3 end = new Vector3(gridSize.x * cellSize, 0, z * cellSize);
                Gizmos.DrawLine(start, end);
            }
        }
    }
}  