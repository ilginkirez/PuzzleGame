using UnityEngine;
using PuzzleGame.Core.Enums;
using PuzzleGame.Core.Interfaces;
using PuzzleGame.Gameplay.Managers;
using PuzzleGame.Gameplay.Grid;
using PuzzleGame.Core.Helpers;

namespace PuzzleGame.Gameplay.Cubes
{
    public class Cube : MonoBehaviour, IClickable, IMovable, IGridObject
    {
        [Header("Cube Settings")]
        [SerializeField] private CubeType cubeType = CubeType.Basic;

        [Tooltip("Hareket yönünün tek kaynağı. JSON -> CubeData.direction ile gelir.")]
        [SerializeField] private Direction moveDirection = Direction.Right;

        [SerializeField] private Color cubeColor = Color.white;

        [Header("Visuals")]
        [SerializeField] private Transform arrowVisual; 
        [SerializeField] private float arrowYawOffset = 0f; // ← Inspector’dan ayarlanacak

        public bool IsClickable => true;
        public bool IsMoving { get; private set; }
        public Vector3Int GridPosition { get; set; }

        public void Initialize(CubeData data)
        {
            cubeType = data.type;
            cubeColor = data.color;

            // JSON'dan gelen yön -> tek gerçek kaynak
            SetDirection(data.direction);

            GridPosition = data.gridPosition;
            transform.position = GridManager.Instance.GridToWorldPosition(GridPosition);
            UpdateColor();
        }

        private void UpdateColor()
        {
            if (TryGetComponent(out Renderer r))
                r.material.color = cubeColor;
        }

        /// <summary>
        /// Yönü programatik olarak değiştir (JSON yüklenince, power-up vb.).
        /// Görsel oku da aynı yöne çevirir.
        /// </summary>
        public void SetDirection(Direction dir)
        {
            moveDirection = dir;
            UpdateArrowVisual();
        }

        /// <summary>
        /// Ok child'ının yerel Y rotasyonunu yönle eşleştir.
        /// </summary>
        private void UpdateArrowVisual()
        {
            if (arrowVisual == null) return;

            float yAngle = moveDirection switch
            {
                Direction.Right => 0f,
                Direction.Up    => 90f,
                Direction.Left  => 180f,
                Direction.Down  => 270f,
                _ => 0f
            };

            // Prefab’ın yerel yönü ile bizim haritamızı hizalamak için telafi açısı
            arrowVisual.localRotation = Quaternion.Euler(0f, yAngle + arrowYawOffset, 0f);
        }

        private void OnValidate()
        {
            UpdateArrowVisual();
        }


        // (Opsiyonel) Prefabı editor’da manuel döndürürsen buradan direction türetebilirsin.
        private Direction DirectionFromArrowRotation()
        {
            if (arrowVisual == null) return moveDirection;

            float y = arrowVisual.localEulerAngles.y;
            // 0/90/180/270 toleranslı eşleme
            if (Mathf.Abs(Mathf.DeltaAngle(y, 0f)) <= 45f)   return Direction.Right;
            if (Mathf.Abs(Mathf.DeltaAngle(y, 90f)) <= 45f)  return Direction.Up;
            if (Mathf.Abs(Mathf.DeltaAngle(y, 180f)) <= 45f) return Direction.Left;
            return Direction.Down;
        }

        // Editor’da inspector’da yön alanını değiştirince oku otomatik çevirsin (quality of life)

        public void OnClick()
        {
            // her tıklamada JSON’dan gelen yönü kullan (okla aynı)
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
    }
}
