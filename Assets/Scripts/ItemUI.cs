using UnityEngine;
using TMPro;
using DG.Tweening;

public class InventoryUI : MonoBehaviour
{
    [Header("Textos de cantidades")]
    [SerializeField] private TextMeshProUGUI cantidadEspada;
    [SerializeField] private TextMeshProUGUI cantidadComida;
    [SerializeField] private TextMeshProUGUI cantidadLingote;
    [SerializeField] private TextMeshProUGUI cantidadEnderPearl;

    [Header("Slots del inventario (para animación)")]
    [SerializeField] private RectTransform slotEspada;
    [SerializeField] private RectTransform slotComida;
    [SerializeField] private RectTransform slotLingote;
    [SerializeField] private RectTransform slotEnderPearl;

    [Header("Animación al añadir")]
    [SerializeField] private float punchScale = 0.3f;
    [SerializeField] private float punchDuration = 0.35f;

    private DatabaseManager db;
    private int userId;

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

        ActualizarCantidades();
    }

    public void AñadirEspada()    { if (db.AddItem(userId, 1)) { ActualizarCantidades(); AnimarSlot(slotEspada); } }
    public void DisminuirEspada() { db.RestarItem(userId, 1); ActualizarCantidades(); }

    public void AñadirComida()    { if (db.AddItem(userId, 2)) { ActualizarCantidades(); AnimarSlot(slotComida); } }
    public void DisminuirComida() { db.RestarItem(userId, 2); ActualizarCantidades(); }

    public void AñadirLingote()    { if (db.AddItem(userId, 3)) { ActualizarCantidades(); AnimarSlot(slotLingote); } }
    public void DisminuirLingote() { db.RestarItem(userId, 3); ActualizarCantidades(); }

    public void AñadirEnderPearl()    { if (db.AddItem(userId, 4)) { ActualizarCantidades(); AnimarSlot(slotEnderPearl); } }
    public void DisminuirEnderPearl() { db.RestarItem(userId, 4); ActualizarCantidades(); }

    private void AnimarSlot(RectTransform slot)
    {
        if (slot == null) return;
        slot.DOKill(true);
        slot.DOPunchScale(Vector3.one * punchScale, punchDuration, vibrato: 1, elasticity: 0.5f);
    }

    private void ActualizarCantidades()
    {
        if (cantidadEspada) cantidadEspada.text = db.GetCantidad(userId, 1).ToString();
        if (cantidadComida) cantidadComida.text = db.GetCantidad(userId, 2).ToString();
        if (cantidadLingote) cantidadLingote.text = db.GetCantidad(userId, 3).ToString();
        if (cantidadEnderPearl) cantidadEnderPearl.text = db.GetCantidad(userId, 4).ToString();
    }
}