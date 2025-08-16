using UnityEngine;
using PuzzleGame.Core.Helpers;
using PuzzleGame.Core.Enums;
using System;

namespace PuzzleGame.Gameplay.Managers
{
    /// <summary>
    /// Oyunun genel akışını yöneten ana manager.
    /// Level durumu, hamle sayısı ve oyun state yönetimi.
    /// </summary>
    public class GameManager : Singleton<GameManager>
    {
        [Header("Game Settings")]
        [SerializeField] private int currentLevel = 1;
        [SerializeField] private int movesLeft;
        [SerializeField] private int maxMoves = 10;
        
        [Header("Debug")]
        [SerializeField] private GameState currentState = GameState.Menu;
        [SerializeField] private bool enableDebugLogs = true;

        // Events
        public event Action OnLevelComplete;
        public event Action OnLevelFailed;
        public event Action<int> OnMovesChanged;
        public event Action<GameState> OnGameStateChanged;
        public event Action<int, int> OnLevelStarted; // level, moves

        // Properties
        public GameState CurrentState => currentState;
        public int CurrentLevel => currentLevel;
        public int MovesLeft => movesLeft;
        public int MaxMoves => maxMoves;
        public bool IsPlaying => currentState == GameState.Playing;
        public bool CanUseMove => IsPlaying && movesLeft > 0;

        protected override void Awake()
        {
            base.Awake();
            // Awake'de currentLevel'ı değiştirme, menüde seçilecek
            SetGameState(GameState.Menu);
        }

        #region Level Control

        public void StartLevel(int levelIndex, int moveLimit)
        {
            if (moveLimit <= 0)
            {
                DebugLogWarning($"Invalid move limit: {moveLimit}. Using default: {maxMoves}");
                moveLimit = maxMoves;
            }

            currentLevel = Mathf.Max(1, levelIndex);
            maxMoves = moveLimit;
            movesLeft = moveLimit;
            
            SetGameState(GameState.Playing);
            OnMovesChanged?.Invoke(movesLeft);
            OnLevelStarted?.Invoke(currentLevel, maxMoves);
            
            DebugLog($"Level {currentLevel} başlatıldı. Move Limit: {maxMoves}");
        }

        [ContextMenu("Quick Start Level")]
        public void QuickStartLevel()
        {
            StartLevel(currentLevel, maxMoves);
        }

        public bool UseMove()
        {
            if (!CanUseMove)
            {
                DebugLogWarning($"Cannot use move. State: {currentState}, Moves: {movesLeft}");
                return false;
            }

            movesLeft--;
            OnMovesChanged?.Invoke(movesLeft);
            DebugLog($"Move kullanıldı. Kalan: {movesLeft}/{maxMoves}");

            if (movesLeft <= 0)
            {
                // Delay fail check to allow current move to complete
                Invoke(nameof(CheckForLevelFail), 0.1f);
            }

            return true;
        }

        private void CheckForLevelFail()
        {
            if (currentState == GameState.Playing && movesLeft <= 0)
            {
                FailLevel();
            }
        }

        public void CompleteLevel()
        {
            if (currentState != GameState.Playing)
            {
                DebugLogWarning($"Cannot complete level in state: {currentState}");
                return;
            }

            SetGameState(GameState.LevelComplete);
            OnLevelComplete?.Invoke();
            
            // Level tamamlandığında bir sonraki level'ı kaydet
            int nextLevel = currentLevel + 1;
            SaveProgressLevel(nextLevel);
            
            DebugLog($"Level {currentLevel} tamamlandı! Kullanılan hamle: {maxMoves - movesLeft}");
        }

        public void FailLevel()
        {
            if (currentState != GameState.Playing)
            {
                DebugLogWarning($"Cannot fail level in state: {currentState}");
                return;
            }

            SetGameState(GameState.LevelFailed);
            OnLevelFailed?.Invoke();
            DebugLog($"Level {currentLevel} başarısız!");
        }

        public void RestartLevel()
        {
            DebugLog($"Level {currentLevel} yeniden başlatılıyor...");
            
            // Cancel any pending fail checks
            CancelInvoke(nameof(CheckForLevelFail));
            
            StartLevel(currentLevel, maxMoves);
        }

        public void NextLevel()
        {
            int nextLevel = currentLevel + 1;
            DebugLog($"Sonraki level: {nextLevel}");
            
            // currentLevel'ı güncelle ama kaydetme, level seçimi UI'dan gelecek
            currentLevel = nextLevel;
        }

        public void AddMoves(int amount)
        {
            if (amount <= 0)
            {
                DebugLogWarning($"Invalid move amount to add: {amount}");
                return;
            }

            movesLeft += amount;
            OnMovesChanged?.Invoke(movesLeft);
            DebugLog($"+{amount} hamle eklendi. Toplam: {movesLeft}");
        }

        #endregion

        #region Level Selection

