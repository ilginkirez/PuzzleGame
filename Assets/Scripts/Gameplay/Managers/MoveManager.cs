using UnityEngine;
using PuzzleGame.Core.Enums;
using PuzzleGame.Core.Helpers;
using PuzzleGame.Gameplay.Cubes;
using PuzzleGame.Gameplay.Managers;
using PuzzleGame.Gameplay.Grid;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;

namespace PuzzleGame.Gameplay.Managers
{
    /// <summary>
    /// Küplerin hareket isteklerini yöneten ana sistem.
    /// DOTween ile akışkan hareket, çarpışma efektleri ve ekrandan çıkma kontrolü içerir.
    /// </summary>
    public class MoveManager : Singleton<MoveManager>
    {
        [Header("Move Settings")]
        [SerializeField] private float moveSpeed = 3f;
        [SerializeField] private bool allowSimultaneousMoves = false;
        [SerializeField] private float stepDistance = 1f; // Her adımda ne kadar ilerleyeceği

        [Header("Animation Settings")]
        [SerializeField] private Ease moveEase = Ease.Linear;
        [SerializeField] private float animationStepDuration = 0.2f; // Her adım animasyon süresi

        [Header("Collision Settings")]
        [SerializeField] private float exitDistance = 15f;

        [Header("🔊 Collision Sound Settings")]
        [SerializeField] private AudioSource audioSource;
        [SerializeField] private AudioClip collisionSound;
        [SerializeField] private AudioClip[] errorSounds;
        [Range(0f, 1f)] [SerializeField] private float collisionVolume = 0.8f;

        private int activeMoves = 0;
        private Dictionary<Cube, Color> originalColors = new Dictionary<Cube, Color>();
        private Dictionary<Cube, Sequence> activeSequences = new Dictionary<Cube, Sequence>();

        // Events
        public System.Action<Cube> OnCubeStartMove;
        public System.Action<Cube, MoveResult> OnCubeMoveComplete;
        public System.Action<Cube, Cube> OnCubeCollision;

        public bool CanMove => activeMoves == 0 || allowSimultaneousMoves;
        public float MoveSpeed => moveSpeed;

        private void Awake()
        {
            // 🔊 AudioSource ekle
            if (audioSource == null)
                audioSource = GetComponent<AudioSource>();
            
            if (audioSource == null)
                audioSource = gameObject.AddComponent<AudioSource>();
        }

        public void RequestMove(Cube cube, Direction direction)
        {
            if (!CanMove || !GameManager.Instance.UseMove())
            {
                OnCubeMoveComplete?.Invoke(cube, MoveResult.Failed);
                return;
            }
            StartSmoothMove(cube, direction);
        }

        private void StartSmoothMove(Cube cube, Direction direction)
        {
            // Eğer bu küp zaten hareket halindeyse, mevcut animasyonu durdur
            if (activeSequences.ContainsKey(cube))
            {
                activeSequences[cube]?.Kill();
                activeSequences.Remove(cube);
            }

            activeMoves++;
            OnCubeStartMove?.Invoke(cube);

            StartCoroutine(SmoothMoveCoroutine(cube, direction));
        }

