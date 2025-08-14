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

        // Events
        public event Action OnLevelComplete;
        public event Action OnLevelFailed;
        public event Action<int> OnMovesChanged; // kalan hamle sayısı
        public event Action OnGameStateChanged;

        // Properties
        public GameState CurrentState => currentState;
        public int CurrentLevel => currentLevel;
        public int MovesLeft => movesLeft;
        public int MaxMoves => maxMoves;

        protected override void Awake()
        {
            base.Awake();
            LoadProgress();
            ChangeState(GameState.Menu);
        }

        #region Level Control

        public void StartLevel(int levelIndex, int moveLimit)
        {
            // Pool temizle
            if (PoolManager.Instance != null)

            currentLevel = levelIndex;
            maxMoves = moveLimit;
            movesLeft = moveLimit;

            ChangeState(GameState.Playing);
            OnMovesChanged?.Invoke(movesLeft);

            Debug.Log($"Level {currentLevel} başlatıldı. Move Limit: {maxMoves}");
        }

        [ContextMenu("Quick Start Level")]
        public void QuickStartLevel()
        {
            StartLevel(currentLevel, maxMoves);
        }

        public bool UseMove()
        {
            if (movesLeft > 0 && currentState == GameState.Playing)
            {
                movesLeft--;
                OnMovesChanged?.Invoke(movesLeft);

                Debug.Log($"Move kullanıldı. Kalan: {movesLeft}/{maxMoves}");

                if (movesLeft <= 0)
                    FailLevel();

                return true;
            }
            return false;
        }

        public void CompleteLevel()
        {
            if (currentState != GameState.Playing) return;

            ChangeState(GameState.LevelComplete);
            OnLevelComplete?.Invoke();

            SaveProgress();

            Debug.Log($"Level {currentLevel} tamamlandı! Kullanılan hamle: {maxMoves - movesLeft}");
        }

        public void FailLevel()
        {
            if (currentState != GameState.Playing) return;

            ChangeState(GameState.LevelFailed);
            OnLevelFailed?.Invoke();

            Debug.Log($"Level {currentLevel} başarısız!");
        }

        public void RestartLevel()
        {
            Debug.Log($"Level {currentLevel} yeniden başlatılıyor...");
            StartLevel(currentLevel, maxMoves);
        }

        public void NextLevel()
        {
            currentLevel++;
            Debug.Log($"Sonraki level: {currentLevel}");
            StartLevel(currentLevel, maxMoves);
        }

        public void AddMoves(int amount)
        {
            movesLeft += amount;
            OnMovesChanged?.Invoke(movesLeft);
            Debug.Log($"+{amount} hamle eklendi. Toplam: {movesLeft}");
        }

        #endregion

        #region Game State

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

        public void ReturnToMenu()
        {
            ChangeState(GameState.Menu);
            Time.timeScale = 1f;

            // Menüye dönünce tüm objeleri havuza iade et
            Debug.Log("Ana menüye dönülüyor...");
        }

        private void ChangeState(GameState newState)
        {
            currentState = newState;
            OnGameStateChanged?.Invoke();
            Debug.Log($"Game State değişti: {currentState}");
        }

        #endregion

        #region Save / Load

        private void SaveProgress()
        {
            PlayerPrefs.SetInt("LastLevel", currentLevel);
            PlayerPrefs.Save();
        }

        private void LoadProgress()
        {
            if (PlayerPrefs.HasKey("LastLevel"))
                currentLevel = PlayerPrefs.GetInt("LastLevel");
        }

        #endregion

        #region Debug Methods

        [ContextMenu("Debug - Complete Level")]
        private void DebugCompleteLevel() => CompleteLevel();

        [ContextMenu("Debug - Fail Level")]
        private void DebugFailLevel() => FailLevel();

        [ContextMenu("Debug - Add 5 Moves")]
        private void DebugAddMoves() => AddMoves(5);

        #endregion
    }
}
