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

    [Header("Optional Level Selection")]
    [SerializeField] private Button previousLevelButton;     // "<" button for level selection
    [SerializeField] private Button nextLevelButton;         // ">" button for level selection
    [SerializeField] private int maxAvailableLevel = 4;      // Maximum available level

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
            // İlk açılışta kayıtlı level'ı yükle
            int savedLevel = GameManager.Instance.GetSavedLevel();
            selectedLevel = Mathf.Clamp(savedLevel, 1, maxAvailableLevel);
            
            // GameManager'daki currentLevel'ı da güncelle (ama oyunu başlatma)
            GameManager.Instance.SetCurrentLevelWithoutStarting(selectedLevel);
        }
        else
        {
            selectedLevel = 1;
        }
    }

    private void UpdateUI()
    {
        // Game title
        if (gameTitleText != null)
            gameTitleText.text = "UNPUZZLE";

        // Current level number
        if (levelNumberText != null)
        {
            levelNumberText.text = $"LEVEL {selectedLevel}";
        }

        // Play button text
        var playButtonText = playButton?.GetComponentInChildren<TextMeshProUGUI>();
        if (playButtonText != null)
        {
            playButtonText.text = "PLAY";
        }

        // Level navigation buttons
        UpdateLevelNavigationButtons();
    }

    private void UpdateLevelNavigationButtons()
    {
        if (previousLevelButton != null)
        {
            previousLevelButton.interactable = selectedLevel > 1;
        }

        if (nextLevelButton != null)
        {
            nextLevelButton.interactable = selectedLevel < maxAvailableLevel;
        }
    }

    private void OnPlayButtonClicked()
    {
        if (GameManager.Instance != null)
        {
            // Seçili level'ı GameManager'a bildir ve başlat
            GameManager.Instance.SelectLevel(selectedLevel);
        }
    }

    private void OnPreviousLevelClicked()
    {
        if (selectedLevel > 1)
        {
            selectedLevel--;
            // GameManager'daki currentLevel'ı da güncelle
            if (GameManager.Instance != null)
                GameManager.Instance.SetCurrentLevelWithoutStarting(selectedLevel);
            UpdateUI();
        }
    }

    private void OnNextLevelClicked()
    {
        if (selectedLevel < maxAvailableLevel)
        {
            selectedLevel++;
            // GameManager'daki currentLevel'ı da güncelle
            if (GameManager.Instance != null)
                GameManager.Instance.SetCurrentLevelWithoutStarting(selectedLevel);
            UpdateUI();
        }
    }

    #region Debug Methods (Inspector'da test için)
    
    [ContextMenu("Debug - Set Level 1")]
    private void DebugSetLevel1()
    {
        selectedLevel = 1;
        UpdateUI();
    }

    [ContextMenu("Debug - Set Level 4")]
    private void DebugSetLevel4()
    {
        selectedLevel = 4;
        UpdateUI();
    }

    #endregion

    private void OnDestroy()
    {
        // Button listener'ları temizle
        if (playButton != null)
            playButton.onClick.RemoveListener(OnPlayButtonClicked);

        if (previousLevelButton != null)
            previousLevelButton.onClick.RemoveListener(OnPreviousLevelClicked);

        if (nextLevelButton != null)
            nextLevelButton.onClick.RemoveListener(OnNextLevelClicked);
    }
}