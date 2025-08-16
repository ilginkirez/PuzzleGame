using UnityEngine;
using UnityEngine.UI;
using TMPro;
using PuzzleGame.Gameplay.Managers;

public class LevelSuccessUI : UIPanel
{
    [Header("Success Screen Elements (Per Requirements)")]
    [SerializeField] private Button nextLevelButton;         // "Next Level" button
    [SerializeField] private TextMeshProUGUI successText;    // "Level Complete!" etc.

    // Optional celebration elements
    [SerializeField] private ParticleSystem celebrationFX;
    [SerializeField] private AudioSource successSound;

    public override void Initialize()
    {
        base.Initialize();
        
        if (nextLevelButton != null)
            nextLevelButton.onClick.AddListener(OnNextLevelClicked);
    }

    protected override void OnShow()
    {
        UpdateUI();
        PlayCelebrationEffects();
    }

    private void UpdateUI()
    {
        if (successText != null && GameManager.Instance != null)
        {
            int completedLevel = GameManager.Instance.CurrentLevel;
            successText.text = $"Level {completedLevel} Complete!";
        }

        // Check if there's a next level
        if (nextLevelButton != null && LevelManager.Instance != null)
        {
            bool hasNext = LevelManager.Instance.HasNextLevel;
            nextLevelButton.gameObject.SetActive(hasNext);
            
            if (!hasNext && successText != null)
            {
                successText.text = "All Levels Complete!";
            }
        }
    }

    private void PlayCelebrationEffects()
    {
        // Particle effects
        if (celebrationFX != null)
            celebrationFX.Play();

        // Success sound
        if (successSound != null)
            successSound.Play();
    }

    private void OnNextLevelClicked()
    {
        // Go to next level
        if (LevelManager.Instance != null)
        {
            LevelManager.Instance.NextLevel();
        }
    }
}