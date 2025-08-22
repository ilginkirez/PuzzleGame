using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;
using PuzzleGame.Gameplay.Managers;

public class LevelSuccessUI : UIPanel
{
    [Header("Success Screen Elements")]
    [SerializeField] private RectTransform panelRoot;      
    [SerializeField] private Button nextLevelButton;         
    [SerializeField] private TextMeshProUGUI successText;    

    [Header("Visual Effects")]
    [SerializeField] private ParticleSystem celebrationFX;

    public override void Initialize()
    {
        base.Initialize();

        if (nextLevelButton != null)
            nextLevelButton.onClick.AddListener(OnNextLevelClicked);

        if (panelRoot != null)
            panelRoot.localScale = Vector3.one;
    }

    protected override void OnShow()
    {
        UpdateUI();
        PlayCelebrationEffects();
        PlayEnterAnimation();
    }

    private void UpdateUI()
    {
        if (successText != null && GameManager.Instance != null)
        {
            int completedLevel = GameManager.Instance.CurrentLevel;
            successText.text = $"Level {completedLevel} Complete!";
        }

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
        if (celebrationFX != null)
            celebrationFX.Play();

        AudioManager.Instance?.PlayLevelComplete();
    }
    
    private void PlayEnterAnimation()
    {
        if (canvasGroup != null)
        {
            canvasGroup.alpha = 0;
            canvasGroup.DOFade(1f, 0.5f);
        }

        if (panelRoot != null)
            panelRoot.DOPunchScale(Vector3.one * 0.1f, 0.4f, 8, 0.5f);

        if (successText != null)
            successText.transform.DOPunchScale(Vector3.one * 0.15f, 0.3f, 6, 0.6f);

        if (nextLevelButton != null && nextLevelButton.gameObject.activeSelf)
        {
            nextLevelButton.transform
                .DOPunchScale(Vector3.one * 0.1f, 0.3f, 6, 0.6f)
                .SetDelay(0.3f);
        }
    }

    private void OnNextLevelClicked()
    {
        AudioManager.Instance?.PlayButtonClick();
        
        if (LevelManager.Instance != null)
            LevelManager.Instance.NextLevel();
    }
}