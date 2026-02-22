using UnityEngine;
using TMPro;

public class ItemsUI : MonoBehaviour
{
    [Header("Textos de cantidades (derecha)")]
    [SerializeField] private TextMeshProUGUI item1Num;
    [SerializeField] private TextMeshProUGUI item2Num;
    [SerializeField] private TextMeshProUGUI item3Num;
    [SerializeField] private TextMeshProUGUI item4Num;

    private DatabaseManager db;
    private int userId;

    void Start()
    {
        db = FindObjectOfType<DatabaseManager>();
        userId = PlayerPrefs.GetInt("CurrentUserID", -1);
        if (userId == -1)
        {
            Debug.LogError("No usuario logueado");
            return;
        }

        ActualizarUI();
    }

    // Botones para añadir (+1, izquierda)
    public void AñadirItem1() { db.AddItem(userId, 1); ActualizarUI(); }
    public void AñadirItem2() { db.AddItem(userId, 2); ActualizarUI(); }
    public void AñadirItem3() { db.AddItem(userId, 3); ActualizarUI(); }
    public void AñadirItem4() { db.AddItem(userId, 4); ActualizarUI(); }

    // Botones para restar (-1, derecha)
    public void RestarItem1() { db.RestarItem(userId, 1); ActualizarUI(); }
    public void RestarItem2() { db.RestarItem(userId, 2); ActualizarUI(); }
    public void RestarItem3() { db.RestarItem(userId, 3); ActualizarUI(); }
    public void RestarItem4() { db.RestarItem(userId, 4); ActualizarUI(); }

    private void ActualizarUI()
    {
        item1Num.text = db.GetCantidad(userId, 1).ToString();
        item2Num.text = db.GetCantidad(userId, 2).ToString();
        item3Num.text = db.GetCantidad(userId, 3).ToString();
        item4Num.text = db.GetCantidad(userId, 4).ToString();
    }
}