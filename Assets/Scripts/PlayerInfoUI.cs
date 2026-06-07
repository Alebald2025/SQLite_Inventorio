using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;
using DG.Tweening;

public class MainMenuUI : MonoBehaviour
{
    [Header("Botón icono de usuario")]
    [SerializeField] private Button botonUsuario;

    [Header("Paneles del juego")]
    [SerializeField] private CanvasGroup panelInventario;
    [SerializeField] private CanvasGroup panelObjetos;
    [SerializeField] private CanvasGroup panelPerfil;

    [Header("Textos del perfil")]
    [SerializeField] private TextMeshProUGUI usernameText;
    [SerializeField] private TextMeshProUGUI idText;

    [Header("Botón cerrar sesión")]
    [SerializeField] private Button botonCerrarSesion;

    [Header("Animación")]
    [SerializeField] private float fadeOutDuration = 0.3f;
    [SerializeField] private float fadeInDuration  = 0.4f;

    private bool perfilVisible = false;

    void Start()
    {
        string username = PlayerPrefs.GetString("CurrentUsername", "Desconocido");
        int userId      = PlayerPrefs.GetInt("CurrentUserID", -1);

        if (usernameText != null)
            usernameText.text = "Username: " + username;

        if (idText != null)
            idText.text = "ID: " + (userId != -1 ? userId.ToString() : "No disponible");

        // Estado inicial: perfil oculto, juego visible
        SetCanvasGroup(panelPerfil,    visible: false, instant: true);
        SetCanvasGroup(panelInventario, visible: true,  instant: true);
        SetCanvasGroup(panelObjetos,    visible: true,  instant: true);

        if (botonUsuario != null)
            botonUsuario.onClick.AddListener(OnUsuarioClick);

        if (botonCerrarSesion != null)
            botonCerrarSesion.onClick.AddListener(CerrarSesion);
    }

    private void OnUsuarioClick()
    {
        if (perfilVisible)
            MostrarJuego();
        else
            MostrarPerfil();
    }

    private void MostrarPerfil()
    {
        perfilVisible = true;
        botonUsuario.interactable = false;

        // Fade out de inventario y objetos en paralelo
        FadeOut(panelInventario, fadeOutDuration);
        FadeOut(panelObjetos,    fadeOutDuration, onComplete: () =>
        {
            // Cuando termina el último fade out, aparece el perfil
            FadeIn(panelPerfil, fadeInDuration, onComplete: () =>
                botonUsuario.interactable = true);
        });
    }

    private void MostrarJuego()
    {
        perfilVisible = false;
        botonUsuario.interactable = false;

        FadeOut(panelPerfil, fadeOutDuration, onComplete: () =>
        {
            FadeIn(panelInventario, fadeInDuration);
            FadeIn(panelObjetos,    fadeInDuration, onComplete: () =>
                botonUsuario.interactable = true);
        });
    }

    private void CerrarSesion()
    {
        PlayerPrefs.DeleteKey("CurrentUsername");
        PlayerPrefs.DeleteKey("CurrentUserID");
        PlayerPrefs.Save();
        SceneManager.LoadScene("MainMenu");
    }

    public void Salir()
    {
        // Vuelve al MainMenu manteniendo la sesión activa
        SceneManager.LoadScene("MainMenu");
    }

    // --- Helpers ---

    private void FadeOut(CanvasGroup cg, float duration, System.Action onComplete = null)
    {
        if (cg == null) { onComplete?.Invoke(); return; }
        cg.blocksRaycasts = false;
        cg.DOKill(true);
        cg.DOFade(0f, duration).OnComplete(() =>
        {
            cg.gameObject.SetActive(false);
            onComplete?.Invoke();
        });
    }

    private void FadeIn(CanvasGroup cg, float duration, System.Action onComplete = null)
    {
        if (cg == null) { onComplete?.Invoke(); return; }
        cg.gameObject.SetActive(true);
        cg.alpha = 0f;
        cg.DOKill(true);
        cg.DOFade(1f, duration).OnComplete(() =>
        {
            cg.blocksRaycasts = true;
            onComplete?.Invoke();
        });
    }

    private void SetCanvasGroup(CanvasGroup cg, bool visible, bool instant)
    {
        if (cg == null) return;
        cg.gameObject.SetActive(visible);
        cg.alpha          = visible ? 1f : 0f;
        cg.blocksRaycasts = visible;
    }
}