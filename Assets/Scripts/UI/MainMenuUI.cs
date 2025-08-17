using UnityEngine;
using PuzzleGame.Gameplay.Managers;
using TMPro;
using UnityEngine.UI;
using DG.Tweening; // ✅ DOTween eklendi

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

    [Header("Sound Effects")]
    [SerializeField] private AudioSource audioSource;       // 🔊 Ses kaynağı
    [SerializeField] private AudioClip buttonClickSound;    // 🔊 Buton tıklama sesi

    private int selectedLevel = 1;

    public override void Initialize()
    {
        base.Initialize();
        
        // 🔊 AudioSource yoksa ekle
        if (audioSource == null)
            audioSource = GetComponent<AudioSource>();
        
        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();
        
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

    // 🔊 Ses çalma metodu
    private void PlayButtonSound()
    {
        if (audioSource != null && buttonClickSound != null)
        {
            audioSource.PlayOneShot(buttonClickSound);
        }
    }

    private void OnPlayButtonClicked()
    {
        PlayButtonSound(); // 🔊 Ses çal

        if (playButton != null)
        {
            // 🔹 Önce animasyonu çalıştır
            playButton.transform
                .DOPunchScale(Vector3.one * 0.1f, 0.3f, 6, 0.6f)
                .OnComplete(() =>
                {
                    // 🔹 Animasyon bittikten sonra level başlat
                    if (GameManager.Instance != null)
                    {
                        GameManager.Instance.SelectLevel(selectedLevel);
                    }
                });
        }
        else
        {
            // Eğer animasyon yoksa direkt level başlat
            if (GameManager.Instance != null)
            {
                GameManager.Instance.SelectLevel(selectedLevel);
            }
        }
    }

    private void OnPreviousLevelClicked()
    {
        PlayButtonSound(); // 🔊 Ses çal

        if (selectedLevel > 1)
        {
            selectedLevel--;
            GameManager.Instance?.SetCurrentLevelWithoutStarting(selectedLevel);
            UpdateUI();
        }
    }

    private void OnNextLevelClicked()
    {
        PlayButtonSound(); // 🔊 Ses çal

        if (selectedLevel < maxAvailableLevel)
        {
            selectedLevel++;
            GameManager.Instance?.SetCurrentLevelWithoutStarting(selectedLevel);
            UpdateUI();
        }
    }

    #region Debug Methods
    
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
        if (playButton != null)
            playButton.onClick.RemoveListener(OnPlayButtonClicked);

        if (previousLevelButton != null)
            previousLevelButton.onClick.RemoveListener(OnPreviousLevelClicked);

        if (nextLevelButton != null)
            nextLevelButton.onClick.RemoveListener(OnNextLevelClicked);
    }
}