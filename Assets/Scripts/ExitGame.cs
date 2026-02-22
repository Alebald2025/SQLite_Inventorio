using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExitGame : MonoBehaviour
{
    public void SalirDelJuego()
    {
        Debug.Log("Cerrando la aplicación...");
        Application.Quit();

    }

}
