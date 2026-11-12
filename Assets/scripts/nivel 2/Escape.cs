using System;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Escape : MonoBehaviour
{
    public static Escape Instance { get; private set; }

    [Header("Configuración de Escenas")]
    [SerializeField] private string nombreSiguienteEscena = "Nivel2";

    public static event Action OnTrapFailed;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    public static void NotificarTrampaFallida()
    {
        OnTrapFailed?.Invoke();
    }

    public void CompletarNivel()
    {
        if (!string.IsNullOrEmpty(nombreSiguienteEscena))
        {
            SceneManager.LoadScene(nombreSiguienteEscena);
        }
    }
}