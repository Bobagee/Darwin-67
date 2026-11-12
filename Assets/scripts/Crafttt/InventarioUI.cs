using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InventarioUI : MonoBehaviour
{
    public static InventarioUI Instance { get; private set; }

    [System.Serializable]
    public class ItemListaUI
    {
        public string itemID;          // "Caja", "Palo", "Hilo", "Queso"
        public GameObject lineaTachado;
        [HideInInspector] public bool recolectado;
    }

    [Header("Lista de Materiales (Nivel 1)")]
    [SerializeField] private List<ItemListaUI> materialesTrampa = new List<ItemListaUI>();
    [SerializeField] private Button botonArmar;

    private HashSet<string> inventarioGeneral = new HashSet<string>();

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void OnEnable()
    {
        Escape.OnTrapFailed += ReiniciarMaterialesTrampa;
    }

    private void OnDisable()
    {
        Escape.OnTrapFailed -= ReiniciarMaterialesTrampa;
    }

    private void Start()
    {
        ActualizarEstadoUI();
    }

    public void RegistrarObjeto(string id)
    {
        inventarioGeneral.Add(id);

        foreach (var item in materialesTrampa)
        {
            if (item.itemID == id)
            {
                item.recolectado = true;
            }
        }

        ActualizarEstadoUI();
    }

    public bool TieneObjeto(string id)
    {
        return inventarioGeneral.Contains(id);
    }

    public bool TieneTodosLosMateriales()
    {
        if (materialesTrampa.Count == 0) return false;

        foreach (var item in materialesTrampa)
        {
            if (!item.recolectado) return false;
        }
        return true;
    }

    private void ActualizarEstadoUI()
    {
        foreach (var item in materialesTrampa)
        {
            if (item.lineaTachado != null)
            {
                item.lineaTachado.SetActive(item.recolectado);
            }
        }

        if (botonArmar != null)
        {
            botonArmar.interactable = TieneTodosLosMateriales();
        }
    }

    public void ReiniciarMaterialesTrampa()
    {
        foreach (var item in materialesTrampa)
        {
            item.recolectado = false;
            inventarioGeneral.Remove(item.itemID);
        }

        ActualizarEstadoUI();
    }
}
