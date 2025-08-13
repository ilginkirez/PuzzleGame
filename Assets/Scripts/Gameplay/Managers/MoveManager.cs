using UnityEngine;
using PuzzleGame.Core.Enums;
using PuzzleGame.Core.Interfaces;
using PuzzleGame.Core.Helpers;
using PuzzleGame.Gameplay.Cubes;
using PuzzleGame.Gameplay.Managers;
using PuzzleGame.Gameplay.Grid;
using System.Collections;

namespace PuzzleGame.Gameplay.Managers
{
    /// <summary>
    /// Küplerin hareket isteklerini yöneten ana sistem.
    /// Sürekli hareket, çarpışma efektleri ve ekrandan çıkma kontrolü içerir.
    /// </summary>
    public class MoveManager : Singleton<MoveManager>
    {
        [Header("Move Settings")]
        [SerializeField, Tooltip("Küpün hareket hızı (saniye başına birim).")]
        private float moveSpeed = 3f;
        
        [SerializeField, Tooltip("Birden fazla küpün aynı anda hareket etmesine izin ver.")]
        private bool allowSimultaneousMoves = false;

        [Header("Collision Settings")]
        [SerializeField, Tooltip("Çarpışma anında küpün kırmızı yanma süresi.")]
        private float collisionFlashDuration = 0.5f;
        
        [SerializeField, Tooltip("Ekrandan çıkmak için maksimum mesafe.")]
        private float exitDistance = 15f;
        
        // Aktif hareket eden küp sayısı
        private int activeMoves = 0;
        
        // Eventler - UI veya diğer sistemler dinleyebilir
        public System.Action<Cube> OnCubeStartMove;
        public System.Action<Cube, MoveResult> OnCubeMoveComplete;
        public System.Action<Cube, Cube> OnCubeCollision; // Çarpışma eventi
        
        // Okunabilir özellikler
        public bool CanMove => activeMoves == 0 || allowSimultaneousMoves;
        public float MoveSpeed => moveSpeed;
        
        /// <summary>
        /// Bir küp için hareket isteği oluşturur.
        /// </summary>
        public void RequestMove(Cube cube, Direction direction)
        {
            // Henüz hareket edilemiyorsa veya move hakkı yoksa
            if (!CanMove || !GameManager.Instance.UseMove())
            {
                OnCubeMoveComplete?.Invoke(cube, MoveResult.Failed);
                return;
            }
            
            StartContinuousMove(cube, direction);
        }
        
        /// <summary>
        /// Küpü sürekli hareket ettir - engele çarpana veya ekrandan çıkana kadar
        /// </summary>
        private void StartContinuousMove(Cube cube, Direction direction)
        {
            activeMoves++;
            OnCubeStartMove?.Invoke(cube);
            
            StartCoroutine(ContinuousMoveCoroutine(cube, direction));
        }
        
        /// <summary>
        /// Sürekli hareket coroutine'i
        /// </summary>
        private IEnumerator ContinuousMoveCoroutine(Cube cube, Direction direction)
        {
            Vector3 targetPosition = cube.transform.position;
            bool isMoving = true;
            
            while (isMoving)
            {
                // Bir sonraki grid pozisyonunu hesapla
                Vector3Int currentGridPos = cube.GridPosition;
                Vector3Int nextGridPos = currentGridPos + direction.ToVector3Int();
                
                // Ekrandan çıktı mı kontrol et
                if (IsOutsidePlayArea(cube.transform.position))
                {
                    // Küpü yok et ve hareketi bitir
                    DestroyCube(cube);
                    isMoving = false;
                    activeMoves--;
                    OnCubeMoveComplete?.Invoke(cube, MoveResult.Success);
                    break;
                }
                
                // Önünde engel var mı kontrol et
                if (GridManager.Instance.IsValidPosition(nextGridPos))
                {
                    var objectsAtNext = GridManager.Instance.GetObjectsAtPosition(nextGridPos);
                    if (objectsAtNext.Count > 0)
                    {
                        // Çarpışma! Engeli kırmızı yap
                        foreach (var obj in objectsAtNext)
                        {
                            if (obj is Cube otherCube)
                            {
                                HandleCollision(cube, otherCube);
                                break;
                            }
                        }
                        isMoving = false;
                        activeMoves--;
                        OnCubeMoveComplete?.Invoke(cube, MoveResult.Blocked);
                        break;
                    }
                }
                
                // Grid pozisyonunu güncelle
                GridManager.Instance.MoveObject(cube, nextGridPos);
                
                // Fiziksel pozisyonu güncelle
                targetPosition = GridManager.Instance.GridToWorldPosition(nextGridPos);
                
                // Smooth movement
                float elapsed = 0f;
                Vector3 startPos = cube.transform.position;
                
                while (elapsed < (1f / moveSpeed))
                {
                    elapsed += Time.deltaTime;
                    float t = elapsed / (1f / moveSpeed);
                    cube.transform.position = Vector3.Lerp(startPos, targetPosition, t);
                    yield return null;
                }
                
                cube.transform.position = targetPosition;
                
                // Kısa bir bekleme (isteğe bağlı, daha smooth görünüm için)
                yield return new WaitForSeconds(0.05f);
            }
            
            // Tüm hareketler tamamlandıysa, oyun durumu kontrol edilir
            if (activeMoves == 0)
            {
                CheckGameEnd();
            }
        }
        
