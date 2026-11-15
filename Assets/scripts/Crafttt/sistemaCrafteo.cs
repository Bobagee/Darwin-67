using System;
using UnityEngine;
using UnityEngine.Events;

public class sistemaCrafteo : MonoBehaviour
{
    [Header("UI de la Mesa de Crafteo")]
    [SerializeField] private GameObject panelMesaCrafteoUI;
    [SerializeField] private PiezaArrastrableUI[] piezasArrastrables;

    [Header("Modelos 3D sobre la Mesa (Opcional)")]
    [Tooltip("0: Palo+Hilo en mesa, 1: +Caja en mesa, 2: Trampa completa con Queso")]
    [SerializeField] private GameObject[] pasos3DEnMesa;

    [Header("Conexión con TrapController (Aner)")]
    public UnityEvent onTrampaCompletada;
    public static event Action OnTrampaArmada;

    private int pasoActual = 0;
    private PiezaArrastrableUI primeraPiezaPaso1;
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
        primeraPiezaPaso1 = null;

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
        string id = pieza.ItemID;

        // PASO 1: Palo + Hilo
        if (pasoActual == 0)
        {
            if (id == "Palo" || id == "Hilo")
            {
                if (primeraPiezaPaso1 == null)
                {
                    primeraPiezaPaso1 = pieza;
                    pieza.MarcarComoUsada();
                }
                else if (primeraPiezaPaso1.ItemID != id)
                {
                    pieza.MarcarComoUsada();
                    pasoActual = 1;
                    ActualizarVisualesPasos();
                }
            }
        }
        // PASO 2: + Caja
        else if (pasoActual == 1)
        {
            if (id == "Caja")
            {
                pieza.MarcarComoUsada();
                pasoActual = 2;
                ActualizarVisualesPasos();
            }
        }
        // PASO 3: + Queso
        else if (pasoActual == 2)
        {
            if (id == "Queso")
            {
                pieza.MarcarComoUsada();
                pasoActual = 3;
                ActualizarVisualesPasos();
                FinalizarCrafteo();
            }
        }
    }

    private void ActualizarVisualesPasos()
    {
        if (pasos3DEnMesa == null) return;

        for (int i = 0; i < pasos3DEnMesa.Length; i++)
        {
            if (pasos3DEnMesa[i] != null)
            {
                // Muestra solo el paso actual alcanzado (o los acumulados)
                pasos3DEnMesa[i].SetActive(i == pasoActual - 1);
            }
        }
    }

    private void FinalizarCrafteo()
    {
        // Oculta el aviso de "Ve a tu escritorio..."
        if (InventarioUI.Instance != null)
            InventarioUI.Instance.OcultarAvisoArmar();

        // Oculta los modelos de armado sobre la mesa
        pasoActual = 0;
        ActualizarVisualesPasos();

        if (mesaActual != null)
            mesaActual.SalirDeMesa(true);

        OnTrampaArmada?.Invoke();
        onTrampaCompletada?.Invoke();
    }
}