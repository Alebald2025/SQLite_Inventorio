using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class MainMenuUI : MonoBehaviour
{
    [Header("Referencias UI")]
    [SerializeField] private TextMeshProUGUI usernameText;
    [SerializeField] private TextMeshProUGUI idText;
    [SerializeField] private Button botonCerrarSesion;

    void Start()
    {
        // Cargar datos del usuario desde PlayerPrefs
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
        SceneManager.LoadScene("Login_Register");
    }
}