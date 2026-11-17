using UnityEngine;
using UnityEngine.Events;

public class Ventila : Iteraccion
{
    [Header("Requisitos")]
    [SerializeField] private string idDesarmadorRequerido = "Desarmador";

    [Header("Configuración de Tornillos")]
    [SerializeField] private int cantidadTornillos = 4;
    [SerializeField] private int clicsPorTornillo = 3;
    [Tooltip("Opcional: Arrastra aquí los 4 modelos 3D de los tornillos para que desaparezcan uno a uno")]
    [SerializeField] private GameObject[] objetosTornillos;

    [Header("Animación y Eventos")]
    [SerializeField] private Animator animadorEscotilla;
    public UnityEvent onClicDesarmador;
    public UnityEvent onTornilloQuitado;
    public UnityEvent onVentilaAbierta;

[Header("UI de Interacción")]
[Tooltip("Texto en Canvas que dice: CLICKEA 3 VECES PARA DESARMAR")]
[SerializeField] private GameObject cartelDesarmarUI;

public override void OnMirar(bool mirando)
{
    if (cartelDesarmarUI == null) return;

    bool tieneDesarmador = InventarioUI.Instance != null && InventarioUI.Instance.TieneObjeto(idDesarmadorRequerido);
    cartelDesarmarUI.SetActive(mirando && tieneDesarmador && !abierta);
}

    private int tornillosQuitados = 0;
    private int clicsTornilloActual = 0;
    private bool abierta = false;

    public override void Interactuar()
    {
        if (abierta) return;

        // 1. Verificar que el jugador tenga el desarmador
        if (InventarioUI.Instance == null || !InventarioUI.Instance.TieneObjeto(idDesarmadorRequerido))
        {
            Debug.Log("Necesitas el Desarmador para quitar los tornillos.");
            return;
        }

        // 2. Sumar clic al tornillo actual
        clicsTornilloActual++;
        onClicDesarmador?.Invoke();
        Debug.Log($"Tornillo {tornillosQuitados + 1}/{cantidadTornillos} -> Clic {clicsTornilloActual}/{clicsPorTornillo}");

        // 3. Si completó los 3 clics, quitar ese tornillo
        if (clicsTornilloActual >= clicsPorTornillo)
        {
            clicsTornilloActual = 0;

            // Apaga el objeto visual del tornillo si fue asignado en el Inspector
            if (objetosTornillos != null && tornillosQuitados < objetosTornillos.Length)
            {
                if (objetosTornillos[tornillosQuitados] != null)
                {
                    objetosTornillos[tornillosQuitados].SetActive(false);
                }
            }

            tornillosQuitados++;
            onTornilloQuitado?.Invoke();
            Debug.Log($"¡Tornillo retirado! ({tornillosQuitados}/{cantidadTornillos})");

            // 4. Si ya quitó los 4 tornillos, abrir ventila y terminar nivel
            if (tornillosQuitados >= cantidadTornillos)
            {
                AbrirVentila();
            }
        }
    }

    private void AbrirVentila()
    {
        abierta = true;

        if (animadorEscotilla != null)
        {
            animadorEscotilla.SetTrigger("Abrir");
        }

        onVentilaAbierta?.Invoke();

        if (Escape.Instance != null)
        {
            Escape.Instance.CompletarNivel();
        }
    }
}