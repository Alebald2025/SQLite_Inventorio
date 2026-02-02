using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CanvasManager : MonoBehaviour
{
    [Header("Configuración")]
    [SerializeField] private RectTransform[] todosLosPaneles; 
    [SerializeField] private int panelInicial = 0; 
    [SerializeField] private float duracionFade = 0.3f; 
    [SerializeField] private Ease easeIn = Ease.OutQuad;
    [SerializeField] private Ease easeOut = Ease.InQuad;

    private CanvasGroup[] gruposPanel;
    private Sequence secuenciaActual;

    void Start()
    {
        InicializarPaneles();
        MostrarPanel(panelInicial);
    }

    public void MostrarPanel(int indice)
    {
        if (indice < 0 || indice >= todosLosPaneles.Length) return;

        DOTween.Kill(secuenciaActual);
        secuenciaActual = DOTween.Sequence();

        for (int i = 0; i < todosLosPaneles.Length; i++)
        {
            if (i != indice && todosLosPaneles[i].gameObject.activeInHierarchy)
            {
                int idx = i;
                secuenciaActual.Join(gruposPanel[idx].DOFade(0f, duracionFade).SetEase(easeOut)
                    .OnComplete(() => {
                        gruposPanel[idx].blocksRaycasts = false;
                        todosLosPaneles[idx].gameObject.SetActive(false);
                    }));
            }
        }

        int targetIdx = indice;
        todosLosPaneles[targetIdx].gameObject.SetActive(true);
        gruposPanel[targetIdx].alpha = 0f;
        gruposPanel[targetIdx].blocksRaycasts = false;

        secuenciaActual.Append(gruposPanel[targetIdx].DOFade(1f, duracionFade).SetEase(easeIn)
            .OnComplete(() => {
                gruposPanel[targetIdx].blocksRaycasts = true;
            }));
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
}
