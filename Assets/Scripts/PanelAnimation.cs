using UnityEngine;

public class PanelAnimation : MonoBehaviour
{
    [SerializeField] private RectTransform panelRect;

    public void Expandir()
    {
        if (panelRect != null)
            panelRect.sizeDelta = new Vector2(270f, 350f);
    }

    public void Contraer()
    {
        if (panelRect != null)
            panelRect.sizeDelta = new Vector2(270f, 260f);
    }
}
