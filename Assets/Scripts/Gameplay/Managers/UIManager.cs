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

    private UIPanel currentPanel;

    protected override void Awake()
    {
        base.Awake();
        InitializePanels();
    }

    private void OnEnable()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnLevelComplete += ShowLevelSuccess;
            GameManager.Instance.OnLevelFailed += ShowLevelFailure;
            GameManager.Instance.OnGameStateChanged += HandleGameStateChanged;
        }
    }

    private void OnDisable()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnLevelComplete -= ShowLevelSuccess;
            GameManager.Instance.OnLevelFailed -= ShowLevelFailure;
            GameManager.Instance.OnGameStateChanged -= HandleGameStateChanged;
        }
    }

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
        if (targetPanel == null) return;

        // Tüm panelleri kapat
        menuScreen?.Hide(true);
        gameScreen?.Hide(true);
        successScreen?.Hide(true);
        failureScreen?.Hide(true);

        // Yeni paneli aç
        currentPanel = targetPanel;
        currentPanel.Show();
    }

    private void HandleGameStateChanged()
    {
        if (GameManager.Instance == null) return;

        switch (GameManager.Instance.CurrentState)
        {
            case GameState.Menu:
                ShowMenuScreen(); // sadece MainMenuUI görünür
                break;
            case GameState.Playing:
                ShowGameScreen(); // sadece GameplayUI görünür
                break;
            case GameState.LevelComplete:
                ShowLevelSuccess();
                break;
            case GameState.LevelFailed:
                ShowLevelFailure();
                break;
        }
    }
}