        private IEnumerator SmoothMoveCoroutine(Cube cube, Direction direction)
        {
            Vector3 directionVector = direction.ToVector3Int();
            bool isMoving = true;

            // DOTween sequence oluştur
            Sequence moveSequence = DOTween.Sequence();
            activeSequences[cube] = moveSequence;

            while (isMoving)
            {
                Vector3Int currentGridPos = cube.GridPosition;
                Vector3Int nextGridPos = currentGridPos + direction.ToVector3Int();

                // Hedef world pozisyonu
                Vector3 targetWorldPos = GridManager.Instance.GridToWorldPosition(nextGridPos);

                // Dışarı çıkma kontrolü - hedef pozisyona göre
                if (IsOutsidePlayArea(targetWorldPos))
                {
                    // Son bir adım at ve çık
                    moveSequence.Append(
                        cube.transform.DOMove(targetWorldPos, animationStepDuration / moveSpeed)
                            .SetEase(moveEase)
                    );

                    // Animasyon bitene kadar bekle
                    yield return moveSequence.WaitForCompletion();

                    ReturnCube(cube);
                    isMoving = false;
                    activeMoves--;
                    OnCubeMoveComplete?.Invoke(cube, MoveResult.Success);
                    break;
                }

                // Engel kontrolü
                if (GridManager.Instance.IsValidPosition(nextGridPos))
                {
                    var objectsAtNext = GridManager.Instance.GetObjectsAtPosition(nextGridPos);
                    if (objectsAtNext.Count > 0)
                    {
                        bool collisionFound = false;
                        foreach (var obj in objectsAtNext)
                        {
                            if (obj is Cube otherCube)
                            {
                                HandleCollision(cube, otherCube);
                                collisionFound = true;
                                break;
                            }
                        }
                        
                        if (collisionFound)
                        {
                            isMoving = false;
                            activeMoves--;
                            OnCubeMoveComplete?.Invoke(cube, MoveResult.Blocked);
                            break;
                        }
                    }
                }

                // Grid güncelle
                GridManager.Instance.MoveObject(cube, nextGridPos);

                // Smooth hareket animasyonu ekle
                moveSequence.Append(
                    cube.transform.DOMove(targetWorldPos, animationStepDuration / moveSpeed)
                        .SetEase(moveEase)
                );

                // Bu adımın animasyonunun bitmesini bekle
                yield return new WaitForSeconds(animationStepDuration / moveSpeed);

                // Eğer oyun durmuşsa veya küp yok edildiyse çık
                if (cube == null || GameManager.Instance.CurrentState != GameState.Playing)
                {
                    isMoving = false;
                    activeMoves--;
                    break;
                }
            }

            // Sequence'i temizle
            if (activeSequences.ContainsKey(cube))
            {
                activeSequences.Remove(cube);
            }

            if (activeMoves == 0)
            {
                CheckGameEnd();
            }
        }

        private bool IsOutsidePlayArea(Vector3 position)
        {
            var gridSize = GridManager.Instance.GridSize;
            float maxX = gridSize.x * GridManager.Instance.CellSize + exitDistance;
            float maxZ = gridSize.z * GridManager.Instance.CellSize + exitDistance;

            return position.x < -exitDistance || position.x > maxX ||
                   position.z < -exitDistance || position.z > maxZ;
        }

        private void HandleCollision(Cube movingCube, Cube obstacleCube)
        {
            // Hareket animasyonunu durdur
            if (activeSequences.ContainsKey(movingCube))
            {
                activeSequences[movingCube]?.Kill();
                activeSequences.Remove(movingCube);
            }

            // 🔊 Çarpışma sesi çal
            PlayCollisionSound();

            OnCubeCollision?.Invoke(movingCube, obstacleCube);

            // 🎯 Scale animasyonu
            if (movingCube != null)
            {
                movingCube.transform.DOKill();
                movingCube.transform.localScale = Vector3.one;

                movingCube.transform.DOPunchScale(
                    Vector3.one * 0.08f,
                    0.2f,
                    vibrato: 6,
                    elasticity: 0.7f
                );
            }

            if (obstacleCube != null)
            {
                obstacleCube.transform.DOKill();
                obstacleCube.transform.localScale = Vector3.one;

                obstacleCube.transform.DOPunchScale(
                    Vector3.one * 0.08f,
                    0.2f,
                    6,
                    0.7f
                );

                // 🔧 Renk animasyonu - güvenli şekilde
                StartCoroutine(FlashCubeColor(obstacleCube));
            }
        }

        // 🔊 Çarpışma sesi çalma metodu
        private void PlayCollisionSound()
        {
            if (audioSource == null) return;

            // Önce collision sound'u dene
            if (collisionSound != null)
            {
                audioSource.PlayOneShot(collisionSound, collisionVolume);
            }
            // Yoksa error sounds'tan rastgele birini çal
            else if (errorSounds != null && errorSounds.Length > 0)
            {
                AudioClip randomErrorSound = errorSounds[Random.Range(0, errorSounds.Length)];
                audioSource.PlayOneShot(randomErrorSound, collisionVolume);
            }
        }

