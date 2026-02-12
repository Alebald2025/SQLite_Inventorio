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

                    cmd.CommandText = @"
                        CREATE TABLE IF NOT EXISTS Usuaris (
                            UserID      INTEGER PRIMARY KEY AUTOINCREMENT,
                            Username    TEXT    UNIQUE NOT NULL,
                            Password    TEXT    NOT NULL
                        )";
                    cmd.ExecuteNonQuery();

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

                    // ──────────────────────────────────────────────
                    // Datos de prueba (opcional – solo se ejecuta una vez)
                    // Puedes comentarlo después de la primera ejecución
                    // ──────────────────────────────────────────────
                    cmd.CommandText = @"
                        INSERT OR IGNORE INTO Item (Name, Description, MaxStack) VALUES
                        ('Espada de Hierro',   'Arma básica de guerrero', 1),
                        ('Poción de Vida',     'Restaura 50 HP',          20),
                        ('Escudo de Madera',   'Defensa básica',          1),
                        ('Cristal Mágico',     'Material de crafting',    99),
                        ('Anillo de Fuerza',   'Aumenta fuerza +5',       1);
                    ";
                    cmd.ExecuteNonQuery();
                }
            }
            Debug.Log("Base de datos inicializada (usuarios + inventario)");
        }
        catch (Exception e)
        {
            Debug.LogError("Error al inicializar la base de datos: " + e.Message);
        }
    }

    public string RegisterUser(string username, string password)
    {
        if (string.IsNullOrWhiteSpace(username))
            return "El nom d'usuari no pot estar buit";

        if (password.Length < 8)
            return "La contrasenya ha de tenir mínim 8 caràcters";

        try
        {
            using (var conn = new SqliteConnection(dbPath))
            {
                conn.Open();
                using (var cmd = conn.CreateCommand())
                {
                    // Comprobar si existe
                    cmd.CommandText = "SELECT COUNT(*) FROM Usuaris WHERE Username = @user";
                    cmd.Parameters.AddWithValue("@user", username);
                    long count = (long)cmd.ExecuteScalar();

                    if (count > 0)
                        return "Aquest usuari ja existeix";

                    // Registrarse
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
                return "Aquest usuari ja existeix";
            return "Error de base de dades: " + ex.Message;
        }
        catch (Exception ex)
        {
            return "Error inesperat: " + ex.Message;
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
                        return (true, "Login correcte", userId);
                    }
                    else
                    {
                        return (false, "Usuari o contrasenya incorrectes", -1);
                    }
                }
            }
        }
        catch (Exception ex)
        {
            return (false, "Error de connexió: " + ex.Message, -1);
        }
    }

    // Obtener todo el inventario de un usuario
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

    // Añadir / incrementar cantidad de un ítem
    public bool AddItem(int userId, int itemId, int cantidad = 1)
    {
        if (cantidad <= 0) return false;

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

    // Cambiar cantidad exacta (puede usarse para restar)
    public bool SetCantidad(int userId, int itemId, int nuevaCantidad)
    {
        if (nuevaCantidad <= 0)
            return RemoveItem(userId, itemId);

        using (var conn = new SqliteConnection(dbPath))
        {
            conn.Open();
            using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText = @"
                    UPDATE Inventario
                    SET Cantidad = @cant
                    WHERE userId = @uid AND itemId = @iid;";

                cmd.Parameters.AddWithValue("@cant", nuevaCantidad);
                cmd.Parameters.AddWithValue("@uid", userId);
                cmd.Parameters.AddWithValue("@iid", itemId);

                return cmd.ExecuteNonQuery() > 0;
            }
        }
    }

    // Eliminar un ítem del inventario
    public bool RemoveItem(int userId, int itemId)
    {
        using (var conn = new SqliteConnection(dbPath))
        {
            conn.Open();
            using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText = "DELETE FROM Inventario WHERE userId = @uid AND itemId = @iid;";
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