using UnityEngine;
using UnityEngine.UI;
using TMPro;
using PuzzleGame.Gameplay.Managers;

public class GameplayUI : UIPanel
{
    [Header("Game Screen Elements (Per Requirements)")]
    [SerializeField] private TextMeshProUGUI movesLeftText;  // "Moves Left: X"
    [SerializeField] private Button resetButton;             // "Reset" button

    // Optional: Level indicator (not required but helpful)
    [SerializeField] private TextMeshProUGUI currentLevelText;

    public override void Initialize()
    {
        base.Initialize();
        
        if (resetButton != null)
            resetButton.onClick.AddListener(OnResetButtonClicked);

        // Listen to GameManager events
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnMovesChanged += UpdateMovesDisplay;
        }
    }

    protected override void OnShow()
    {
        UpdateUI();
    }

    private void UpdateUI()
    {
        // Current level (optional display)
        if (currentLevelText != null && GameManager.Instance != null)
        {
            currentLevelText.text = $"Level {GameManager.Instance.CurrentLevel}";
        }

        // Moves left
        if (GameManager.Instance != null)
            UpdateMovesDisplay(GameManager.Instance.MovesLeft);
        else
            UpdateMovesDisplay(0);
    }

    private void UpdateMovesDisplay(int movesLeft)
    {
        if (movesLeftText != null)
        {
            movesLeftText.text = $"Moves {movesLeft}";
            movesLeftText.color = (movesLeft <= 2) ? Color.red : Color.white;
        }
    }

    private void OnResetButtonClicked()
    {
        // Restart current level
        if (LevelManager.Instance != null)
        {
            LevelManager.Instance.RestartLevel();
        }
    }

    private void OnDestroy()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnMovesChanged -= UpdateMovesDisplay;
        }
    }
}
