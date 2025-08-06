using UnityEngine;


namespace PuzzleGame.Core;
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

    public CubeData(CubeType cubeType, Vector3Int position, Direction moveDirection, Color cubeColor)
    {
        type = cubeType;
        gridPosition = position;
        direction = moveDirection;
        color = cubeColor;
    }
}
}