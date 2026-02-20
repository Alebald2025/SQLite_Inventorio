using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IventoryManager : MonoBehaviour
{
    private DatabaseManager db;
    private int userId;

    void Awake()
    {
        db = FindObjectOfType<DatabaseManager>();
        userId = PlayerPrefs.GetInt("CurrentUserID", -1);
        if (userId == -1)
        {
            Debug.LogError("No usuario logueado");
        }
    }

    // Método genérico para añadir 1 (si posible, respeta MaxStack)
    public bool AñadirItem(int itemId)
    {
        return db.AddItem(userId, itemId, 1);
    }

    // Método genérico para disminuir 1 (no baja de 0)
    public bool DisminuirItem(int itemId)
    {
        return db.RestarItem(userId, itemId, 1);
    }

    // Métodos específicos si los necesitas para botones
    public void AñadirEspada() { AñadirItem(1); }  // ID de espada
    public void DisminuirEspada() { DisminuirItem(1); }

    // Similar para otros items (meat ID=2, cesped=3, ender=4)

    // Opcional: obtener cantidad actual (para mostrar en texto)
    public int GetCantidadItem(int itemId)
    {
        return db.GetCantidad(userId, itemId);
    }

    // Opcional: mostrar descripción (para hover o UI)
    public string GetDescripcionItem(int itemId)
    {
        using (var conn = new SqliteConnection(db.dbPath))
        {
            conn.Open();
            using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText = "SELECT Descripcion FROM Item WHERE ID = @id";
                cmd.Parameters.AddWithValue("@id", itemId);
                var result = cmd.ExecuteScalar();
                return result != null ? result.ToString() : "";
            }
        }
    }
}
