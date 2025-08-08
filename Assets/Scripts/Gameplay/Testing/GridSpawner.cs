using UnityEngine;
using PuzzleGame.Core.Enums;
using PuzzleGame.Gameplay.Cubes;
using PuzzleGame.Core;

public class GridSpawner : MonoBehaviour
{
    void Start()
    {
        var data = new CubeData(CubeType.Basic, new Vector3Int(3, 0, 3), Direction.Right, Color.yellow);
        CubeFactory.Instance.CreateCube(data);
    }
}