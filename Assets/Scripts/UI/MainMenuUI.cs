using UnityEngine;
using PuzzleGame.Gameplay.Managers;
using TMPro;
using UnityEngine.UI;
using DG.Tweening;

public class MainMenuUI : UIPanel
{
    [Header("Menu Screen Elements (Per Requirements)")]
    [SerializeField] private TextMeshProUGUI gameTitleText;
    [SerializeField] private Button playButton;
    [SerializeField] private TextMeshProUGUI levelNumberText;

    [Header("Optional Level Selection")]
    [SerializeField] private Button previousLevelButton;
    [SerializeField] private Button nextLevelButton;
    [SerializeField] private int maxAvailableLevel = 4;

    private int selectedLevel = 1;

    public override void Initialize()
    {
        base.Initialize();
        
        if (playButton != null)
            playButton.onClick.AddListener(OnPlayButtonClicked);

        if (previousLevelButton != null)
            previousLevelButton.onClick.AddListener(OnPreviousLevelClicked);

        if (nextLevelButton != null)
            nextLevelButton.onClick.AddListener(OnNextLevelClicked);

        LoadSelectedLevel();
        UpdateUI();
    }

    protected override void OnShow()
    {
        LoadSelectedLevel();
        UpdateUI();
    }

    private void LoadSelectedLevel()
    {
        if (GameManager.Instance != null)
        {
            int savedLevel = GameManager.Instance.GetSavedLevel();
            selectedLevel = Mathf.Clamp(savedLevel, 1, maxAvailableLevel);
            GameManager.Instance.SetCurrentLevelWithoutStarting(selectedLevel);
        }
        else
        {
            selectedLevel = 1;
        }
    }

    private void UpdateUI()
    {
        if (gameTitleText != null)
            gameTitleText.text = "UNPUZZLE";

        if (levelNumberText != null)
            levelNumberText.text = $"LEVEL {selectedLevel}";

        var playButtonText = playButton?.GetComponentInChildren<TextMeshProUGUI>();
        if (playButtonText != null)
            playButtonText.text = "PLAY";

        UpdateLevelNavigationButtons();
    }

    private void UpdateLevelNavigationButtons()
    {
        if (previousLevelButton != null)
            previousLevelButton.interactable = selectedLevel > 1;

        if (nextLevelButton != null)
            nextLevelButton.interactable = selectedLevel < maxAvailableLevel;
    }

    private void OnPlayButtonClicked()
    {
        AudioManager.Instance?.PlayButtonClick();

        if (playButton != null)
        {
            playButton.transform
                .DOPunchScale(Vector3.one * 0.1f, 0.3f, 6, 0.6f)
                .OnComplete(() =>
                {
                    if (GameManager.Instance != null)
                    {
                        GameManager.Instance.SelectLevel(selectedLevel);
                    }
                });
        }
        else
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.SelectLevel(selectedLevel);
            }
        }
    }

    private void OnPreviousLevelClicked()
    {
        AudioManager.Instance?.PlayButtonClick();

        if (selectedLevel > 1)
        {
            selectedLevel--;
            GameManager.Instance?.SetCurrentLevelWithoutStarting(selectedLevel);
            UpdateUI();
        }
    }

    private void OnNextLevelClicked()
    {
        AudioManager.Instance?.PlayButtonClick();

        if (selectedLevel < maxAvailableLevel)
        {
            selectedLevel++;
            GameManager.Instance?.SetCurrentLevelWithoutStarting(selectedLevel);
            UpdateUI();
        }
    }

    private void OnDestroy()
    {
        if (playButton != null)
            playButton.onClick.RemoveListener(OnPlayButtonClicked);

        if (previousLevelButton != null)
            previousLevelButton.onClick.RemoveListener(OnPreviousLevelClicked);

        if (nextLevelButton != null)
            nextLevelButton.onClick.RemoveListener(OnNextLevelClicked);
    }
}