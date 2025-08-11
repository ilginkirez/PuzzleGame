using UnityEngine;
using PuzzleGame.Core;
using PuzzleGame.Core.Enums;
using PuzzleGame.Gameplay.Cubes;
using PuzzleGame.Gameplay.Managers;

namespace PuzzleGame.Gameplay.Testing
{
    public class CubeSpawner : MonoBehaviour
    {
        [Header("Test Cube")]
        [SerializeField] private Vector3Int spawnGridPos = new Vector3Int(2, 0, 2);
        [SerializeField] private Direction spawnDirection = Direction.Right;
        [SerializeField] private Color spawnColor = Color.red;

        private void Start()
        {
            // 1) Test için oyunu 10 hamle ile başlat
            GameManager.Instance.QuickStartLevel();

            // 2) Küp verisi
            var data = new CubeData(
                CubeType.Basic,
                spawnGridPos,
                spawnDirection,
                spawnColor
            );

            // 3) Factory ile sahneye küp üret
            CubeFactory.Instance.CreateCube(data);
        }
    }
}
