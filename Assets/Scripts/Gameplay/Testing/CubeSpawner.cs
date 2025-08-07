using UnityEngine;
using PuzzleGame.Core;
using PuzzleGame.Core.Enums;
using PuzzleGame.Gameplay.Cubes;

namespace PuzzleGame.Gameplay.Testing
{
    public class CubeSpawner : MonoBehaviour
    {
        private void Start()
        {
            CubeData data = new CubeData(
                CubeType.Basic,
                new Vector3Int(0, 0, 0),
                Direction.Right,
                Color.red
            );

            CubeFactory.Instance.CreateCube(data);
        }
    }
}