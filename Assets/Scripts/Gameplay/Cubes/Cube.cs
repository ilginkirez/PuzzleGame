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

        [Header("Audio Settings")]
        [SerializeField] private AudioSource audioSource;
        [SerializeField] private AudioClip[] clickSounds;
        [SerializeField] private AudioClip moveSound;
        [SerializeField] private AudioClip blockedSound;
        [Range(0f, 1f)] [SerializeField] private float clickVolume = 0.7f;
        [Range(0f, 1f)] [SerializeField] private float moveVolume = 0.5f;
        [Range(0f, 1f)] [SerializeField] private float blockedVolume = 0.8f;

        [Header("Click Animation")]
        [SerializeField] private AnimationCurve scalePunch = AnimationCurve.EaseInOut(0f, 1f, 1f, 1f);
        [SerializeField] private float punchScale = 1.2f;
        [SerializeField] private float punchDuration = 0.3f;

        public bool IsClickable => true;
        public bool IsMoving { get; private set; }
        public Vector3Int GridPosition { get; set; }

        public event Action<Cube> OnCubeDestroyed;

        private Vector3 originalScale;
        private Coroutine scaleCoroutine;

        private void Awake()
        {
            if (audioSource == null)
                audioSource = gameObject.AddComponent<AudioSource>();

            originalScale = transform.localScale;
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
            PlayClickSound();
            PlayScalePunch();

            Debug.Log($"Küp tıklandı: {name}");
            MoveManager.Instance.RequestMove(this, moveDirection);
        }

        public void OnClick()
        {
            OnMouseDown();
        }

        private void PlayClickSound()
        {
            if (clickSounds != null && clickSounds.Length > 0)
            {
                AudioClip randomClip = clickSounds[UnityEngine.Random.Range(0, clickSounds.Length)];
                audioSource.PlayOneShot(randomClip, clickVolume);
            }
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
            if (TryGetComponent(out Renderer r))
                r.material.color = cubeColor;

            if (arrowVisual != null && arrowVisual.TryGetComponent(out Renderer arrowRenderer))
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
                PlayBlockedSound();
                onComplete?.Invoke(MoveResult.Blocked);
                return;
            }

            IsMoving = true;
            PlayMoveSound();

            GridManager.Instance.MoveObject(this, targetPos);
            transform.position = GridManager.Instance.GridToWorldPosition(targetPos);
            IsMoving = false;

            onComplete?.Invoke(MoveResult.Success);
        }

        private void PlayMoveSound()
        {
            if (moveSound != null)
                audioSource.PlayOneShot(moveSound, moveVolume);
        }

        private void PlayBlockedSound()
        {
            if (blockedSound != null)
                audioSource.PlayOneShot(blockedSound, blockedVolume);
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
    }
}
