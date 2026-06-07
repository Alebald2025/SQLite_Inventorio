using UnityEngine;
using UnityEngine.SceneManagement;
using DG.Tweening;

public class CanvasManager : MonoBehaviour
{
    [Header("Configuración")]
    [SerializeField] private RectTransform[] todosLosPaneles;
    [SerializeField] private int panelInicial = 0;

    [Header("Animación del título")]
    [SerializeField] private RectTransform titleRect;
    [SerializeField] private Vector2 titleTargetAnchoredPosition = new Vector2(0f, 200f);
    [SerializeField] private Vector3 titleTargetScale = new Vector3(0.65f, 0.65f, 0.65f);
    [SerializeField] private float titleAnimationDuration = 0.6f;
    [SerializeField] private Ease titleEase = Ease.OutCubic;
    [SerializeField] private float panelFadeDuration = 0.5f;

    private CanvasGroup[] gruposPanel;
    private bool titleIsAtTop;
    private bool hasStarted;
    private int currentPanelIndex = -1;
    private Vector2 titleOriginalAnchoredPosition;
    private Vector3 titleOriginalScale;

    void Start()
    {
        if (titleRect != null)
        {
            titleOriginalAnchoredPosition = titleRect.anchoredPosition;
            titleOriginalScale = titleRect.localScale;
        }

        InicializarPaneles();
        MostrarPanel(panelInicial);
        hasStarted = true;
    }

    // Llama a este método desde el botón Login del MainMenu
    public void IrALoginOJuego(int indicePanelLogin)
    {
        if (PlayerPrefs.HasKey("CurrentUserID"))
            SceneManager.LoadScene("Game");
        else
            MostrarPanel(indicePanelLogin);
    }

    public void MostrarPanel(int indice)
    {
        if (indice < 0 || indice >= todosLosPaneles.Length) return;
        if (indice == currentPanelIndex) return;

        if (!hasStarted)
        {
            SetActivePanel(indice);
            currentPanelIndex = indice;
            return;
        }

        if (indice == panelInicial)
        {
            FadeOutPanel(currentPanelIndex, panelFadeDuration);
            AnimateTitleToOriginal(() => FadeInPanel(indice, panelFadeDuration));
        }
        else if (currentPanelIndex == panelInicial)
        {
            FadeOutPanel(currentPanelIndex, panelFadeDuration);
            AnimateTitleToTop(() => FadeInPanel(indice, panelFadeDuration));
        }
        else
        {
            FadeOutPanel(currentPanelIndex, panelFadeDuration, () => FadeInPanel(indice, panelFadeDuration));
        }
    }

    public void MostrarPanelPorNombre(string nombrePanel)
    {
        for (int i = 0; i < todosLosPaneles.Length; i++)
        {
            if (todosLosPaneles[i].name == nombrePanel)
            {
                MostrarPanel(i);
                return;
            }
        }

        Debug.LogWarning("Panel '" + nombrePanel + "' no encontrado!");
    }

    private void InicializarPaneles()
    {
        gruposPanel = new CanvasGroup[todosLosPaneles.Length];

        for (int i = 0; i < todosLosPaneles.Length; i++)
        {
            gruposPanel[i] = todosLosPaneles[i].GetComponent<CanvasGroup>();
            if (gruposPanel[i] == null)
                gruposPanel[i] = todosLosPaneles[i].gameObject.AddComponent<CanvasGroup>();

            gruposPanel[i].alpha = 0f;
            gruposPanel[i].blocksRaycasts = false;
            todosLosPaneles[i].gameObject.SetActive(false);
        }
    }

    private void SwitchPanel(int indice)
    {
        SetActivePanel(indice);
        currentPanelIndex = indice;
    }

    private void FadeOutPanel(int indice, float duration, System.Action onComplete = null)
    {
        if (indice < 0 || indice >= gruposPanel.Length) 
        {
            onComplete?.Invoke();
            return;
        }

        CanvasGroup group = gruposPanel[indice];
        if (group == null)
        {
            onComplete?.Invoke();
            return;
        }

        group.DOKill(true);
        group.blocksRaycasts = false;
        group.DOFade(0f, duration).SetEase(titleEase).OnComplete(() =>
        {
            todosLosPaneles[indice].gameObject.SetActive(false);
            onComplete?.Invoke();
        });
    }

    private void FadeInPanel(int indice, float duration)
    {
        if (indice < 0 || indice >= gruposPanel.Length) return;

        CanvasGroup group = gruposPanel[indice];
        if (group == null) return;

        todosLosPaneles[indice].gameObject.SetActive(true);
        group.alpha = 0f;
        group.blocksRaycasts = false;
        group.DOKill(true);
        group.DOFade(1f, duration).SetEase(titleEase).OnComplete(() =>
        {
            group.blocksRaycasts = true;
            currentPanelIndex = indice;
        });
    }

    private void SetActivePanel(int indice)
    {
        for (int i = 0; i < todosLosPaneles.Length; i++)
        {
            bool isTarget = i == indice;
            todosLosPaneles[i].gameObject.SetActive(isTarget);

            if (gruposPanel[i] != null)
            {
                gruposPanel[i].alpha = isTarget ? 1f : 0f;
                gruposPanel[i].blocksRaycasts = isTarget;
            }
        }
    }

    private void AnimateTitleToTop(System.Action onComplete = null)
    {
        if (titleRect == null || titleIsAtTop)
        {
            onComplete?.Invoke();
            return;
        }

        titleIsAtTop = true;
        titleRect.DOKill(true);

        Sequence seq = DOTween.Sequence();
        seq.Append(titleRect.DOAnchorPos(titleTargetAnchoredPosition, titleAnimationDuration).SetEase(titleEase));
        seq.Join(titleRect.DOScale(titleTargetScale, titleAnimationDuration).SetEase(titleEase));
        seq.OnComplete(() => onComplete?.Invoke());
        seq.SetTarget(titleRect);
    }

    private void AnimateTitleToOriginal(System.Action onComplete = null)
    {
        if (titleRect == null || !titleIsAtTop)
        {
            onComplete?.Invoke();
            return;
        }

        titleIsAtTop = false;
        titleRect.DOKill(true);

        Sequence seq = DOTween.Sequence();
        seq.Append(titleRect.DOAnchorPos(titleOriginalAnchoredPosition, titleAnimationDuration).SetEase(titleEase));
        seq.Join(titleRect.DOScale(titleOriginalScale, titleAnimationDuration).SetEase(titleEase));
        seq.OnComplete(() => onComplete?.Invoke());
        seq.SetTarget(titleRect);
    }
}
