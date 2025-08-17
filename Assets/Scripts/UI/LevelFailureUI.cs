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

    [Header("Optional Elements")]
    [SerializeField] private AudioSource failureSound;

    [SerializeField] private RectTransform panelRoot; // 🔹 UI root (animasyon için)

    public override void Initialize()
    {
        base.Initialize();

        if (retryButton != null)
            retryButton.onClick.AddListener(OnRetryClicked);

        if (panelRoot != null)
            panelRoot.localScale = Vector3.one; // default scale

        // Başlangıçta görünmez yap
        if (canvasGroup != null)
        {
            canvasGroup.alpha = 0f;
            canvasGroup.blocksRaycasts = false;
        }
    }

    protected override void OnShow()
    {
        UpdateUI();
        PlayFailureEffects();
        PlayEnterAnimation();
    }

    private void UpdateUI()
    {
        if (failureText != null && GameManager.Instance != null)
            failureText.text = $"Level {GameManager.Instance.CurrentLevel} Failed!";
    }

    private void PlayFailureEffects()
    {
        if (failureSound != null)
            failureSound.Play();
    }

    private void OnRetryClicked()
    {
        if (LevelManager.Instance != null)
            LevelManager.Instance.RestartLevel();
    }

    private void PlayEnterAnimation()
    {
        // Fade-in
        if (canvasGroup != null)
        {
            canvasGroup.alpha = 0;
            canvasGroup.DOFade(1f, 0.5f).OnComplete(() => canvasGroup.blocksRaycasts = true);
        }

        // Panel shake (fail efekti)
        if (panelRoot != null)
        {
            panelRoot.DOShakePosition(
                duration: 0.6f,
                strength: new Vector3(20f, 0, 0),
                vibrato: 20,
                randomness: 90
            );
        }

        // Text punch
        if (failureText != null)
            failureText.transform.DOPunchScale(Vector3.one * 0.2f, 0.3f, 8, 0.7f);

        // Retry button bounce (gecikmeli)
        if (retryButton != null)
        {
            retryButton.transform
                .DOPunchScale(Vector3.one * 0.15f, 0.3f, 6, 0.6f)
                .SetDelay(0.3f);
        }
    }
}
