using UnityEngine;
using PuzzleGame.Core.Helpers;
using PuzzleGame.Core.Enums;

namespace PuzzleGame.Gameplay.Managers
{
    /// <summary>
    /// Oyunun genel akışını yöneten ana manager.
    /// Level durumu, move sayısı ve oyun state yönetimi.
    /// </summary>
    public class GameManager : Singleton<GameManager>
    {
        [Header("Game Settings")]
        [SerializeField] private int currentLevel = 1;
        [SerializeField] private int movesLeft;
        [SerializeField] private int maxMoves;
        
        [Header("Debug")]
        [SerializeField] private GameState currentState = GameState.Menu;
        
        // Events
        public System.Action OnLevelComplete;
        public System.Action OnLevelFailed;
        public System.Action<int> OnMovesChanged; // int parametresi kalan move sayısı
        public System.Action OnGameStateChanged;
        
        // Properties
        public GameState CurrentState => currentState;
        public int CurrentLevel => currentLevel;
        public int MovesLeft => movesLeft;
        public int MaxMoves => maxMoves;
        
        protected override void Awake()
        {
            base.Awake();
            // Başlangıç state'i
            ChangeState(GameState.Menu);
        }
        
        /// <summary>
        /// Yeni bir level başlatır.
        /// </summary>
        public void StartLevel(int levelIndex, int moveLimit)
        {
            currentLevel = levelIndex;
            maxMoves = moveLimit;
            movesLeft = moveLimit;
            
            ChangeState(GameState.Playing);
            OnMovesChanged?.Invoke(movesLeft);
            
            Debug.Log($"Level {currentLevel} başlatıldı. Move Limit: {maxMoves}");
        }
        
        /// <summary>
        /// Test için hızlı level başlatma (Inspector'dan çağrılabilir).
        /// </summary>
        [ContextMenu("Quick Start Level")]
        public void QuickStartLevel()
        {
            StartLevel(currentLevel, 10); // Varsayılan 10 hamle
        }
        
        /// <summary>
        /// Bir hamle kullanır. MoveManager tarafından çağrılır.
        /// </summary>
        public bool UseMove()
        {
            if (movesLeft > 0 && currentState == GameState.Playing)
            {
                movesLeft--;
                OnMovesChanged?.Invoke(movesLeft);
                
                Debug.Log($"Move kullanıldı. Kalan: {movesLeft}/{maxMoves}");
                return true;
            }
            return false;
        }
        
        /// <summary>
        /// Level tamamlandığında çağrılır.
        /// </summary>
        public void CompleteLevel()
        {
            if (currentState != GameState.Playing) return;
            
            ChangeState(GameState.LevelComplete);
            OnLevelComplete?.Invoke();
            
            Debug.Log($"Level {currentLevel} tamamlandı! Kullanılan hamle: {maxMoves - movesLeft}");
            
            // TODO: Skor hesaplama, yıldız verme vb.
        }
        
        /// <summary>
        /// Level başarısız olduğunda çağrılır.
        /// </summary>
        public void FailLevel()
        {
            if (currentState != GameState.Playing) return;
            
            ChangeState(GameState.LevelFailed);
            OnLevelFailed?.Invoke();
            
            Debug.Log($"Level {currentLevel} başarısız!");
        }
        
        /// <summary>
        /// Mevcut leveli yeniden başlatır.
        /// </summary>
        public void RestartLevel()
        {
            Debug.Log($"Level {currentLevel} yeniden başlatılıyor...");
            
            // TODO: LevelManager hazır olduğunda sahneyi yeniden yükleyecek
            // Şimdilik sadece değerleri resetle
            StartLevel(currentLevel, maxMoves);
        }
        
        /// <summary>
        /// Sonraki levele geçer.
        /// </summary>
        public void NextLevel()
        {
            currentLevel++;
            Debug.Log($"Sonraki level: {currentLevel}");
            
            // TODO: LevelManager ile level yükleme
            // Şimdilik varsayılan hamle sayısı ile başlat
            StartLevel(currentLevel, 10);
        }
        
        /// <summary>
        /// Oyunu duraklatır/devam ettirir.
        /// </summary>
        public void TogglePause()
        {
            if (currentState == GameState.Playing)
            {
                ChangeState(GameState.Paused);
                Time.timeScale = 0f;
            }
            else if (currentState == GameState.Paused)
            {
                ChangeState(GameState.Playing);
                Time.timeScale = 1f;
            }
        }
        
        /// <summary>
        /// Ana menüye döner.
        /// </summary>
        public void ReturnToMenu()
        {
            ChangeState(GameState.Menu);
            Time.timeScale = 1f;
            
            // TODO: Menu sahnesini yükle
            Debug.Log("Ana menüye dönülüyor...");
        }
        
        /// <summary>
        /// Oyun durumunu değiştirir.
        /// </summary>
        private void ChangeState(GameState newState)
        {
            currentState = newState;
            OnGameStateChanged?.Invoke();
            
            Debug.Log($"Game State: {currentState}");
        }
        
        /// <summary>
        /// Hamle ekler (power-up vb. için).
        /// </summary>
        public void AddMoves(int amount)
        {
            movesLeft += amount;
            OnMovesChanged?.Invoke(movesLeft);
            
            Debug.Log($"+{amount} hamle eklendi. Toplam: {movesLeft}");
        }
        
        #region Debug Methods
        
        [ContextMenu("Debug - Complete Level")]
        private void DebugCompleteLevel()
        {
            CompleteLevel();
        }
        
        [ContextMenu("Debug - Fail Level")]
        private void DebugFailLevel()
        {
            FailLevel();
        }
        
        [ContextMenu("Debug - Add 5 Moves")]
        private void DebugAddMoves()
        {
            AddMoves(5);
        }
        
        #endregion
    }
}