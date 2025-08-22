using UnityEngine;
using PuzzleGame.Core.Enums;
using PuzzleGame.Core.Interfaces;
using PuzzleGame.Gameplay.Managers;
using PuzzleGame.Gameplay.Grid;
using PuzzleGame.Core.Helpers;
using System;
using System.Collections;

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

        [Header("Click Animation")]
        [SerializeField] private AnimationCurve scalePunch = AnimationCurve.EaseInOut(0f, 1f, 1f, 1f);
        [SerializeField] private float punchScale = 1.2f;
        [SerializeField] private float punchDuration = 0.3f;

        // ✅ CACHED COMPONENT REFERENCES - Performance Optimization
        private Renderer cubeRenderer;
        private Renderer arrowRenderer;
        private Material cubeMaterial;

        public bool IsClickable => true;
        public bool IsMoving { get; private set; }
        public Vector3Int GridPosition { get; set; }

        public event Action<Cube> OnCubeDestroyed;

        private Vector3 originalScale;
        private Coroutine scaleCoroutine;

        private void Awake()
        {
            // ✅ CACHE COMPONENTS ONCE AT START
            CacheComponents();
            originalScale = transform.localScale;
        }

        private void CacheComponents()
        {
            // Cache main cube renderer and material
            cubeRenderer = GetComponent<Renderer>();
            if (cubeRenderer != null)
                cubeMaterial = cubeRenderer.material;

            // Cache arrow renderer
            if (arrowVisual != null)
                arrowRenderer = arrowVisual.GetComponent<Renderer>();
        }

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
            HandleCubeClick();
        }

        public void OnClick()
        {
            HandleCubeClick();
        }

        private void HandleCubeClick()
        {
            Debug.Log($"Küp tıklandı: {name} - AudioManager var mı: {AudioManager.Instance != null}");
            
            AudioManager.Instance?.PlayCubeClick();
            PlayScalePunch();
            MoveManager.Instance?.RequestMove(this, moveDirection);
        }

        private void PlayScalePunch()
        {
            if (scaleCoroutine != null)
                StopCoroutine(scaleCoroutine);

            scaleCoroutine = StartCoroutine(ScalePunchCoroutine());
        }

        private IEnumerator ScalePunchCoroutine()
        {
            float elapsed = 0f;

            while (elapsed < punchDuration)
            {
                elapsed += Time.deltaTime;
                float progress = elapsed / punchDuration;
                float scale = scalePunch.Evaluate(progress);

                transform.localScale = originalScale * (1f + (punchScale - 1f) * scale);
                yield return null;
            }

            transform.localScale = originalScale;
            scaleCoroutine = null;
        }

        private void UpdateColor()
        {
            // ✅ USE CACHED REFERENCES - No more TryGetComponent!
            if (cubeMaterial != null)
                cubeMaterial.color = cubeColor;

            // ✅ Use cached arrow renderer
            if (arrowRenderer != null)
                arrowRenderer.sharedMaterial.color = Color.white;
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
            if (Application.isPlaying)
                UpdateColor();
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
                AudioManager.Instance?.PlayCubeBlocked();
                onComplete?.Invoke(MoveResult.Blocked);
                return;
            }

            IsMoving = true;
            AudioManager.Instance?.PlayCubeMove();

            GridManager.Instance.MoveObject(this, targetPos);
            transform.position = GridManager.Instance.GridToWorldPosition(targetPos);
            IsMoving = false;

            onComplete?.Invoke(MoveResult.Success);
        }

        public Vector3 WorldPosition => transform.position;
        public void SetGridPosition(Vector3Int position) => GridPosition = position;

        public void ResetCube()
        {
            cubeType = CubeType.Basic;
            moveDirection = Direction.Right;
            cubeColor = Color.white;
            IsMoving = false;
            GridPosition = Vector3Int.zero;

            if (scaleCoroutine != null)
            {
                StopCoroutine(scaleCoroutine);
                scaleCoroutine = null;
            }

            transform.localScale = originalScale;
            UpdateColor();
            UpdateArrowVisual();
            OnCubeDestroyed = null;
        }

        public void OnReturnedToPool()
        {
            ResetCube();
            GridManager.Instance.UnregisterObject(this);
            gameObject.SetActive(false);
        }

        // ✅ If you need to refresh component cache (for pooled objects)
        public void RefreshComponentCache()
        {
            CacheComponents();
        }
    }
}