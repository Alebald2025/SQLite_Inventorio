using System;
using UnityEngine;
using Mono.Data.Sqlite;
using System.Data;
using System.Collections.Generic;

public class DatabaseManager : MonoBehaviour
{
    private string dbPath;

    private void Awake()
    {
        dbPath = "URI=file:" + Application.persistentDataPath + "/usuaris.db";
        InitializeDatabase();
    }

    private void InitializeDatabase()
    {
        try
        {
            using (var conn = new SqliteConnection(dbPath))
            {
                conn.Open();
                using (var cmd = conn.CreateCommand())
                {
                    // Tabla Usuaris (sin cambios)
                    cmd.CommandText = @"
                        CREATE TABLE IF NOT EXISTS Usuaris (
                            UserID      INTEGER PRIMARY KEY AUTOINCREMENT,
                            Username    TEXT    UNIQUE NOT NULL,
                            Password    TEXT    NOT NULL
                        )";
                    cmd.ExecuteNonQuery();

                    // Tablas Inventario (sin cambios)
                    cmd.CommandText = @"
                        CREATE TABLE IF NOT EXISTS Item (
                            ID          INTEGER PRIMARY KEY AUTOINCREMENT,
                            Name        TEXT    NOT NULL,
                            Description TEXT,
                            MaxStack    INTEGER NOT NULL DEFAULT 99
                        );

                        CREATE TABLE IF NOT EXISTS Inventario (
                            InventarioID INTEGER PRIMARY KEY AUTOINCREMENT,
                            userId       INTEGER NOT NULL,
                            itemId       INTEGER NOT NULL,
                            Cantidad     INTEGER NOT NULL DEFAULT 1,
                            FOREIGN KEY (userId)  REFERENCES Usuaris(UserID)  ON DELETE CASCADE,
                            FOREIGN KEY (itemId)  REFERENCES Item(ID)         ON DELETE RESTRICT,
                            UNIQUE(userId, itemId)
                        );";
                    cmd.ExecuteNonQuery();

                    // Datos de prueba ajustados a tus 4 items
                    cmd.CommandText = @"
                        INSERT OR IGNORE INTO Item (Name, Description, MaxStack) VALUES
                        ('Espada', 'Arma para combate', 1),
                        ('Meat', 'Comida para restaurar salud', 15),
                        ('Cesped', 'Bloque de hierba para construcción', 20),
                        ('Ender', 'Ojo para portales', 6);
                    ";
                    cmd.ExecuteNonQuery();
                }
            }
            Debug.Log("DB inicializada");
        }
        catch (Exception e)
        {
            Debug.LogError("Error DB: " + e.Message);
        }
    }

    // Login/Register sin cambios (mantén los tuyos)

    public string RegisterUser(string username, string password)
    {
        if (string.IsNullOrWhiteSpace(username))
            return "El nombre de usuario no puede estar vacío";

        if (password.Length < 8)
            return "La contraseña debe tener al menos 8 caracteres";

        try
        {
            using (var conn = new SqliteConnection(dbPath))
            {
                conn.Open();
                using (var cmd = conn.CreateCommand())
                {
                    cmd.CommandText = "SELECT COUNT(*) FROM Usuaris WHERE Username = @user";
                    cmd.Parameters.AddWithValue("@user", username);
                    long count = (long)cmd.ExecuteScalar();

                    if (count > 0)
                        return "Este usuario ya existe";

                    cmd.CommandText = "INSERT INTO Usuaris (Username, Password) VALUES (@user, @pass)";
                    cmd.Parameters.AddWithValue("@user", username);
                    cmd.Parameters.AddWithValue("@pass", password);
                    cmd.ExecuteNonQuery();

                    return "OK";
                }
            }
        }
        catch (SqliteException ex)
        {
            if (ex.Message.Contains("UNIQUE constraint failed"))
                return "Este usuario ya existe";
            return "Error de base de datos: " + ex.Message;
        }
        catch (Exception ex)
        {
            return "Error inesperado: " + ex.Message;
        }
    }

    public (bool success, string message, int userId) LoginUser(string username, string password)
    {
        try
        {
            using (var conn = new SqliteConnection(dbPath))
            {
                conn.Open();
                using (var cmd = conn.CreateCommand())
                {
                    cmd.CommandText = "SELECT UserID FROM Usuaris WHERE Username = @user AND Password = @pass";
                    cmd.Parameters.AddWithValue("@user", username);
                    cmd.Parameters.AddWithValue("@pass", password);

                    var result = cmd.ExecuteScalar();

                    if (result != null)
                    {
                        int userId = Convert.ToInt32(result);
                        return (true, "Login correcto", userId);
                    }
                    else
                    {
                        return (false, "Usuario o contraseña incorrectos", -1);
                    }
                }
            }
        }
        catch (Exception ex)
        {
            return (false, "Error de conexión: " + ex.Message, -1);
        }
    }

