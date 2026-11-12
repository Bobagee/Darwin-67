using System;
using UnityEngine;
using UnityEngine.Events;

public class sistemaCrafteo : MonoBehaviour
{
    [Header("Panel de Crafteo")]
    [SerializeField] private GameObject panelCrafteoUI;
    [SerializeField] private GameObject[] pasosVisualesDibujo; // 0: Palo+Hilo, 1: +Caja, 2: +Queso

    [Header("Conexión con TrapController (Aner)")]
    public UnityEvent onTrampaCompletada;
    public static event Action OnTrampaArmada;

    private int pasoActual = 0;
    private string primerSeleccionadoPaso1 = "";

    private void Start()
    {
        if (panelCrafteoUI != null) panelCrafteoUI.SetActive(false);
    }

    public void AbrirMesaCrafteo()
    {
        if (InventarioUI.Instance != null && !InventarioUI.Instance.TieneTodosLosMateriales())
            return;

        pasoActual = 0;
        primerSeleccionadoPaso1 = "";
        ActualizarVisualesPasos();

        if (panelCrafteoUI != null)
        {
            panelCrafteoUI.SetActive(true);
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }

    public void SeleccionarPiezaDibujo(string itemID)
    {
        // PASO 1: Palo + Hilo
        if (pasoActual == 0)
        {
            if (itemID == "Palo" || itemID == "Hilo")
            {
                if (string.IsNullOrEmpty(primerSeleccionadoPaso1))
                {
                    primerSeleccionadoPaso1 = itemID;
                }
                else if (primerSeleccionadoPaso1 != itemID)
                {
                    pasoActual = 1;
                    ActualizarVisualesPasos();
                }
            }
            else
            {
                primerSeleccionadoPaso1 = "";
            }
        }
        // PASO 2: + Caja
        else if (pasoActual == 1)
        {
            if (itemID == "Caja")
            {
                pasoActual = 2;
                ActualizarVisualesPasos();
            }
        }
        // PASO 3: + Queso
        else if (pasoActual == 2)
        {
            if (itemID == "Queso")
            {
                pasoActual = 3;
                ActualizarVisualesPasos();
                FinalizarCrafteo();
            }
        }
    }

    private void ActualizarVisualesPasos()
    {
        if (pasosVisualesDibujo == null) return;

        for (int i = 0; i < pasosVisualesDibujo.Length; i++)
        {
            if (pasosVisualesDibujo[i] != null)
                pasosVisualesDibujo[i].SetActive(i < pasoActual);
        }
    }

    private void FinalizarCrafteo()
    {
        if (panelCrafteoUI != null)
        {
            panelCrafteoUI.SetActive(false);
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        OnTrampaArmada?.Invoke();
        onTrampaCompletada?.Invoke();
    }
}