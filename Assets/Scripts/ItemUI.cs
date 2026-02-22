using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;

public class InventoryUI : MonoBehaviour
{
    [Header("Referencias UI")]
    [SerializeField] private Button botonCofre;                 // InventoyIcon (el cofre que se pulsa)
    [SerializeField] private RectTransform inventoryPanel;      // InventoryPanel (el que tiene el grid)

    [Header("Textos de cantidades (dentro del panel)")]
    [SerializeField] private TextMeshProUGUI cantidadEspada;
    [SerializeField] private TextMeshProUGUI cantidadComida;
    [SerializeField] private TextMeshProUGUI cantidadLingote;
    [SerializeField] private TextMeshProUGUI cantidadEnderPearl;

    [Header("Configuración DOTween")]
    [SerializeField] private float duracionApertura = 0.45f;
    [SerializeField] private Ease easeApertura = Ease.OutBack;   // Rebote al abrir
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
            // Empieza oculto abajo (ajusta según tu posición)
            inventoryPanel.anchoredPosition = new Vector2(inventoryPanel.anchoredPosition.x, -800f);
        }

        // Conectar el cofre para toggle
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

        // Animación: subir desde abajo con rebote
        inventoryPanel.anchoredPosition = new Vector2(inventoryPanel.anchoredPosition.x, -800f); // ajusta valor si es necesario
        inventoryPanel.DOAnchorPosY(0, duracionApertura)
            .SetEase(easeApertura);

        ActualizarCantidades();
    }

    private void CerrarPanel()
    {
        if (inventoryPanel == null) return;

        panelAbierto = false;

        // Animación: bajar hacia abajo
        inventoryPanel.DOAnchorPosY(-800f, duracionCierre)
            .SetEase(easeCierre)
            .OnComplete(() => inventoryPanel.gameObject.SetActive(false));
    }

    // Métodos para + y - (conecta tus botones dentro del panel a estos)
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