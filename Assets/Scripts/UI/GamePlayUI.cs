using UnityEngine;
using UnityEngine.UI;
using TMPro;
using PuzzleGame.Gameplay.Managers;

public class GameplayUI : UIPanel
{
    [Header("Game Screen Elements (Per Requirements)")]
    [SerializeField] private TextMeshProUGUI movesLeftText;
    [SerializeField] private Button resetButton;

    [Header("Optional Elements")]
    [SerializeField] private TextMeshProUGUI currentLevelText;

    public override void Initialize()
    {
        base.Initialize();
        
        if (resetButton != null)
            resetButton.onClick.AddListener(OnResetButtonClicked);

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
        if (currentLevelText != null && GameManager.Instance != null)
        {
            currentLevelText.text = $"Level {GameManager.Instance.CurrentLevel}";
        }

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
        AudioManager.Instance?.PlayButtonClick();
        
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