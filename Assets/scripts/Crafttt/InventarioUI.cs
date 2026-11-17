using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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

    [Header("Hoja de Papel (Lista de Objetos)")]
    [SerializeField] private GameObject panelHojaLista;
    [SerializeField] private KeyCode teclaSacarHoja = KeyCode.Tab;
    [SerializeField] private bool iniciarConHojaAbierta = false;
    [SerializeField] private bool asomarHojaAlRecoger = true;
    [SerializeField] private float segundosAsomarHoja = 2.0f;

    [Header("Lista de Materiales (Nivel 1)")]
    [SerializeField] private List<ItemListaUI> materialesTrampa = new List<ItemListaUI>();

    [Header("Aviso para Armar (Aparece al tener los 4)")]
    [Tooltip("Texto o imagen en pantalla que dice: Presiona [E] para Armar")]
    [SerializeField] private GameObject avisoArmarUI;

    private HashSet<string> inventarioGeneral = new HashSet<string>();
    private bool hojaAbiertaManualmente;
    private Coroutine rutinaAsomar;

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
        hojaAbiertaManualmente = iniciarConHojaAbierta;
        if (panelHojaLista != null)
            panelHojaLista.SetActive(hojaAbiertaManualmente);

        ActualizarEstadoUI();
    }

    private void Update()
    {
        if (Input.GetKeyDown(teclaSacarHoja))
        {
            AlternarHojaLista();
        }
    }

    public void AlternarHojaLista()
    {
        if (panelHojaLista == null) return;

        if (rutinaAsomar != null)
        {
            StopCoroutine(rutinaAsomar);
            rutinaAsomar = null;
        }

        hojaAbiertaManualmente = !panelHojaLista.activeSelf;
        panelHojaLista.SetActive(hojaAbiertaManualmente);
    }

    public void OcultarHoja()
    {
        if (panelHojaLista == null) return;

        if (rutinaAsomar != null)
        {
            StopCoroutine(rutinaAsomar);
            rutinaAsomar = null;
        }

        hojaAbiertaManualmente = false;
        panelHojaLista.SetActive(false);
    }

    public void RegistrarObjeto(string id)
    {
        inventarioGeneral.Add(id);

        bool esMaterialTrampa = false;
        foreach (var item in materialesTrampa)
        {
            if (item.itemID == id)
            {
                item.recolectado = true;
                esMaterialTrampa = true;
            }
        }

        ActualizarEstadoUI();

        if (esMaterialTrampa && asomarHojaAlRecoger && !hojaAbiertaManualmente && panelHojaLista != null)
        {
            if (rutinaAsomar != null) StopCoroutine(rutinaAsomar);
            rutinaAsomar = StartCoroutine(RutinaAsomarHoja());
        }
    }

    private IEnumerator RutinaAsomarHoja()
    {
        panelHojaLista.SetActive(true);
        yield return new WaitForSeconds(segundosAsomarHoja);
        if (!hojaAbiertaManualmente)
        {
            panelHojaLista.SetActive(false);
        }
        rutinaAsomar = null;
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

        // Aparece solo cuando los 4 materiales fueron recolectados
        if (avisoArmarUI != null)
        {
            avisoArmarUI.SetActive(TieneTodosLosMateriales());
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
    public void OcultarAvisoArmar()
{
    if (avisoArmarUI != null) avisoArmarUI.SetActive(false);
}
}