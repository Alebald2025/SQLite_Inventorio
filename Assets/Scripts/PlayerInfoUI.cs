using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class MainMenuUI : MonoBehaviour
{
    [Header("Referencias UI")]
    [SerializeField] private TextMeshProUGUI usernameText;    // TextMeshPro para "Username: ..."
    [SerializeField] private TextMeshProUGUI idText;          // TextMeshPro para "ID: ..."
    [SerializeField] private Button botonCerrarSesion;        // El botón "CERRAR SESIÓN"

    void Start()
    {
        // Cargar datos del usuario desde PlayerPrefs (guardados en el login)
        string username = PlayerPrefs.GetString("CurrentUsername", "Desconocido");
        int userId = PlayerPrefs.GetInt("CurrentUserID", -1);

        // Mostrar en pantalla
        if (usernameText != null)
            usernameText.text = "Username: " + username;

        if (idText != null)
            idText.text = "ID: " + (userId != -1 ? userId.ToString() : "No disponible");

        // Conectar botón de cerrar sesión
        if (botonCerrarSesion != null)
            botonCerrarSesion.onClick.AddListener(CerrarSesion);
    }

    private void CerrarSesion()
    {
        // Limpiar datos del usuario
        PlayerPrefs.DeleteKey("CurrentUsername");
        PlayerPrefs.DeleteKey("CurrentUserID");
        PlayerPrefs.Save();

        // Regresar a la escena de login/register
        SceneManager.LoadScene("Login_Register");  // ← Cambia "Login" por el nombre real de tu escena de login/register
    }
}