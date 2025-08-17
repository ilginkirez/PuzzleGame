using UnityEngine;
using UnityEngine.UI;
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

    [Header("UI Sound Effects")]
    [SerializeField] private AudioClip buttonClickSound;
    [SerializeField] private AudioClip panelOpenSound;
    [SerializeField] private AudioClip panelCloseSound;
    [SerializeField] private float sfxVolume = 0.7f;

    [Header("Debug")]
    [SerializeField] private bool enableDebugLogs = true;

    // UI Components
    private UIPanel currentPanel;
    private AudioSource uiAudioSource;

    // Properties
    public bool IsSFXMuted => PlayerPrefs.GetInt("SFXMuted", 0) == 1;
    public float SFXVolume => sfxVolume;

    protected override void Awake()
    {
        base.Awake();
        InitializeAudioSource();
        InitializePanels();
    }

    private void Start()
    {
        // Başlangıçta menü göster
        ShowMenuScreen();
        
        // Tüm butonlara ses ekle
        AddSoundToAllButtons();
    }

    private void OnEnable()
    {
        SubscribeToEvents();
    }

    private void OnDisable()
    {
        UnsubscribeFromEvents();
    }

    #region Audio System

    private void InitializeAudioSource()
    {
        uiAudioSource = GetComponent<AudioSource>();
        if (uiAudioSource == null)
        {
            uiAudioSource = gameObject.AddComponent<AudioSource>();
        }
        
        uiAudioSource.loop = false;
        uiAudioSource.volume = PlayerPrefs.GetFloat("SFXVolume", sfxVolume);
        uiAudioSource.playOnAwake = false;
        
        sfxVolume = uiAudioSource.volume;
        DebugLog("UI Audio system initialized");
    }

    public void PlayButtonSound()
    {
        if (buttonClickSound != null && !IsSFXMuted)
        {
            uiAudioSource.PlayOneShot(buttonClickSound);
        }
    }

    public void PlayPanelOpenSound()
    {
        if (panelOpenSound != null && !IsSFXMuted)
        {
            uiAudioSource.PlayOneShot(panelOpenSound);
        }
    }

    public void PlayPanelCloseSound()
    {
        if (panelCloseSound != null && !IsSFXMuted)
        {
            uiAudioSource.PlayOneShot(panelCloseSound);
        }
    }

    public void ToggleSFX()
    {
        bool newMutedState = !IsSFXMuted;
        PlayerPrefs.SetInt("SFXMuted", newMutedState ? 1 : 0);
        PlayerPrefs.Save();
        
        DebugLog($"SFX toggled: {(newMutedState ? "OFF" : "ON")}");
    }

    public void SetSFXVolume(float volume)
    {
        sfxVolume = Mathf.Clamp01(volume);
        uiAudioSource.volume = sfxVolume;
        PlayerPrefs.SetFloat("SFXVolume", sfxVolume);
        PlayerPrefs.Save();
        DebugLog($"SFX volume set to: {sfxVolume:F2}");
    }

    private void AddSoundToAllButtons()
    {
        // Scene'deki tüm butonları bul
        Button[] allButtons = FindObjectsOfType<Button>();
        
        foreach (var button in allButtons)
        {
            // Zaten listener varsa ekleme
            bool hasListener = false;
            for (int i = 0; i < button.onClick.GetPersistentEventCount(); i++)
            {
                if (button.onClick.GetPersistentMethodName(i) == "PlayButtonSound")
                {
                    hasListener = true;
                    break;
                }
            }
            
            if (!hasListener)
            {
                button.onClick.AddListener(() => PlayButtonSound());
            }
        }
        
        DebugLog($"Added sound to {allButtons.Length} UI buttons");
    }

    #endregion

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
            PlayPanelCloseSound();
        }

        // Yeni paneli aç
        currentPanel = targetPanel;
        currentPanel.Show();
        PlayPanelOpenSound();
        
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

    [ContextMenu("Debug - Toggle SFX")]
    private void DebugToggleSFX() => ToggleSFX();

    [ContextMenu("Debug - Play Button Sound")]
    private void DebugPlayButtonSound() => PlayButtonSound();

    #endregion
}