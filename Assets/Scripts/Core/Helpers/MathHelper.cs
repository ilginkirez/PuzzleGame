using UnityEngine;

namespace PuzzleGame.Core.Helpers
{
    public static class MathHelper
    {
        public static bool IsInBounds(Vector3Int pos, Vector3Int gridSize)
        {
            return pos.x >= 0 && pos.x < gridSize.x &&
                   pos.y >= 0 && pos.y < gridSize.y &&
                   pos.z >= 0 && pos.z < gridSize.z;
        }

        public static Vector3 GridToWorld(Vector3Int gridPos, float cellSize)
        {
            return new Vector3(
                gridPos.x * cellSize,
                gridPos.y * cellSize,
                gridPos.z * cellSize
            );
        }

        public static Vector3Int WorldToGrid(Vector3 worldPos, float cellSize)
        {
            return new Vector3Int(
                Mathf.RoundToInt(worldPos.x / cellSize),
                Mathf.RoundToInt(worldPos.y / cellSize),
                Mathf.RoundToInt(worldPos.z / cellSize)
            );
        }
    }
}