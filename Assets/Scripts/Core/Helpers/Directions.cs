using UnityEngine;
using PuzzleGame.Core.Enums;

namespace PuzzleGame.Core.Helpers
{
    public static class Directions
    {
        public static Vector3Int ToVector3Int(this Direction direction)
        {
            return direction switch
            {
                Direction.Up    => new Vector3Int(0, 0, 1),
                Direction.Down  => new Vector3Int(0, 0, -1),
                Direction.Left  => new Vector3Int(-1, 0, 0),
                Direction.Right => new Vector3Int(1, 0, 0),
                _ => Vector3Int.zero
            };
        }

        public static Vector3 ToVector3(this Direction direction)
        {
            Vector3Int v = direction.ToVector3Int();
            return new Vector3(v.x, v.y, v.z);
        }

    }
}