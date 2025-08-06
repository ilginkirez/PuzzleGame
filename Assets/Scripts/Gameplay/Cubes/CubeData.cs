using UnityEngine;
using PuzzleGame.Core.Enums;

namespace PuzzleGame.Gameplay.Cubes
{
    [System.Serializable]
    public class CubeData
    {
        public CubeType type;
        public Vector3Int gridPosition;
        public Direction direction;
        public Color color;

        public CubeData()
        {
            type = CubeType.Basic;
            gridPosition = Vector3Int.zero;
            direction = Direction.Right;
            color = Color.white;
        }

        public CubeData(CubeType type, Vector3Int position, Direction direction, Color color)
        {
            this.type = type;
            this.gridPosition = position;
            this.direction = direction;
            this.color = color;
        }
    }
}