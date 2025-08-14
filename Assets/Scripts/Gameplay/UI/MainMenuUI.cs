using UnityEngine;
using PuzzleGame.Gameplay.Managers;
using TMPro;
using UnityEngine.UI;

public class MainMenuUI : UIPanel
{
    [Header("Menu Screen Elements (Per Requirements)")]
    [SerializeField] private TextMeshProUGUI gameTitleText;  // "Unpuzzle" 
    [SerializeField] private Button playButton;              // "Play" button
    [SerializeField] private TextMeshProUGUI levelNumberText; // Current level number

    public override void Initialize()
    {
        base.Initialize();
        
        if (playButton != null)
            playButton.onClick.AddListener(OnPlayButtonClicked);

        UpdateUI();
    }

    protected override void OnShow()
    {
        UpdateUI();
    }

    private void UpdateUI()
    {
        // Game title
        if (gameTitleText != null)
            gameTitleText.text = "UNPUZZLE";

        // Current level number on play button
        if (levelNumberText != null && GameManager.Instance != null)
        {
            int currentLevel = GameManager.Instance.CurrentLevel;
            levelNumberText.text = $"LEVEL {currentLevel}";
        }

        // Play button text
        var playButtonText = playButton.GetComponentInChildren<TextMeshProUGUI>();
        if (playButtonText != null)
        {
            playButtonText.text = "PLAY";
        }
    }

    private void OnPlayButtonClicked()
    {
        // Start current level
        if (LevelManager.Instance != null && GameManager.Instance != null)
        {
            int levelToStart = GameManager.Instance.CurrentLevel;
            LevelManager.Instance.StartLevel(levelToStart);
        }
    }
}
