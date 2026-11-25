using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.AI;
using UnityEngine.Playables;
using System.Collections.Generic;

public class Ventila : MonoBehaviour
{
    [Header("Requisito e Interacción")]
    [SerializeField] private GameObject objetoDesarmadorEnEscena;
    [SerializeField] private bool exigirDesarmador = true;
    [SerializeField] private KeyCode teclaInteractuar = KeyCode.E;
    [SerializeField] private float distanciaInteractuar = 3.0f;

    [Header("Tornillos (4 objetos con Collider)")]
    [SerializeField] private GameObject[] objetosTornillos = new GameObject[4];
    [SerializeField] private int clicsPorTornillo = 3;

    [Header("Cámara frente a la Ventila")]
    [SerializeField] private Transform puntoCamaraVentila;
    [SerializeField] private MonoBehaviour scriptMovimientoJugador;

    [Header("Tensión: Luz Morada y Pantalla Morada")]
    [SerializeField] private float tiempoParaHuir = 8f;
    [SerializeField] private float retrasoCaminarGuardia = 1.2f;
    [SerializeField] private float velocidadGiroLuz = 3.5f;
    [SerializeField] private float velocidadAcercamientoGuardia = 1.4f;
    [SerializeField] private float distanciaCapturaLuz = 2.5f;
    [SerializeField] private Patrulla patrullaGuardia;
    [SerializeField] private Transform luzGuardia;
    [SerializeField] private float rangoLuzAlerta = 18f;
    [SerializeField] private float intensidadLuzAlerta = 12f;
    [Tooltip("Arrastra aquí tu imagen UI Filtro_LuzMorada del Canvas")]
    [SerializeField] private Image filtroPantallaMorada;
    [SerializeField] private float opacidadMaximaMorada = 0.45f;

    [Header("Checkpoint y Audio al Ser Atrapado")]
    [SerializeField] private Transform puntoCheckpoint;
    [SerializeField] private Transform puntoInicioGuardia;
    [SerializeField] private AudioSource audioSourceAtrapado;
    [SerializeField] private AudioClip clipAtrapado;

    [Header("Cinemática al Escapar")]
    [SerializeField] private PlayableDirector cinematicaEscape;
    [SerializeField] private Animator animadorEscotilla;
    [SerializeField] private GameObject panelVictoria;
    [SerializeField] private UnityEvent onPrimerTornilloQuitado;
    [SerializeField] private UnityEvent onVentilaAbierta;

    [Header("Brillo al Acercarse")]
    [SerializeField] private float distanciaBrillo = 4f;
    [SerializeField] private Color colorBrillo = new Color(1f, 0.85f, 0.3f, 1f);
    [SerializeField] private float intensidadBrillo = 1.5f;

    private int[] clicsPorCadaTornillo;
    private int tornillosQuitados = 0;
    private bool modoDesarmarActivo = false;
    private bool alertaIniciada = false;
    private bool guardiaYaInvestigo = false;
    private bool abierta = false;
    private float timerEscape;
    private float tiempoDesdePrimerTornillo = 0f;

    private Transform playerTransform;
    private Camera camaraPrincipal;
    private Transform padreOriginalCamara;
    private Vector3 posLocalOriginalCamara;
    private Quaternion rotLocalOriginalCamara;

    private List<MonoBehaviour> scriptsDesactivados = new List<MonoBehaviour>();

    private Light componenteLuzGuardia;
    private float rangoLuzOriginal;
    private float intensidadLuzOriginal;

    private Renderer[] renderers;
    private bool brillando = false;

    private void Start()
    {
        camaraPrincipal = Camera.main;
        if (camaraPrincipal != null)
        {
            padreOriginalCamara = camaraPrincipal.transform.parent;
            posLocalOriginalCamara = camaraPrincipal.transform.localPosition;
            rotLocalOriginalCamara = camaraPrincipal.transform.localRotation;
        }

        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null) playerTransform = player.transform;

        if (luzGuardia != null)
        {
            componenteLuzGuardia = luzGuardia.GetComponentInChildren<Light>();
            if (componenteLuzGuardia != null)
            {
                rangoLuzOriginal = componenteLuzGuardia.range;
                intensidadLuzOriginal = componenteLuzGuardia.intensity;
            }
        }

        SetOpacidadFiltroMorado(0f);

