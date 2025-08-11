using UnityEngine;
using PuzzleGame.Core.Enums;
using PuzzleGame.Core.Interfaces;
using PuzzleGame.Core.Helpers;
using PuzzleGame.Gameplay.Cubes;
using PuzzleGame.Gameplay.Managers;
namespace PuzzleGame.Gameplay.Managers
{
    /// <summary>
    /// Küplerin hareket isteklerini yöneten ana sistem.
    /// Move limit kontrolü, eşzamanlı hareket izni ve seviye bitiş kontrolü içerir.
    /// </summary>
    public class MoveManager : Singleton<MoveManager>
    {
        [Header("Move Settings")]
        [SerializeField, Tooltip("Küpün hareket hızı (saniye başına birim).")]
        private float moveSpeed = 1f;
        
        [SerializeField, Tooltip("Birden fazla küpün aynı anda hareket etmesine izin ver.")]
        private bool allowSimultaneousMoves = false;
        
        // Aktif hareket eden küp sayısı
        private int activeMoves = 0;
        
        // Eventler - UI veya diğer sistemler dinleyebilir
        public System.Action<Cube> OnCubeStartMove;
        public System.Action<Cube, MoveResult> OnCubeMoveComplete;
        
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
            
            // Hedefe gidilebilir mi?
            if (cube.CanMove(direction))
            {
                StartMove(cube, direction);
            }
            else
            {
                OnCubeMoveComplete?.Invoke(cube, MoveResult.Blocked);
            }
        }
        
        /// <summary>
        /// Hareketi başlatır ve tamamlandığında eventleri tetikler.
        /// </summary>
        private void StartMove(Cube cube, Direction direction)
        {
            activeMoves++;
            OnCubeStartMove?.Invoke(cube);
            
            cube.Move(direction, (result) =>
            {
                activeMoves--;
                OnCubeMoveComplete?.Invoke(cube, result);
                
                // Tüm hareketler tamamlandıysa, oyun durumu kontrol edilir
                if (activeMoves == 0)
                {
                    CheckGameEnd();
                }
            });
        }
        
        /// <summary>
        /// Level tamamlanma veya başarısız olma durumunu kontrol eder.
        /// TODO: LevelManager eklendikten sonra aktif küp sayısı kontrolü eklenecek
        /// </summary>
        public void CheckGameEnd()
        {
            // Henüz oyun oynanmıyorsa kontrol etme
            if (GameManager.Instance.CurrentState != GameState.Playing)
                return;
            
            // TODO: LevelManager.Instance.GetActiveCubeCount() kontrolü eklenecek
            // Şimdilik sadece move kontrolü yapılıyor
            
            if (GameManager.Instance.MovesLeft <= 0 && activeMoves == 0)
            {
                GameManager.Instance.FailLevel();
            }
            
            // TODO: Aktif küp sayısı 0 olduğunda CompleteLevel çağrılacak
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
