using UnityEngine;

public class MesaCrafteo : Iteraccion
{
    [Header("Cámaras y Jugador")]
    [SerializeField] private GameObject camaraJugador;
    [SerializeField] private GameObject camaraMesa;
    [SerializeField] private MonoBehaviour playerController;

    [Header("UI de la Mesa")]
    [Tooltip("Texto en pantalla al mirar la mesa: Build Trap [E]")]
    [SerializeField] private GameObject promptBuildTrapUI;
    [SerializeField] private sistemaCrafteo sistemaCraft;

    [Header("Resplandor de la Mesa")]
    [SerializeField] private Color colorBrillo = new Color(1f, 0.9f, 0.4f, 1f);
    [SerializeField] private float intensidadBrillo = 1.2f;

    [Header("Controles")]
    [SerializeField] private KeyCode teclaSalirMesa = KeyCode.Escape;

    private Renderer[] renderers;
    private Color[] coloresOriginales;
    private bool enMesa = false;
    private bool trampaYaArmada = false;

    private void Awake()
    {
        renderers = GetComponentsInChildren<Renderer>();
        coloresOriginales = new Color[renderers.Length];

        for (int i = 0; i < renderers.Length; i++)
        {
            if (renderers[i].material.HasProperty("_BaseColor"))
                coloresOriginales[i] = renderers[i].material.GetColor("_BaseColor");
            else if (renderers[i].material.HasProperty("_Color"))
                coloresOriginales[i] = renderers[i].material.color;
        }

        Escape.OnTrapFailed += ReiniciarMesa;
    }

    private void OnDestroy()
    {
        Escape.OnTrapFailed -= ReiniciarMesa;
    }

    private void Start()
    {
        if (camaraMesa != null) camaraMesa.SetActive(false);
        if (promptBuildTrapUI != null) promptBuildTrapUI.SetActive(false);
    }

    private void Update()
    {
        if (enMesa && Input.GetKeyDown(teclaSalirMesa))
        {
            SalirDeMesa(false);
        }
    }

    private bool PuedeArmar()
    {
        return !trampaYaArmada &&
               !enMesa &&
               InventarioUI.Instance != null &&
               InventarioUI.Instance.TieneTodosLosMateriales();
    }

    public override void OnMirar(bool mirando)
    {
        bool activar = mirando && PuedeArmar();

        if (promptBuildTrapUI != null)
            promptBuildTrapUI.SetActive(activar);

        for (int i = 0; i < renderers.Length; i++)
        {
            Material mat = renderers[i].material;
            if (activar)
            {
                mat.EnableKeyword("_EMISSION");
                mat.SetColor("_EmissionColor", colorBrillo * intensidadBrillo);

                Color resaltado = Color.Lerp(coloresOriginales[i], colorBrillo, 0.35f);
                if (mat.HasProperty("_BaseColor")) mat.SetColor("_BaseColor", resaltado);
                if (mat.HasProperty("_Color")) mat.color = resaltado;
            }
            else
            {
                mat.SetColor("_EmissionColor", Color.black);
                if (mat.HasProperty("_BaseColor")) mat.SetColor("_BaseColor", coloresOriginales[i]);
                if (mat.HasProperty("_Color")) mat.color = coloresOriginales[i];
            }
        }
    }

    public override void Interactuar()
    {
        if (!PuedeArmar()) return;

        enMesa = true;
        OnMirar(false);

        if (InventarioUI.Instance != null)
            InventarioUI.Instance.OcultarHoja();

        if (playerController != null) playerController.enabled = false;
        if (camaraJugador != null) camaraJugador.SetActive(false);
        if (camaraMesa != null) camaraMesa.SetActive(true);

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        if (sistemaCraft != null)
            sistemaCraft.AbrirMesaCrafteo(this);
    }

    public void SalirDeMesa(bool completado)
    {
        enMesa = false;
        if (completado) trampaYaArmada = true;

        if (sistemaCraft != null)
            sistemaCraft.OcultarUI();

        if (camaraMesa != null) camaraMesa.SetActive(false);
        if (camaraJugador != null) camaraJugador.SetActive(true);
        if (playerController != null) playerController.enabled = true;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void ReiniciarMesa()
    {
        trampaYaArmada = false;
        enMesa = false;
        OnMirar(false);
    }
}