        clicsPorCadaTornillo = new int[objetosTornillos.Length];
        renderers = GetComponentsInChildren<Renderer>();
        timerEscape = tiempoParaHuir;
    }

    private void Update()
    {
        if (abierta) return;

        Transform refPos = playerTransform != null ? playerTransform : (camaraPrincipal != null ? camaraPrincipal.transform : null);

        // Si otro script (como TiempoBajoLuz) teletransportó al Player lejos al Checkpoint, libera la cámara de inmediato
        if (modoDesarmarActivo && playerTransform != null && Vector3.Distance(playerTransform.position, transform.position) > distanciaInteractuar + 2f)
        {
            ReiniciarEstadoVentila();
            RestaurarCamaraYJugador();
            return;
        }

        // 1. Brillo e interacción con tecla E
        if (!modoDesarmarActivo && refPos != null)
        {
            float dist = Vector3.Distance(refPos.position, transform.position);
            if (dist <= distanciaInteractuar)
            {
                bool falta = exigirDesarmador && objetoDesarmadorEnEscena != null && objetoDesarmadorEnEscena.activeInHierarchy;
                UIInteraccion.TextoExtra = falta ? "Necesitas un desarmador" : "Pulsa [E] para abrir la ventila";
            }
            ActualizarBrillo(dist <= distanciaBrillo);

            if (dist <= distanciaInteractuar && Input.GetKeyDown(teclaInteractuar))
            {
                Interactuar();
            }
        }



        // 2. Minijuego frente a los tornillos
        if (modoDesarmarActivo)
        {
            ActualizarBrillo(false);

            if (Input.GetMouseButtonDown(0))
            {
                DetectarClicEnTornillo();
            }

            if (alertaIniciada)
            {
                tiempoDesdePrimerTornillo += Time.deltaTime;
                timerEscape -= Time.deltaTime;

                // Sube gradualmente la opacidad morada en pantalla conforme se acaba el tiempo
                float progreso = 1f - Mathf.Clamp01(timerEscape / tiempoParaHuir);
                SetOpacidadFiltroMorado(progreso * opacidadMaximaMorada);

                // Apunta la luz morada directo a la ventila para ver el haz morado iluminando la placa frente a ti
                Vector3 objetivoMirada = transform.position;

                if (luzGuardia != null)
                {
                    Vector3 dirLuz = (objetivoMirada - luzGuardia.position).normalized;
                    if (dirLuz != Vector3.zero)
                    {
                        Quaternion rotObjetivo = Quaternion.LookRotation(dirLuz);
                        luzGuardia.rotation = Quaternion.Slerp(luzGuardia.rotation, rotObjetivo, Time.deltaTime * velocidadGiroLuz);
                    }

                    float anguloLuz = Vector3.Angle(luzGuardia.forward, dirLuz);
                    float distLuz = Vector3.Distance(luzGuardia.position, objetivoMirada);
                    if (anguloLuz < 22f && distLuz <= distanciaCapturaLuz)
                    {
                        AtrapadoVolverACheckpoint();
                        return;
                    }
                }

                if (tiempoDesdePrimerTornillo >= retrasoCaminarGuardia && patrullaGuardia != null)
                {
                    if (!guardiaYaInvestigo)
                    {
                        guardiaYaInvestigo = true;
                        patrullaGuardia.InvestigateNoise(objetivoMirada);

                        NavMeshAgent agente = patrullaGuardia.GetComponent<NavMeshAgent>();
                        if (agente != null)
                            agente.speed = velocidadAcercamientoGuardia;
                    }

                    NavMeshAgent nav = patrullaGuardia.GetComponent<NavMeshAgent>();
                    if (nav == null || !nav.enabled)
                    {
                        Vector3 destinoPlano = new Vector3(objetivoMirada.x, patrullaGuardia.transform.position.y, objetivoMirada.z);
                        patrullaGuardia.transform.position = Vector3.MoveTowards(
                            patrullaGuardia.transform.position,
                            destinoPlano,
                            velocidadAcercamientoGuardia * Time.deltaTime
                        );
                        patrullaGuardia.transform.LookAt(destinoPlano);
                    }
                }

                if (timerEscape <= 0f)
                {
                    AtrapadoVolverACheckpoint();
                }
            }
        }
    }

    private void LateUpdate()
    {
        if (modoDesarmarActivo && !abierta && camaraPrincipal != null && puntoCamaraVentila != null)
        {
            camaraPrincipal.transform.position = puntoCamaraVentila.position;
            camaraPrincipal.transform.LookAt(transform.position);
        }
    }

    public void Interactuar()
    {
        if (abierta || modoDesarmarActivo) return;

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
        scriptsDesactivados.Clear();

        if (scriptMovimientoJugador != null && scriptMovimientoJugador.enabled)
        {
            scriptMovimientoJugador.enabled = false;
            scriptsDesactivados.Add(scriptMovimientoJugador);
        }

        if (playerTransform != null)
        {
            foreach (MonoBehaviour script in playerTransform.GetComponentsInChildren<MonoBehaviour>(true))
            {
                if (script != null && script != this && script.enabled)
                {
                    script.enabled = false;
                    scriptsDesactivados.Add(script);
                }
            }
        }

        if (camaraPrincipal != null && puntoCamaraVentila != null)
        {
            camaraPrincipal.transform.SetParent(null);
            camaraPrincipal.transform.position = puntoCamaraVentila.position;
            camaraPrincipal.transform.LookAt(transform.position);
        }

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    private void RestaurarCamaraYJugador()
    {
        modoDesarmarActivo = false;
        SetOpacidadFiltroMorado(0f);

        if (camaraPrincipal != null)
        {
            if (padreOriginalCamara != null)
                camaraPrincipal.transform.SetParent(padreOriginalCamara);

            camaraPrincipal.transform.localPosition = posLocalOriginalCamara;
            camaraPrincipal.transform.localRotation = rotLocalOriginalCamara;
            camaraPrincipal.enabled = true;
        }

        foreach (MonoBehaviour script in scriptsDesactivados)
        {
            if (script != null)
                script.enabled = true;
        }
        scriptsDesactivados.Clear();

        if (scriptMovimientoJugador != null)
            scriptMovimientoJugador.enabled = true;

        if (playerTransform != null)
        {
            PlayerController pc = playerTransform.GetComponentInChildren<PlayerController>(true);
            if (pc != null) pc.enabled = true;
        }

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
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
                            tiempoDesdePrimerTornillo = 0f;
                            timerEscape = tiempoParaHuir;

                            if (componenteLuzGuardia != null)
                            {
                                componenteLuzGuardia.range = rangoLuzAlerta;
                                componenteLuzGuardia.intensity = intensidadLuzAlerta;
                            }

                            onPrimerTornilloQuitado?.Invoke();
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

    private void AtrapadoVolverACheckpoint()
    {
        if (clipAtrapado != null)
        {
            if (audioSourceAtrapado != null)
            {
                audioSourceAtrapado.Stop();
                audioSourceAtrapado.clip = clipAtrapado;
                audioSourceAtrapado.Play();
            }
            else if (camaraPrincipal != null)
            {
                AudioSource.PlayClipAtPoint(clipAtrapado, camaraPrincipal.transform.position);
            }
        }

        RestaurarCamaraYJugador();

        if (playerTransform != null && puntoCheckpoint != null)
        {
            CharacterController cc = playerTransform.GetComponent<CharacterController>();
            if (cc != null) cc.enabled = false;

            playerTransform.position = puntoCheckpoint.position;
            playerTransform.rotation = puntoCheckpoint.rotation;

            if (cc != null) cc.enabled = true;
        }

        ReiniciarEstadoVentila();
    }

    private void ReiniciarEstadoVentila()
    {
        alertaIniciada = false;
        guardiaYaInvestigo = false;
        tornillosQuitados = 0;
        timerEscape = tiempoParaHuir;
        SetOpacidadFiltroMorado(0f);

        for (int i = 0; i < objetosTornillos.Length; i++)
        {
            clicsPorCadaTornillo[i] = 0;
            if (objetosTornillos[i] != null)
                objetosTornillos[i].SetActive(true);
        }

        if (componenteLuzGuardia != null)
        {
            componenteLuzGuardia.range = rangoLuzOriginal;
            componenteLuzGuardia.intensity = intensidadLuzOriginal;
        }

        if (patrullaGuardia != null && puntoInicioGuardia != null)
        {
            NavMeshAgent nav = patrullaGuardia.GetComponent<NavMeshAgent>();
            if (nav != null && nav.enabled)
                nav.Warp(puntoInicioGuardia.position);
            else
                patrullaGuardia.transform.position = puntoInicioGuardia.position;
        }
    }

    private void AbrirYHuir()
    {
        abierta = true;
        modoDesarmarActivo = false;
        SetOpacidadFiltroMorado(0f);

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        if (animadorEscotilla != null)
            animadorEscotilla.SetTrigger("Abrir");

        if (cinematicaEscape != null)
            cinematicaEscape.Play();

        if (panelVictoria != null)
            panelVictoria.SetActive(true);

        onVentilaAbierta?.Invoke();
    }

    private void SetOpacidadFiltroMorado(float alpha)
    {
        if (filtroPantallaMorada == null) return;
        Color c = filtroPantallaMorada.color;
        c.a = alpha;
        filtroPantallaMorada.color = c;
        filtroPantallaMorada.gameObject.SetActive(alpha > 0.001f);
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