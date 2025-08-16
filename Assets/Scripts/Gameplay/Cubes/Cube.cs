using UnityEngine;
using PuzzleGame.Core.Enums;
using PuzzleGame.Core.Interfaces;
using PuzzleGame.Gameplay.Managers;
using PuzzleGame.Gameplay.Grid;
using PuzzleGame.Core.Helpers;
using System;

namespace PuzzleGame.Gameplay.Cubes
{
    public class Cube : MonoBehaviour, IClickable, IMovable, IGridObject
    {
        [Header("Cube Settings")]
        [SerializeField] private CubeType cubeType = CubeType.Basic;
        [SerializeField] private Direction moveDirection = Direction.Right;
        [SerializeField] private Color cubeColor = Color.white;

        [Header("Visuals")]
        [SerializeField] private Transform arrowVisual;
        [SerializeField] private float arrowYawOffset = 0f;

        public bool IsClickable => true;
        public bool IsMoving { get; private set; }
        public Vector3Int GridPosition { get; set; }

        // Event’ler (varsa)
        public event Action<Cube> OnCubeDestroyed;

        public void Initialize(CubeData data)
        {
            cubeType = data.type;
            cubeColor = data.color;
            SetDirection(data.direction);

            GridPosition = data.gridPosition;
            transform.position = GridManager.Instance.GridToWorldPosition(GridPosition);
            UpdateColor();
        }

        private void OnMouseDown()
        {
            MoveManager.Instance.RequestMove(this, moveDirection);
        }

        private void UpdateColor()
        {
            // Küpün ana rengi
            if (TryGetComponent(out Renderer r))
                r.material.color = cubeColor;

            // Oku renklendir
            UpdateArrowColor();
        }

        private void UpdateArrowColor()
        {
            if (arrowVisual != null && arrowVisual.TryGetComponent(out Renderer arrowRenderer))
            {
                // sharedMaterial kullanarak draw call artışını engelle
                Material mat = arrowRenderer.sharedMaterial;

                // Oku sabit beyaz yap
                mat.color = Color.white;
            }
        }

        public void SetDirection(Direction dir)
        {
            moveDirection = dir;
            UpdateArrowVisual();
        }

        private void UpdateArrowVisual()
        {
            if (arrowVisual == null) return;

            float yAngle = moveDirection switch
            {
                Direction.Right => 0f,
                Direction.Up    => 270f,
                Direction.Left  => 180f,
                Direction.Down  => 90f,
                _ => 0f
            };

            arrowVisual.localRotation = Quaternion.Euler(0f, yAngle + arrowYawOffset, 0f);
        }

        private void OnValidate()
        {
            UpdateArrowVisual();
        }

        public void OnClick()
        {
            MoveManager.Instance.RequestMove(this, moveDirection);
        }

        public bool CanMove(Direction direction)
        {
            Vector3Int targetPos = GridPosition + direction.ToVector3Int();
            return GridManager.Instance.CanMoveTo(targetPos, this);
        }

        public void Move(Direction direction, System.Action<MoveResult> onComplete = null)
        {
            if (IsMoving)
            {
                onComplete?.Invoke(MoveResult.Failed);
                return;
            }

            Vector3Int targetPos = GridPosition + direction.ToVector3Int();
            if (!GridManager.Instance.CanMoveTo(targetPos, this))
            {
                onComplete?.Invoke(MoveResult.Blocked);
                return;
            }

            IsMoving = true;
            GridManager.Instance.MoveObject(this, targetPos);
            transform.position = GridManager.Instance.GridToWorldPosition(targetPos);
            IsMoving = false;

            onComplete?.Invoke(MoveResult.Success);
        }

        public Vector3 WorldPosition => transform.position;

        public void SetGridPosition(Vector3Int position) => GridPosition = position;

        /// <summary>
        /// Pool’dan iade edilirken sıfırlama yapılır.
        /// </summary>
        public void ResetCube()
        {
            cubeType = CubeType.Basic;
            moveDirection = Direction.Right;
            cubeColor = Color.white;
            IsMoving = false;
            GridPosition = Vector3Int.zero;

            UpdateColor();
            UpdateArrowVisual();

            OnCubeDestroyed = null; // Event temizliği
        }

        /// <summary>
        /// PoolManager tarafından çağrılacak hook.
        /// </summary>
        public void OnReturnedToPool()
        {
            ResetCube();
            GridManager.Instance.UnregisterObject(this);
            gameObject.SetActive(false);
        }
    }
}
