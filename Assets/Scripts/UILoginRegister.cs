using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using System.Collections;

public class UILoginRegister : MonoBehaviour
{
    [Header("--- Panel Login ---")]
    [SerializeField] private TMP_InputField loginUsernameField;
    [SerializeField] private TMP_InputField loginPasswordField;
    [SerializeField] private Button loginButton;

    [Header("--- Panel Register ---")]
    [SerializeField] private TMP_InputField registerUsernameField;
    [SerializeField] private TMP_InputField registerPasswordField;
    [SerializeField] private Button registerButton;

    [Header("Mensaje único (puede estar fuera de los paneles)")]
    [SerializeField] private TextMeshProUGUI feedbackText;
    [SerializeField] private float mensajeDuracion = 4f;
    [SerializeField] private Color colorExito = Color.green;
    [SerializeField] private Color colorError = new Color(1f, 0.3f, 0.3f); 
    [SerializeField] private Color colorInfo = new Color(1f, 0.9f, 0.4f);

    private DatabaseManager dbManager;
    private Coroutine hideMessageCoroutine;

    private void Awake()
    {
        dbManager = FindObjectOfType<DatabaseManager>();
    }

    private void Start()
    {
        if (loginButton != null)
            loginButton.onClick.AddListener(OnLogin);

        if (registerButton != null)
            registerButton.onClick.AddListener(OnRegister);

        HideFeedback();
    }

    private void OnLogin()
    {
        string user = GetTrimmedText(loginUsernameField);
        string pass = loginPasswordField.text;

        if (string.IsNullOrEmpty(user) || string.IsNullOrEmpty(pass))
        {
            ShowFeedback("Completa usuario y contraseña", colorInfo);
            return;
        }

        var (success, message, userId) = dbManager.LoginUser(user, pass);

        if (success)
        {
            ShowFeedback("Has iniciado sesión correctamente", colorExito);

            PlayerPrefs.SetString("CurrentUsername", user);
            PlayerPrefs.SetInt("CurrentUserID", userId);
            PlayerPrefs.Save();
            Debug.Log("UserID guardado: " + userId);

            Invoke(nameof(CargarEscenaPrincipal), 1.1f);
        }
        else
        {
            ShowFeedback(message, colorError);
        }
    }

    private void CargarEscenaPrincipal()
    {
        SceneManager.LoadScene("Inventario");
    }

    private void OnRegister()
    {
        string user = GetTrimmedText(registerUsernameField);
        string pass = registerPasswordField.text;

        if (string.IsNullOrEmpty(user) || string.IsNullOrEmpty(pass))
        {
            ShowFeedback("Completa usuario y contraseña", colorInfo);
            return;
        }

        string result = dbManager.RegisterUser(user, pass);

        if (result == "OK")
        {
            ShowFeedback("Cuenta creada correctamente • Inicia sesión", colorExito);

            if (registerUsernameField) registerUsernameField.text = "";
            if (registerPasswordField) registerPasswordField.text = "";
        }
        else
        {
            ShowFeedback(result, colorError);
        }
    }

    private string GetTrimmedText(TMP_InputField field)
    {
        return field != null ? field.text.Trim() : "";
    }

    private void ShowFeedback(string mensaje, Color color)
    {
        if (feedbackText == null)
        {
            Debug.Log(mensaje);
            return;
        }

        if (hideMessageCoroutine != null)
            StopCoroutine(hideMessageCoroutine);

        feedbackText.text = mensaje;
        feedbackText.color = color;
        feedbackText.gameObject.SetActive(true);

        hideMessageCoroutine = StartCoroutine(HideAfterDelay());
    }

    private IEnumerator HideAfterDelay()
    {
        yield return new WaitForSeconds(mensajeDuracion);
        HideFeedback();
    }

    private void HideFeedback()
    {
        if (feedbackText != null)
        {
            feedbackText.text = "";
            feedbackText.gameObject.SetActive(false);
        }
    }
}