        /// <summary>
        /// Küpün oyun alanının dışına çıkıp çıkmadığını kontrol et
        /// </summary>
        private bool IsOutsidePlayArea(Vector3 position)
        {
            var gridSize = GridManager.Instance.GridSize;
            float maxX = gridSize.x * GridManager.Instance.CellSize + exitDistance;
            float maxZ = gridSize.z * GridManager.Instance.CellSize + exitDistance;
            
            return position.x < -exitDistance || position.x > maxX ||
                   position.z < -exitDistance || position.z > maxZ;
        }
        
        /// <summary>
        /// Çarpışma durumunu handle et - engeli kırmızı yap
        /// </summary>
        private void HandleCollision(Cube movingCube, Cube obstaceCube)
        {
            OnCubeCollision?.Invoke(movingCube, obstaceCube);
            
            // Engel küpü kırmızı yap
            StartCoroutine(FlashCubeRed(obstaceCube));
        }
        
        /// <summary>
        /// Küpü kırmızı renkle yanıp sönder
        /// </summary>
        private IEnumerator FlashCubeRed(Cube cube)
        {
            if (cube == null) yield break;
            
            // Orijinal rengini sakla
            Color originalColor = cube.GetComponent<Renderer>().material.color;
            Material cubeMaterial = cube.GetComponent<Renderer>().material;
            
            float elapsed = 0f;
            while (elapsed < collisionFlashDuration)
            {
                // Kırmızı ve orijinal renk arasında ping-pong
                float t = Mathf.PingPong(elapsed * 8f, 1f); // 8f = yanıp sönme hızı
                cubeMaterial.color = Color.Lerp(Color.red, originalColor, t);
                
                elapsed += Time.deltaTime;
                yield return null;
            }
            
            // Orijinal renge geri döndür
            cubeMaterial.color = originalColor;
        }
        
        /// <summary>
        /// Küpü yok et (ekrandan çıktığında)
        /// </summary>
        private void DestroyCube(Cube cube)
        {
            if (cube != null)
            {
                GridManager.Instance.UnregisterObject(cube);
                
                // Smooth fade out efekti (isteğe bağlı)
                StartCoroutine(FadeOutAndDestroy(cube));
            }
        }
        
        /// <summary>
        /// Küpü yavaşça kaybet ve yok et
        /// </summary>
        private IEnumerator FadeOutAndDestroy(Cube cube)
        {
            Renderer cubeRenderer = cube.GetComponent<Renderer>();
            Material cubeMaterial = cubeRenderer.material;
            Color originalColor = cubeMaterial.color;
            
            float elapsed = 0f;
            float fadeDuration = 0.3f;
            
            while (elapsed < fadeDuration)
            {
                elapsed += Time.deltaTime;
                float alpha = Mathf.Lerp(1f, 0f, elapsed / fadeDuration);
                cubeMaterial.color = new Color(originalColor.r, originalColor.g, originalColor.b, alpha);
                yield return null;
            }
            
            Destroy(cube.gameObject);
        }
        
        /// <summary>
        /// Level tamamlanma veya başarısız olma durumunu kontrol eder.
        /// </summary>
        public void CheckGameEnd()
        {
            // Henüz oyun oynanmıyorsa kontrol etme
            if (GameManager.Instance.CurrentState != GameState.Playing)
                return;
            
            // Aktif küp sayısını kontrol et
            // TODO: Gerçek aktif küp sayısını al
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
        
        /// <summary>
        /// Aktif küp sayısını al (GridManager'dan)
        /// </summary>
        private int GetActiveCubeCount()
        {
            // GridManager'dan tüm küpleri say
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
        
        /// <summary>
        /// Aktif hareket sayısını döndürür (Debug için)
        /// </summary>
        public int GetActiveMoveCount()
        {
            return activeMoves;
        }
    }
}