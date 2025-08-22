using UnityEngine;
using UnityEngine.UI;
using TMPro;
using PuzzleGame.Gameplay.Managers;
using DG.Tweening;

public class LevelFailureUI : UIPanel
{
    [Header("Failure Screen Elements")]
    [SerializeField] private Button retryButton;
    [SerializeField] private TextMeshProUGUI failureText;
    [SerializeField] private RectTransform panelRoot;

    public override void Initialize()
    {
        base.Initialize();

        if (retryButton != null)
            retryButton.onClick.AddListener(OnRetryClicked);

        if (panelRoot != null)
            panelRoot.localScale = Vector3.one;

        if (canvasGroup != null)
        {
            canvasGroup.alpha = 0f;
            canvasGroup.blocksRaycasts = false;
        }
    }

    protected override void OnShow()
    {
        UpdateUI();
        AudioManager.Instance?.PlayLevelFail();
        PlayEnterAnimation();
    }

    private void UpdateUI()
    {
        if (failureText != null && GameManager.Instance != null)
            failureText.text = $"Level {GameManager.Instance.CurrentLevel} Failed!";
    }

    private void OnRetryClicked()
    {
        AudioManager.Instance?.PlayButtonClick();
        
        if (LevelManager.Instance != null)
            LevelManager.Instance.RestartLevel();
    }

    private void PlayEnterAnimation()
    {
        if (canvasGroup != null)
        {
            canvasGroup.alpha = 0;
            canvasGroup.DOFade(1f, 0.5f).OnComplete(() => canvasGroup.blocksRaycasts = true);
        }

        if (panelRoot != null)
        {
            panelRoot.DOShakePosition(
                duration: 0.6f,
                strength: new Vector3(20f, 0, 0),
                vibrato: 20,
                randomness: 90
            );
        }

        if (failureText != null)
            failureText.transform.DOPunchScale(Vector3.one * 0.2f, 0.3f, 8, 0.7f);

        if (retryButton != null)
        {
            retryButton.transform
                .DOPunchScale(Vector3.one * 0.15f, 0.3f, 6, 0.6f)
                .SetDelay(0.3f);
        }
    }
}