    // Métodos Inventario (actualizados)
    public List<InventarioEntry> GetInventario(int userId)
    {
        var lista = new List<InventarioEntry>();

        using (var conn = new SqliteConnection(dbPath))
        {
            conn.Open();
            using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText = @"
                    SELECT inv.itemId, i.Name, i.Description, i.MaxStack, inv.Cantidad
                    FROM Inventario inv
                    JOIN Item i ON inv.itemId = i.ID
                    WHERE inv.userId = @userId
                    ORDER BY i.Name";

                cmd.Parameters.AddWithValue("@userId", userId);

                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        lista.Add(new InventarioEntry
                        {
                            ItemId = reader.GetInt32(0),
                            Name = reader.GetString(1),
                            Description = reader.IsDBNull(2) ? "" : reader.GetString(2),
                            MaxStack = reader.GetInt32(3),
                            Cantidad = reader.GetInt32(4)
                        });
                    }
                }
            }
        }
        return lista;
    }

    // Añadir/incrementar (respeta MaxStack)
    public bool AddItem(int userId, int itemId, int cantidad = 1)
    {
        int actual = GetCantidad(userId, itemId);
        int max = GetMaxStack(itemId);

        if (actual + cantidad > max) return false; // no añadir si supera max

        using (var conn = new SqliteConnection(dbPath))
        {
            conn.Open();
            using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText = @"
                    INSERT INTO Inventario (userId, itemId, Cantidad)
                    VALUES (@uid, @iid, @cant)
                    ON CONFLICT(userId, itemId)
                    DO UPDATE SET Cantidad = Cantidad + @cant;";

                cmd.Parameters.AddWithValue("@uid", userId);
                cmd.Parameters.AddWithValue("@iid", itemId);
                cmd.Parameters.AddWithValue("@cant", cantidad);

                return cmd.ExecuteNonQuery() > 0;
            }
        }
    }

    // Restar/decrementar (si 0, elimina)
    public bool RestarItem(int userId, int itemId, int cantidad = 1)
    {
        int actual = GetCantidad(userId, itemId);
        int nueva = actual - cantidad;

        if (nueva <= 0)
            return RemoveItem(userId, itemId);
        else
            return SetCantidad(userId, itemId, nueva);
    }

    // Helpers privados (añadidos)
    public int GetCantidad(int userId, int itemId)
    {
        using (var conn = new SqliteConnection(dbPath))
        {
            conn.Open();
            using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText = "SELECT Cantidad FROM Inventario WHERE userId = @uid AND itemId = @iid";
                cmd.Parameters.AddWithValue("@uid", userId);
                cmd.Parameters.AddWithValue("@iid", itemId);
                var result = cmd.ExecuteScalar();
                return result != null ? Convert.ToInt32(result) : 0;
            }
        }
    }

    public int GetMaxStack(int itemId)
    {
        using (var conn = new SqliteConnection(dbPath))
        {
            conn.Open();
            using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText = "SELECT MaxStack FROM Item WHERE ID = @iid";
                cmd.Parameters.AddWithValue("@iid", itemId);
                var result = cmd.ExecuteScalar();
                return result != null ? Convert.ToInt32(result) : 99;
            }
        }
    }

    // Set exacto
    public bool SetCantidad(int userId, int itemId, int nuevaCantidad)
    {
        using (var conn = new SqliteConnection(dbPath))
        {
            conn.Open();
            using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText = "UPDATE Inventario SET Cantidad = @cant WHERE userId = @uid AND itemId = @iid";
                cmd.Parameters.AddWithValue("@cant", nuevaCantidad);
                cmd.Parameters.AddWithValue("@uid", userId);
                cmd.Parameters.AddWithValue("@iid", itemId);
                return cmd.ExecuteNonQuery() > 0;
            }
        }
    }

    // Remove
    public bool RemoveItem(int userId, int itemId)
    {
        using (var conn = new SqliteConnection(dbPath))
        {
            conn.Open();
            using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText = "DELETE FROM Inventario WHERE userId = @uid AND itemId = @iid";
                cmd.Parameters.AddWithValue("@uid", userId);
                cmd.Parameters.AddWithValue("@iid", itemId);
                return cmd.ExecuteNonQuery() > 0;
            }
        }
    }
}

public class InventarioEntry
{
    public int ItemId;
    public string Name;
    public string Description;
    public int MaxStack;
    public int Cantidad;
}