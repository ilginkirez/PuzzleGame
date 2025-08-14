using UnityEngine;
using UnityEngine.UI;
using TMPro;
using PuzzleGame.Gameplay.Managers;

public class LevelFailureUI : UIPanel
{
    [Header("Failure Screen Elements (Per Requirements)")]
    [SerializeField] private Button retryButton;            // "Retry" button
    [SerializeField] private TextMeshProUGUI failureText;   // "Level Failed!" etc.

    // Optional elements
    [SerializeField] private AudioSource failureSound;

    public override void Initialize()
    {
        base.Initialize();
        
        if (retryButton != null)
            retryButton.onClick.AddListener(OnRetryClicked);
    }

    protected override void OnShow()
    {
        UpdateUI();
        PlayFailureEffects();
    }

    private void UpdateUI()
    {
        if (failureText != null && GameManager.Instance != null)
        {
            int failedLevel = GameManager.Instance.CurrentLevel;
            failureText.text = $"Level {failedLevel} Failed!";
            failureText.color = Color.red;
        }
    }

    private void PlayFailureEffects()
    {
        // Failure sound
        if (failureSound != null)
            failureSound.Play();
    }

    private void OnRetryClicked()
    {
        // Restart current level
        if (LevelManager.Instance != null)
        {
            LevelManager.Instance.RestartLevel();
        }
    }
}