        private IEnumerator FlashCubeColor(Cube cube)
        {
            if (cube == null) yield break;

            var renderer = cube.GetComponent<Renderer>();
            if (renderer == null || renderer.material == null) yield break;

            // 🔧 Orijinal rengi kaydet
            if (!originalColors.ContainsKey(cube))
            {
                originalColors[cube] = renderer.material.color;
            }
            Color originalColor = originalColors[cube];

            // 🔧 Tüm material tween'lerini iptal et
            renderer.material.DOKill();

            // DOTween ile renk flash animasyonu
            Sequence colorSequence = DOTween.Sequence();
            colorSequence.Append(renderer.material.DOColor(Color.red, 0.1f));
            colorSequence.Append(renderer.material.DOColor(originalColor, 0.1f));
            colorSequence.Append(renderer.material.DOColor(Color.red, 0.1f));
            colorSequence.Append(renderer.material.DOColor(originalColor, 0.1f));

            yield return colorSequence.WaitForCompletion();
        }

        /// <summary>
        /// Destroy yerine pool'a iade
        /// </summary>
        private void ReturnCube(Cube cube)
        {
            if (cube != null)
            {
                // Aktif animasyonları durdur
                if (activeSequences.ContainsKey(cube))
                {
                    activeSequences[cube]?.Kill();
                    activeSequences.Remove(cube);
                }

                GridManager.Instance.UnregisterObject(cube);
                
                // 🔧 Renk cache'ini temizle
                if (originalColors.ContainsKey(cube))
                {
                    originalColors.Remove(cube);
                }
                
                StartCoroutine(FadeOutAndReturn(cube));
            }
        }

        private IEnumerator FadeOutAndReturn(Cube cube)
        {
            var renderer = cube.GetComponent<Renderer>();
            if (renderer == null || renderer.material == null) 
            {
                PoolManager.Instance.Return(cube);
                yield break;
            }

            var cubeMaterial = renderer.material;
            Color originalColor = cubeMaterial.color;

            // 🔧 Tüm material animasyonlarını iptal et
            cubeMaterial.DOKill();

            // DOTween ile fade out
            yield return cubeMaterial.DOFade(0f, 0.3f).WaitForCompletion();

            // Havuza iade
            PoolManager.Instance.Return(cube);

            // 🔧 Rengi resetle
            if (cubeMaterial != null)
            {
                cubeMaterial.color = originalColor;
            }
        }

        public void CheckGameEnd()
        {
            if (GameManager.Instance.CurrentState != GameState.Playing)
                return;

            int activeCubeCount = GetActiveCubeCount();

            if (activeCubeCount == 0)
            {
                GameManager.Instance.CompleteLevel();
            }
            else if (GameManager.Instance.MovesLeft <= 0 && activeMoves == 0)
            {
                GameManager.Instance.FailLevel();
            }
        }

        private int GetActiveCubeCount()
        {
            int count = 0;
            var gridSize = GridManager.Instance.GridSize;

            for (int x = 0; x < gridSize.x; x++)
            {
                for (int z = 0; z < gridSize.z; z++)
                {
                    var objects = GridManager.Instance.GetObjectsAtPosition(new Vector3Int(x, 0, z));
                    foreach (var obj in objects)
                    {
                        if (obj is Cube) count++;
                    }
                }
            }
            return count;
        }

        public int GetActiveMoveCount() => activeMoves;

        // 🔧 Tüm aktif küp animasyonlarını durdur
        public void StopAllCubeAnimations()
        {
            foreach (var sequence in activeSequences.Values)
            {
                sequence?.Kill();
            }
            activeSequences.Clear();
        }

        // 🔧 Level temizlenirken renk cache'ini temizle
        public void ClearColorCache()
        {
            originalColors.Clear();
        }

        private void OnDestroy()
        {
            StopAllCubeAnimations();
            ClearColorCache();
        }
    }
}