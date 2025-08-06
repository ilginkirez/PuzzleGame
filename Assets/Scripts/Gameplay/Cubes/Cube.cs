using UnityEngine;
using PuzzleGame.Core.Enums;

namespace PuzzleGame.Gameplay.Cubes
{
    public class Cube : MonoBehaviour
    {
        [Header("Cube Settings")]
        [SerializeField] private CubeType cubeType = CubeType.Basic;
        [SerializeField] private Direction moveDirection = Direction.Right;
        [SerializeField] private Color cubeColor = Color.white;
        [SerializeField] private GameObject arrowVisual;

        public void Initialize(CubeData data)
        {
            cubeType = data.type;
            moveDirection = data.direction;
            cubeColor = data.color;

            transform.position = data.gridPosition;
            UpdateVisuals();
        }

        private void UpdateVisuals()
        {
            if (TryGetComponent(out Renderer renderer))
            {
                renderer.material.color = cubeColor;
            }

            if (arrowVisual != null)
            {
                float angle = GetAngleFromDirection(moveDirection);
                arrowVisual.transform.rotation = Quaternion.Euler(0f, angle - 90f, 0f);
            }
        }

        private float GetAngleFromDirection(Direction direction)
        {
            switch (direction)
            {
                case Direction.Right: return 0f;
                case Direction.Left: return 180f;
                case Direction.Up: return 90f;
                case Direction.Down: return 270f;
                default: return 0f;
            }
        }
    }
}