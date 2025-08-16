using UnityEngine;
using PuzzleGame.Core.Helpers;    // Singleton
using PuzzleGame.Core.Enums;      // GameState
using PuzzleGame.Gameplay.Managers;

public class UIManager : Singleton<UIManager>
{
    [Header("Required UI Panels")]
    [SerializeField] private MainMenuUI menuScreen;
    [SerializeField] private GameplayUI gameScreen;
    [SerializeField] private LevelSuccessUI successScreen;
    [SerializeField] private LevelFailureUI failureScreen;

    [Header("Debug")]
    [SerializeField] private bool enableDebugLogs = true;

    private UIPanel currentPanel;

    protected override void Awake()
    {
        base.Awake();
        InitializePanels();
    }

    private void Start()
    {
        // Başlangıçta menü göster
        ShowMenuScreen();
    }

    private void OnEnable()
    {
        SubscribeToEvents();
    }

    private void OnDisable()
    {
        UnsubscribeFromEvents();
    }

    #region Event Management

    private void SubscribeToEvents()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnLevelComplete += ShowLevelSuccess;
            GameManager.Instance.OnLevelFailed += ShowLevelFailure;
            GameManager.Instance.OnGameStateChanged += HandleGameStateChanged;
        }
    }

    private void UnsubscribeFromEvents()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnLevelComplete -= ShowLevelSuccess;
            GameManager.Instance.OnLevelFailed -= ShowLevelFailure;
            GameManager.Instance.OnGameStateChanged -= HandleGameStateChanged;
        }
    }

    #endregion

    #region Panel Management

    private void InitializePanels()
    {
        UIPanel[] panels = { menuScreen, gameScreen, successScreen, failureScreen };

        foreach (var panel in panels)
        {
            if (panel != null)
            {
                panel.Initialize();
                panel.Hide(true); // başta hepsini kapat
            }
            else
            {
                Debug.LogWarning("UIManager: Missing panel reference in Inspector!");
            }
        }
    }

    public void ShowMenuScreen() => ShowPanel(menuScreen);
    public void ShowGameScreen() => ShowPanel(gameScreen);
    public void ShowLevelSuccess() => ShowPanel(successScreen);
    public void ShowLevelFailure() => ShowPanel(failureScreen);

    private void ShowPanel(UIPanel targetPanel)
    {
        if (targetPanel == null) 
        {
            DebugLogWarning("Trying to show null panel!");
            return;
        }

        // Mevcut paneli kapat
        if (currentPanel != null)
        {
            currentPanel.Hide();
        }

        // Yeni paneli aç
        currentPanel = targetPanel;
        currentPanel.Show();
        
        DebugLog($"Showing panel: {targetPanel.GetType().Name}");
    }

    private void HandleGameStateChanged(GameState newState)
    {
        DebugLog($"Game state changed to: {newState}");

        switch (newState)
        {
            case GameState.Menu:
                ShowMenuScreen();
                break;
            case GameState.Playing:
                ShowGameScreen();
                break;
            case GameState.LevelComplete:
                ShowLevelSuccess();
                break;
            case GameState.LevelFailed:
                ShowLevelFailure();
                break;
            case GameState.Paused:
                // Pause UI gösterilebilir veya mevcut panel korunabilir
                break;
        }
    }

    #endregion

    #region Button Event Handlers

    /// <summary>
    /// Menüdeki "Play" butonu için - MainMenuUI'dan çağrılmayacak artık
    /// </summary>
    public void OnPlayButtonPressed()
    {
        DebugLog("Play button pressed from UIManager (deprecated - use MainMenuUI.OnPlayButtonClicked)");
        
        if (GameManager.Instance != null)
        {
            // Level seçimi MainMenuUI'da yapılacak
            int levelToStart = GameManager.Instance.CurrentLevel;
            GameManager.Instance.SelectLevel(levelToStart);
        }
    }

    /// <summary>
    /// Success ekranındaki "Next Level" butonu için
    /// </summary>
    public void OnNextLevelButtonPressed()
    {
        DebugLog("Next level button pressed");
        
        // Sadece LevelManager'ın NextLevel metodunu çağır
        // GameManager otomatik olarak güncellenir
        if (LevelManager.Instance != null)
        {
            LevelManager.Instance.NextLevel();
        }
        else
        {
            DebugLogWarning("LevelManager instance not found!");
        }
    }

    /// <summary>
    /// Failure ekranındaki "Retry" butonu için
    /// </summary>
    public void OnRetryButtonPressed()
    {
        DebugLog("Retry button pressed");
        
        // Sadece LevelManager'ın RestartLevel metodunu çağır
        if (LevelManager.Instance != null)
        {
            LevelManager.Instance.RestartLevel();
        }
        else
        {
            DebugLogWarning("LevelManager instance not found!");
        }
    }

    /// <summary>
    /// Tüm ekranlarda "Main Menu" butonu için
    /// </summary>
    public void OnMainMenuButtonPressed()
    {
        DebugLog("Main menu button pressed");
        
        if (GameManager.Instance != null)
        {
            GameManager.Instance.ReturnToMenu();
        }
    }

    #endregion

    #region Debug Methods

    private void DebugLog(string message)
    {
        if (enableDebugLogs)
            Debug.Log($"[UIManager] {message}");
    }

    private void DebugLogWarning(string message)
    {
        if (enableDebugLogs)
            Debug.LogWarning($"[UIManager] {message}");
    }

    [ContextMenu("Debug - Show Menu")]
    private void DebugShowMenu() => ShowMenuScreen();

    [ContextMenu("Debug - Show Game")]
    private void DebugShowGame() => ShowGameScreen();

    #endregion
}