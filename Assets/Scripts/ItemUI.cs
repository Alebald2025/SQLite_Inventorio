using UnityEngine;
using TMPro;

public class ItemsUI : MonoBehaviour
{
    [Header("Textos de cantidades")]
    [SerializeField] private TextMeshProUGUI cantidadItem1;
    [SerializeField] private TextMeshProUGUI cantidadItem2;
    [SerializeField] private TextMeshProUGUI cantidadItem3;
    [SerializeField] private TextMeshProUGUI cantidadItem4;

    private DatabaseManager db;
    private int userId = -1;

    void Start()
    {
        // Buscar DB
        db = FindObjectOfType<DatabaseManager>();
        if (db == null)
        {
            Debug.LogError("DatabaseManager no encontrado en la escena. Añade un GameObject con este script.");
            return;
        }

        // Cargar UserID
        userId = PlayerPrefs.GetInt("CurrentUserID", -1);
        if (userId == -1)
        {
            Debug.LogError("No hay usuario logueado (CurrentUserID no existe en PlayerPrefs).");
            return;
        }

        Debug.Log($"Inventario cargado para usuario ID: {userId}");

        ActualizarCantidades();
    }

    // Añadir (botones izquierda)
    public void AñadirItem1() { Añadir(1); }
    public void AñadirItem2() { Añadir(2); }
    public void AñadirItem3() { Añadir(3); }
    public void AñadirItem4() { Añadir(4); }

    // Disminuir (botones derecha)
    public void DisminuirItem1() { Disminuir(1); }
    public void DisminuirItem2() { Disminuir(2); }
    public void DisminuirItem3() { Disminuir(3); }
    public void DisminuirItem4() { Disminuir(4); }

    private void Añadir(int itemId)
    {
        if (db == null || userId == -1) return;

        bool añadido = db.AddItem(userId, itemId);
        if (añadido)
        {
            ActualizarCantidades();
        }
        else
        {
            Debug.Log($"No se pudo añadir más del item {itemId} (límite alcanzado)");
        }
    }

    private void Disminuir(int itemId)
    {
        if (db == null || userId == -1) return;

        bool restado = db.RestarItem(userId, itemId);
        if (restado)
        {
            ActualizarCantidades();
        }
        else
        {
            Debug.Log($"No se pudo restar del item {itemId} (ya en 0)");
        }
    }

    private void ActualizarCantidades()
    {
        if (cantidadItem1) cantidadItem1.text = db.GetCantidad(userId, 1).ToString();
        if (cantidadItem2) cantidadItem2.text = db.GetCantidad(userId, 2).ToString();
        if (cantidadItem3) cantidadItem3.text = db.GetCantidad(userId, 3).ToString();
        if (cantidadItem4) cantidadItem4.text = db.GetCantidad(userId, 4).ToString();
    }
}