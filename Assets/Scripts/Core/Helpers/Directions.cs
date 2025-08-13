using UnityEngine;
using PuzzleGame.Core.Enums;

namespace PuzzleGame.Core.Helpers
{
    public static class Directions
    {
        public static Vector3Int ToVector3Int(this Direction dir)
        {
            switch (dir)
            {
                case Direction.Right: return new Vector3Int(+1, 0, 0);
                case Direction.Left:  return new Vector3Int(-1, 0, 0);
                case Direction.Up:    return new Vector3Int(0, 0, -1);
                case Direction.Down:  return new Vector3Int(0, 0, +1);

                default: return Vector3Int.zero;
            }
        }
    }
}