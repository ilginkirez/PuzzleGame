using UnityEngine;

public abstract class UIPanel : MonoBehaviour
{
    [SerializeField] private CanvasGroup canvasGroup;

    public virtual void Initialize()
    {
        if (canvasGroup == null)
            canvasGroup = GetComponent<CanvasGroup>();
    }

    public virtual void Show()
    {
        gameObject.SetActive(true);

        if (canvasGroup != null)
        {
            canvasGroup.alpha = 1;
            canvasGroup.blocksRaycasts = true;
        }

        OnShow();
    }

    public virtual void Hide(bool instant = false)
    {
        if (canvasGroup != null)
        {
            canvasGroup.alpha = 0;
            canvasGroup.blocksRaycasts = false;
        }

        gameObject.SetActive(false);
    }

    /// <summary>
    /// Panel açıldığında tetiklenecek ek davranışlar.
    /// Alt sınıflarda override edilebilir.
    /// </summary>
    protected virtual void OnShow() { }
}