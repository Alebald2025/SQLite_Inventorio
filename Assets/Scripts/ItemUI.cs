using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;

public class InventoryUI : MonoBehaviour
{
    [Header("Referencias UI")]
    [SerializeField] private Button botonCofre;            
    [SerializeField] private RectTransform inventoryPanel;

    [Header("Textos de cantidades (dentro del panel)")]
    [SerializeField] private TextMeshProUGUI cantidadEspada;
    [SerializeField] private TextMeshProUGUI cantidadComida;
    [SerializeField] private TextMeshProUGUI cantidadLingote;
    [SerializeField] private TextMeshProUGUI cantidadEnderPearl;

    [Header("Configuración DOTween")]
    [SerializeField] private float duracionApertura = 0.45f;
    [SerializeField] private Ease easeApertura = Ease.OutBack;
    [SerializeField] private float duracionCierre = 0.3f;
    [SerializeField] private Ease easeCierre = Ease.InBack;

    private DatabaseManager db;
    private int userId;
    private bool panelAbierto = false;

    void Start()
    {
        db = FindObjectOfType<DatabaseManager>();
        if (db == null)
        {
            Debug.LogError("No se encontró DatabaseManager en la escena");
            return;
        }

        userId = PlayerPrefs.GetInt("CurrentUserID", -1);
        if (userId == -1)
        {
            Debug.LogError("No hay usuario logueado (CurrentUserID no encontrado)");
            return;
        }

        // Configuración inicial del panel
        if (inventoryPanel != null)
        {
            inventoryPanel.gameObject.SetActive(false);
            inventoryPanel.anchoredPosition = new Vector2(inventoryPanel.anchoredPosition.x, 360f);
        }

        if (botonCofre != null)
            botonCofre.onClick.AddListener(TogglePanel);

        ActualizarCantidades();
    }

    void Update()
    {
        // Cerrar con ESC si está abierto
        if (panelAbierto && Input.GetKeyDown(KeyCode.Escape))
        {
            CerrarPanel();
        }
    }

    private void TogglePanel()
    {
        if (panelAbierto)
            CerrarPanel();
        else
            AbrirPanel();
    }

    private void AbrirPanel()
    {
        if (inventoryPanel == null) return;

        panelAbierto = true;
        inventoryPanel.gameObject.SetActive(true);

        inventoryPanel.localScale = Vector3.one * 0.1f;
        inventoryPanel.DOScale(1f, duracionApertura)
            .SetEase(Ease.OutBack);

        var canvasGroup = inventoryPanel.GetComponent<CanvasGroup>();
        if (canvasGroup == null) canvasGroup = inventoryPanel.gameObject.AddComponent<CanvasGroup>();
        canvasGroup.alpha = 0f;
        canvasGroup.DOFade(1f, duracionApertura * 0.6f);

        ActualizarCantidades();
    }

    private void CerrarPanel()
    {
        if (inventoryPanel == null) return;

        panelAbierto = false;

        inventoryPanel.DOScale(0.1f, duracionCierre)
            .SetEase(Ease.InBack);

        var canvasGroup = inventoryPanel.GetComponent<CanvasGroup>();
        if (canvasGroup != null)
            canvasGroup.DOFade(0f, duracionCierre * 0.6f)
                .OnComplete(() => inventoryPanel.gameObject.SetActive(false));
    }

    public void AñadirEspada() { db.AddItem(userId, 1); ActualizarCantidades(); }
    public void DisminuirEspada() { db.RestarItem(userId, 1); ActualizarCantidades(); }

    public void AñadirComida() { db.AddItem(userId, 2); ActualizarCantidades(); }
    public void DisminuirComida() { db.RestarItem(userId, 2); ActualizarCantidades(); }

    public void AñadirLingote() { db.AddItem(userId, 3); ActualizarCantidades(); }
    public void DisminuirLingote() { db.RestarItem(userId, 3); ActualizarCantidades(); }

    public void AñadirEnderPearl() { db.AddItem(userId, 4); ActualizarCantidades(); }
    public void DisminuirEnderPearl() { db.RestarItem(userId, 4); ActualizarCantidades(); }

    private void ActualizarCantidades()
    {
        if (cantidadEspada) cantidadEspada.text = db.GetCantidad(userId, 1).ToString();
        if (cantidadComida) cantidadComida.text = db.GetCantidad(userId, 2).ToString();
        if (cantidadLingote) cantidadLingote.text = db.GetCantidad(userId, 3).ToString();
        if (cantidadEnderPearl) cantidadEnderPearl.text = db.GetCantidad(userId, 4).ToString();
    }
}