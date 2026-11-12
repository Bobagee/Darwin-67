using UnityEngine;

public abstract class Iteraccion : MonoBehaviour
{
    [Header("Configuración de Interacción")]
    [Tooltip("Texto para la interfaz visual")]
    [SerializeField] protected string mensajePrompt = "Presionar E para interactuar";

    public string MensajePrompt => mensajePrompt;
    public abstract void Interactuar();
}