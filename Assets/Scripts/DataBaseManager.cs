using System;
using UnityEngine;
using Mono.Data.Sqlite;
using System.Data;

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
                    // Tabla Usuaris
                    cmd.CommandText = @"
                        CREATE TABLE IF NOT EXISTS Usuaris (
                            UserID      INTEGER PRIMARY KEY AUTOINCREMENT,
                            Username    TEXT    UNIQUE NOT NULL,
                            Password    TEXT    NOT NULL
                        )";
                    cmd.ExecuteNonQuery();

                    // Tabla Item (catálogo global)
                    cmd.CommandText = @"
                        CREATE TABLE IF NOT EXISTS Item (
                            ID          INTEGER PRIMARY KEY AUTOINCREMENT,
                            Nombre      TEXT    NOT NULL UNIQUE,
                            Descripcion TEXT,
                            MaxStack    INTEGER NOT NULL
                        )";
                    cmd.ExecuteNonQuery();

                    // Tabla Inventario
                    cmd.CommandText = @"
                        CREATE TABLE IF NOT EXISTS Inventario (
                            InventarioID    INTEGER PRIMARY KEY AUTOINCREMENT,
                            userId          INTEGER NOT NULL,
                            itemId          INTEGER NOT NULL,
                            Cantidad        INTEGER NOT NULL DEFAULT 0,
                            FOREIGN KEY (userId) REFERENCES Usuaris(UserID) ON DELETE CASCADE,
                            FOREIGN KEY (itemId) REFERENCES Item(ID) ON DELETE RESTRICT,
                            UNIQUE(userId, itemId)
                        )";
                    cmd.ExecuteNonQuery();

                    // Datos iniciales de ítems (no sobreescribe si ya existen)
                    cmd.CommandText = @"
                        INSERT OR IGNORE INTO Item (Nombre, Descripcion, MaxStack) VALUES
                        ('Espada',     'Arma de combate cuerpo a cuerpo', 1),
                        ('Comida',     'Restaura puntos de vida',         15),
                        ('Lingote',    'Material de crafteo básico',      20),
                        ('EnderPearl', 'Permite teletransportarse',       6)";
                    cmd.ExecuteNonQuery();
                }
            }
            Debug.Log("Base de datos inicializada en: " + dbPath);
        }
        catch (Exception e)
        {
            Debug.LogError("Error inicializando base de datos: " + e.Message);
        }
    }

    public string RegisterUser(string username, string password)
    {
        if (string.IsNullOrWhiteSpace(username))
            return "El nombre de usuario no puede estar vacio";

        if (password.Length < 8)
            return "La contraseña debe tener minimo 8 caracteres";

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
                        return "Este usuario ya existe";

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
                return "Este usuario ya existe";
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
            return (false, "Error de connexión: " + ex.Message, -1);
        }
    }

    // Devuelve la cantidad actual del ítem en el inventario del usuario (0 si no existe)
    public int GetCantidad(int userId, int itemId)
    {
        try
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
        catch (Exception ex)
        {
            Debug.LogError("GetCantidad error: " + ex.Message);
            return 0;
        }
    }

    // Añade 1 unidad del ítem respetando MaxStack. Devuelve false si ya está al máximo.
    public bool AddItem(int userId, int itemId)
    {
        try
        {
            using (var conn = new SqliteConnection(dbPath))
            {
                conn.Open();
                using (var cmd = conn.CreateCommand())
                {
                    // Obtener MaxStack del ítem
                    cmd.CommandText = "SELECT MaxStack FROM Item WHERE ID = @iid";
                    cmd.Parameters.AddWithValue("@iid", itemId);
                    var msResult = cmd.ExecuteScalar();
                    if (msResult == null) return false;
                    int maxStack = Convert.ToInt32(msResult);

                    // Cantidad actual
                    cmd.Parameters.Clear();
                    cmd.CommandText = "SELECT Cantidad FROM Inventario WHERE userId = @uid AND itemId = @iid";
                    cmd.Parameters.AddWithValue("@uid", userId);
                    cmd.Parameters.AddWithValue("@iid", itemId);
                    var cantResult = cmd.ExecuteScalar();
                    int cantidadActual = cantResult != null ? Convert.ToInt32(cantResult) : 0;

                    if (cantidadActual >= maxStack) return false;

                    cmd.Parameters.Clear();
                    if (cantResult == null)
                    {
                        // Insertar nuevo registro
                        cmd.CommandText = "INSERT INTO Inventario (userId, itemId, Cantidad) VALUES (@uid, @iid, 1)";
                    }
                    else
                    {
                        // Incrementar existente
                        cmd.CommandText = "UPDATE Inventario SET Cantidad = Cantidad + 1 WHERE userId = @uid AND itemId = @iid";
                    }
                    cmd.Parameters.AddWithValue("@uid", userId);
                    cmd.Parameters.AddWithValue("@iid", itemId);
                    cmd.ExecuteNonQuery();
                    return true;
                }
            }
        }
        catch (Exception ex)
        {
            Debug.LogError("AddItem error: " + ex.Message);
            return false;
        }
    }

    // Resta 1 unidad. Si llega a 0, elimina el registro. Devuelve false si no tiene ninguno.
    public bool RestarItem(int userId, int itemId)
    {
        try
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
                    if (result == null) return false;

                    int cantidad = Convert.ToInt32(result);

                    cmd.Parameters.Clear();
                    if (cantidad <= 1)
                    {
                        cmd.CommandText = "DELETE FROM Inventario WHERE userId = @uid AND itemId = @iid";
                    }
                    else
                    {
                        cmd.CommandText = "UPDATE Inventario SET Cantidad = Cantidad - 1 WHERE userId = @uid AND itemId = @iid";
                    }
                    cmd.Parameters.AddWithValue("@uid", userId);
                    cmd.Parameters.AddWithValue("@iid", itemId);
                    cmd.ExecuteNonQuery();
                    return true;
                }
            }
        }
        catch (Exception ex)
        {
            Debug.LogError("RestarItem error: " + ex.Message);
            return false;
        }
    }
}