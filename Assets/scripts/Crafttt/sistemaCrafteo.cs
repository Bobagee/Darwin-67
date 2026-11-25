using System;
using UnityEngine;
using UnityEngine.Events;

public class sistemaCrafteo : MonoBehaviour
{
    [Header("UI de la Mesa de Crafteo")]
    [SerializeField] private GameObject panelMesaCrafteoUI;
    [SerializeField] private PiezaArrastrableUI[] piezasArrastrables;

    [Header("Modelos 3D sobre la Mesa")]
    [Tooltip("0: Palo, 1: Palo + Hilo, 2: Palo + Hilo + Caja. Con el Queso se termina el crafteo y se sale de la mesa")]
    [SerializeField] private GameObject[] pasos3DEnMesa;

    [Header("Conexión con TrapController (Aner)")]
    public UnityEvent onTrampaCompletada;
    public static event Action OnTrampaArmada;

    // Orden en el que se deben arrastrar las piezas al centro
    private static readonly string[] ordenPiezas = { "Palo", "Hilo", "Caja", "Queso" };

    // Cuantas piezas llevamos puestas (0 a 4)
    private int pasoActual = 0;
    private MesaCrafteo mesaActual;

    private void Start()
    {
        OcultarUI();
        ActualizarVisualesPasos();
    }

    public void AbrirMesaCrafteo(MesaCrafteo mesa)
    {
        mesaActual = mesa;
        pasoActual = 0;

        ActualizarVisualesPasos();

        if (piezasArrastrables != null)
        {
            foreach (var pieza in piezasArrastrables)
            {
                if (pieza != null) pieza.ReiniciarPieza();
            }
        }

        if (panelMesaCrafteoUI != null)
            panelMesaCrafteoUI.SetActive(true);
    }

    public void OcultarUI()
    {
        if (panelMesaCrafteoUI != null)
            panelMesaCrafteoUI.SetActive(false);
    }

    public void IntentarAgregarPieza(PiezaArrastrableUI pieza)
    {
        if (pieza == null || pasoActual >= ordenPiezas.Length) return;

        // Solo se acepta la pieza que toca: Palo -> Hilo -> Caja -> Queso
        if (pieza.ItemID != ordenPiezas[pasoActual]) return;

        pieza.MarcarComoUsada();
        pasoActual++;

        if (pasoActual >= ordenPiezas.Length)
        {
            FinalizarCrafteo();   // con el Queso termina y se sale de la mesa
        }
        else
        {
            ActualizarVisualesPasos();
        }
    }

    private void ActualizarVisualesPasos()
    {
        if (pasos3DEnMesa == null) return;

        // Se ve un solo modelo a la vez: el del paso actual. Los demás se apagan.
        for (int i = 0; i < pasos3DEnMesa.Length; i++)
        {
            if (pasos3DEnMesa[i] != null)
                pasos3DEnMesa[i].SetActive(i == pasoActual - 1);
        }
    }

    private void FinalizarCrafteo()
    {
        // Oculta el aviso de "Ve a tu escritorio..."
        if (InventarioUI.Instance != null)
            InventarioUI.Instance.OcultarAvisoArmar();

        // Apaga los modelos de armado sobre la mesa
        pasoActual = 0;
        ActualizarVisualesPasos();

        if (mesaActual != null)
            mesaActual.SalirDeMesa(true);

        OnTrampaArmada?.Invoke();
        onTrampaCompletada?.Invoke();
    }
}