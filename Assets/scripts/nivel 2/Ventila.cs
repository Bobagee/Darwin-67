using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

public class Ventila : MonoBehaviour
{
    [Header("Requisito e Interacción")]
    [Tooltip("Opcional: arrastra el objeto Desarmador de la escena; se pedirá que ya lo hayas recogido (desactivado)")]
    [SerializeField] private GameObject objetoDesarmadorEnEscena;
    [SerializeField] private bool exigirDesarmador = true;
    [SerializeField] private KeyCode teclaInteractuar = KeyCode.E;
    [SerializeField] private float distanciaInteractuar = 3.0f;

    [Header("Tornillos (4 objetos con Collider)")]
    [SerializeField] private GameObject[] objetosTornillos = new GameObject[4];
    [SerializeField] private int clicsPorTornillo = 3;

    [Header("Cámara frente a la Ventila")]
    [Tooltip("Crea un objeto vacío justo frente a la ventila mirando hacia los tornillos y arrástralo aquí")]
    [SerializeField] private Transform puntoCamaraVentila;
    [SerializeField] private MonoBehaviour scriptMovimientoJugador;

    [Header("Alerta al quitar el 1er Tornillo")]
    [SerializeField] private float tiempoParaHuir = 10f;
    [SerializeField] private Patrulla patrullaGuardia;
    [SerializeField] private Transform luzGuardia;

    [Header("Brillo al Acercarse")]
    [SerializeField] private float distanciaBrillo = 4f;
    [SerializeField] private Color colorBrillo = new Color(1f, 0.85f, 0.3f, 1f);
    [SerializeField] private float intensidadBrillo = 1.5f;

    [Header("Animación y Eventos de Escape")]
    [SerializeField] private Animator animadorEscotilla;
    [Tooltip("Opcional: arrastra aquí el panel de victoria si quieres que se active al escapar")]
    [SerializeField] private GameObject panelVictoria;
    [SerializeField] private UnityEvent onPrimerTornilloQuitado;
    [SerializeField] private UnityEvent onVentilaAbierta;

    private int[] clicsPorCadaTornillo;
    private int tornillosQuitados = 0;
    private bool modoDesarmarActivo = false;
    private bool alertaIniciada = false;
    private bool abierta = false;
    private float timerEscape;

    private Transform playerTransform;
    private Camera camaraPrincipal;
    private Renderer[] renderers;
    private bool brillando = false;

    private void Start()
    {
        camaraPrincipal = Camera.main;
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null) playerTransform = player.transform;

        clicsPorCadaTornillo = new int[objetosTornillos.Length];
        renderers = GetComponentsInChildren<Renderer>();
        timerEscape = tiempoParaHuir;
    }

   private void Update()
    {
        if (abierta) return;

        Transform refPos = playerTransform != null ? playerTransform : (camaraPrincipal != null ? camaraPrincipal.transform : null);

        // 1. Brillo e interacción con tecla E al acercarse
        if (!modoDesarmarActivo && refPos != null)
        {
            float dist = Vector3.Distance(refPos.position, transform.position);
            ActualizarBrillo(dist <= distanciaBrillo);

            if (Input.GetKeyDown(teclaInteractuar))
            {
                Debug.Log($"[Ventila] Tecla E presionada. Distancia actual: {dist:F2} / Máxima: {distanciaInteractuar}");
                if (dist <= distanciaInteractuar)
                {
                    Interactuar();
                }
            }
        }

        // 2. Modo minijuego frente a los tornillos
        if (modoDesarmarActivo)
        {
            ActualizarBrillo(false);

            // Fuerza la cámara a quedarse en el punto y mirar directo a la ventila cada frame
            if (camaraPrincipal != null && puntoCamaraVentila != null)
            {
                camaraPrincipal.transform.position = puntoCamaraVentila.position;
                camaraPrincipal.transform.LookAt(transform.position);
            }

            if (Input.GetMouseButtonDown(0))
            {
                DetectarClicEnTornillo();
            }

            if (alertaIniciada)
            {
                timerEscape -= Time.deltaTime;

                if (luzGuardia != null)
                {
                    Quaternion rotObjetivo = Quaternion.LookRotation((transform.position - luzGuardia.position).normalized);
                    luzGuardia.rotation = Quaternion.Slerp(luzGuardia.rotation, rotObjetivo, Time.deltaTime * 5f);
                }

                if (timerEscape <= 0f)
                {
                    ReiniciarNivel();
                }
            }
        }
    }

    public void Interactuar()
    {
        if (abierta || modoDesarmarActivo) return;

        // Si se exige desarmador y aún está tirado en la escena (activo), no permite abrir
        if (exigirDesarmador && objetoDesarmadorEnEscena != null && objetoDesarmadorEnEscena.activeInHierarchy)
        {
            Debug.Log("Necesitas recoger el Desarmador primero.");
            return;
        }

        EntrarModoVentila();
    }

    private void EntrarModoVentila()
    {
        modoDesarmarActivo = true;

        if (scriptMovimientoJugador != null)
            scriptMovimientoJugador.enabled = false;

        if (camaraPrincipal != null && puntoCamaraVentila != null)
        {
            camaraPrincipal.transform.SetParent(null);
            camaraPrincipal.transform.position = puntoCamaraVentila.position;
            camaraPrincipal.transform.rotation = puntoCamaraVentila.rotation;
        }

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    private void DetectarClicEnTornillo()
    {
        if (camaraPrincipal == null) return;

        Ray rayo = camaraPrincipal.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(rayo, out RaycastHit hit, 5f))
        {
            for (int i = 0; i < objetosTornillos.Length; i++)
            {
                if (objetosTornillos[i] != null && objetosTornillos[i].activeSelf &&
                    (hit.collider.gameObject == objetosTornillos[i] || hit.collider.transform.IsChildOf(objetosTornillos[i].transform)))
                {
                    clicsPorCadaTornillo[i]++;

                    if (clicsPorCadaTornillo[i] >= clicsPorTornillo)
                    {
                        objetosTornillos[i].SetActive(false);
                        tornillosQuitados++;

                        if (!alertaIniciada)
                        {
                            alertaIniciada = true;
                            timerEscape = tiempoParaHuir;
                            onPrimerTornilloQuitado?.Invoke();

                            if (patrullaGuardia != null)
                                patrullaGuardia.InvestigateNoise(transform.position);
                        }

                        if (tornillosQuitados >= objetosTornillos.Length)
                        {
                            AbrirYHuir();
                        }
                    }
                    break;
                }
            }
        }
    }

    private void AbrirYHuir()
    {
        abierta = true;
        modoDesarmarActivo = false;

        if (animadorEscotilla != null)
            animadorEscotilla.SetTrigger("Abrir");

        if (panelVictoria != null)
            panelVictoria.SetActive(true);

        onVentilaAbierta?.Invoke();
    }

    private void ReiniciarNivel()
    {
        abierta = true;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    private void ActualizarBrillo(bool activar)
    {
        if (brillando == activar) return;
        brillando = activar;

        foreach (Renderer r in renderers)
        {
            if (r == null) continue;
            Material mat = r.material;
            if (activar)
            {
                mat.EnableKeyword("_EMISSION");
                mat.SetColor("_EmissionColor", colorBrillo * intensidadBrillo);
            }
            else
            {
                mat.SetColor("_EmissionColor", Color.black);
                mat.DisableKeyword("_EMISSION");
            }
        }
    }
}