        /// <summary>
        /// Belirli bir level'ı başlatmak için kullan (UI'dan çağrılacak)
        /// </summary>
        public void SelectLevel(int levelIndex)
        {
            if (levelIndex <= 0)
            {
                DebugLogWarning($"Invalid level index: {levelIndex}");
                return;
            }

            currentLevel = levelIndex;
            DebugLog($"Level {currentLevel} seçildi");
            
            // LevelManager'a haber ver ki bu level'ı başlatsın
            if (LevelManager.Instance != null)
            {
                LevelManager.Instance.StartLevel(currentLevel);
            }
        }

        /// <summary>
        /// Mevcut kayıtlı progress level'ını döndürür (menü için)
        /// </summary>
        public int GetSavedLevel()
        {
            return PlayerPrefs.GetInt("LastLevel", 1);
        }

        /// <summary>
        /// Level seçiminde oyunu başlatmadan sadece currentLevel'ı günceller
        /// </summary>
        public void SetCurrentLevelWithoutStarting(int levelIndex)
        {
            if (levelIndex <= 0)
            {
                DebugLogWarning($"Invalid level index: {levelIndex}");
                return;
            }

            currentLevel = levelIndex;
            DebugLog($"Current level set to: {currentLevel} (without starting)");
        }

        #endregion

        #region Game State

        public void TogglePause()
        {
            switch (currentState)
            {
                case GameState.Playing:
                    SetGameState(GameState.Paused);
                    Time.timeScale = 0f;
                    break;
                case GameState.Paused:
                    SetGameState(GameState.Playing);
                    Time.timeScale = 1f;
                    break;
                default:
                    DebugLogWarning($"Cannot toggle pause from state: {currentState}");
                    break;
            }
        }

        public void ReturnToMenu()
        {
            CancelInvoke();
            
            SetGameState(GameState.Menu);
            Time.timeScale = 1f;
            // Menüye dönerken progress yükleme, level seçimi UI'dan gelecek
            DebugLog("Ana menüye dönülüyor...");
        }

        public void SetGameState(GameState newState)
        {
            if (currentState == newState)
                return;

            GameState previousState = currentState;
            currentState = newState;
            
            OnGameStateChanged?.Invoke(currentState);
            DebugLog($"Game State değişti: {previousState} -> {currentState}");
        }

        #endregion

        #region Save / Load

        private void SaveProgress()
        {
            SaveProgressLevel(currentLevel);
        }

        private void SaveProgressLevel(int levelToSave)
        {
            try
            {
                PlayerPrefs.SetInt("LastLevel", levelToSave);
                PlayerPrefs.SetInt("GameVersion", 1);
                PlayerPrefs.Save();
                DebugLog($"Progress saved: Level {levelToSave}");
            }
            catch (System.Exception e)
            {
                DebugLogError($"Failed to save progress: {e.Message}");
            }
        }

        private void LoadProgress()
        {
            try
            {
                if (PlayerPrefs.HasKey("LastLevel"))
                {
                    int savedLevel = PlayerPrefs.GetInt("LastLevel", 1);
                    DebugLog($"Progress loaded (saved level): {savedLevel}");
                }
            }
            catch (System.Exception e)
            {
                DebugLogError($"Failed to load progress: {e.Message}");
            }
        }

        #endregion

        #region Debug Methods

        private void DebugLog(string message)
        {
            if (enableDebugLogs)
                Debug.Log($"[GameManager] {message}");
        }

        private void DebugLogWarning(string message)
        {
            if (enableDebugLogs)
                Debug.LogWarning($"[GameManager] {message}");
        }

        private void DebugLogError(string message)
        {
            Debug.LogError($"[GameManager] {message}");
        }

        [ContextMenu("Debug - Complete Level")]
        private void DebugCompleteLevel() => CompleteLevel();

        [ContextMenu("Debug - Fail Level")]
        private void DebugFailLevel() => FailLevel();

        [ContextMenu("Debug - Add 5 Moves")]
        private void DebugAddMoves() => AddMoves(5);

        [ContextMenu("Debug - Reset Progress")]
        private void DebugResetProgress()
        {
            PlayerPrefs.DeleteAll();
            currentLevel = 1;
            DebugLog("Progress reset");
        }

        [ContextMenu("Debug - Show Current Values")]
        private void DebugShowValues()
        {
            DebugLog($"Current Level: {currentLevel}");
            DebugLog($"Saved Level: {GetSavedLevel()}");
            DebugLog($"Current State: {currentState}");
        }

        #endregion

        #region Unity Lifecycle

        private void OnDestroy()
        {
            // Cleanup
            CancelInvoke();
        }

        private void OnApplicationPause(bool pauseStatus)
        {
            if (pauseStatus && IsPlaying)
            {
                SetGameState(GameState.Paused);
                Time.timeScale = 0f;
            }
        }

        #endregion
    }
}