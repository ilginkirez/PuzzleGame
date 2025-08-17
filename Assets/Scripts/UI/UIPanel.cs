using UnityEngine;

using UnityEngine;

public abstract class UIPanel : MonoBehaviour
{
    [SerializeField] protected CanvasGroup canvasGroup; // protected yap → alt sınıflar erişebilsin

    public virtual void Initialize()
    {
        if (canvasGroup == null)
            canvasGroup = GetComponent<CanvasGroup>();

        if (canvasGroup == null)
            canvasGroup = gameObject.AddComponent<CanvasGroup>();
    }

    public virtual void Show()
    {
        gameObject.SetActive(true);

        if (canvasGroup != null)
        {
            canvasGroup.alpha = 1;
            canvasGroup.blocksRaycasts = true;
            canvasGroup.interactable = true; // 🔹 Etkileşim aç
        }

        OnShow();
    }

    public virtual void Hide(bool instant = false)
    {
        if (canvasGroup != null)
        {
            canvasGroup.alpha = 0;
            canvasGroup.blocksRaycasts = false;
            canvasGroup.interactable = false; // 🔹 Etkileşim kapat
        }

        gameObject.SetActive(false);
    }

    /// <summary>
    /// Panel açıldığında tetiklenecek ek davranışlar.
    /// Alt sınıflarda override edilebilir.
    /// </summary>
    protected virtual void OnShow() { }
}
