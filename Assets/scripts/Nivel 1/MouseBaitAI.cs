using UnityEngine;

/// <summary>
/// IA de Darwin en el Nivel 1.
/// - Con la trampa puesta, solo se mueve hacia ella cuando el jugador esta a mas de
///   distanciaAlejamiento metros de la trampa (si te acercas, se queda quieto).
/// - Al llegar muerde el cebo y abre la ventana de captura (3 a 5 s segun el GDD).
/// - Si la ventana se acaba o falla la trampa, huye por el ducto, se oculta y reaparece.
/// Se mueve por transform en linea recta (no esquiva muebles ni usa NavMesh).
/// </summary>
public class MouseBaitAI : MonoBehaviour
{
    public enum Estado { Libre, HaciaTrampa, Comiendo, Huyendo, Oculto, Capturado }

    [Header("Referencias")]
    [Tooltip("Si se deja vacio se busca el PlayerController de la escena")]
    [SerializeField] private Transform jugador;
    [Tooltip("Punto del ducto por donde escapa. Si esta vacio, desaparece en el lugar")]
    [SerializeField] private Transform puntoEscape;
    [Tooltip("Opcional: al reaparecer usa la zona de huellas activa. Si esta vacio, vuelve a su posicion inicial")]
    [SerializeField] private FootprintZone zonaHuellas;

    [Header("Deteccion del jugador")]
    [Tooltip("Metros (en el plano, jugador - trampa) a partir de los cuales el raton se atreve a salir")]
    [SerializeField] private float distanciaAlejamiento = 3f;

    [Header("Movimiento")]
    [SerializeField] private float velocidad = 2f;
    [SerializeField] private float velocidadHuida = 9f;
    [SerializeField] private float distanciaLlegada = 0.1f;
    [SerializeField] private float velocidadGiro = 10f;

    [Header("Mordida y ventana de captura")]
    [Tooltip("Segundos comiendo antes de morder el cebo")]
    [SerializeField] private float tiempoParaMorder = 1f;
    [SerializeField] private float ventanaMin = 3f;
    [SerializeField] private float ventanaMax = 5f;

    [Header("Despues de huir")]
    [Tooltip("Segundos oculto antes de reaparecer en otra zona")]
    [SerializeField] private float tiempoOculto = 6f;

    private Estado estado = Estado.Libre;
    private Vector3 posicionInicial;
    private Vector3 posTrampa;
    private float timer;
    private Renderer[] renderers;

    /// <summary>True solo mientras el raton esta mordiendo el cebo: es el momento de jalar la trampa.</summary>
    public bool EnVentanaDeCaptura { get; private set; }
    public bool EstaLibre => estado == Estado.Libre;
    public Estado EstadoActual => estado;

    /// <summary>Se dispara cuando muerde el cebo y abre la ventana.</summary>
    public event System.Action AlMorder;
    /// <summary>Se dispara cuando la ventana se acaba sin que el jugador atrape al raton.</summary>
    public event System.Action AlExpirarVentana;

    private void Awake()
    {
        posicionInicial = transform.position;
        renderers = GetComponentsInChildren<Renderer>(true);
    }

    private void Start()
    {
        if (jugador == null)
        {
            PlayerController pc = FindFirstObjectByType<PlayerController>();
            if (pc != null) jugador = pc.transform;
        }
    }

    private void Update()
    {
        switch (estado)
        {
            case Estado.HaciaTrampa: ActualizarAcercamiento(); break;
            case Estado.Comiendo: ActualizarComida(); break;
            case Estado.Huyendo: ActualizarHuida(); break;
            case Estado.Oculto: ActualizarOculto(); break;
        }
    }

    // ---------------- API publica ----------------

    /// <summary>Lo llama TrapController al colocar la trampa.</summary>
    public void ConfigurarTrampa(Vector3 posicion)
    {
        if (estado != Estado.Libre) return;
        posTrampa = posicion;
        estado = Estado.HaciaTrampa;
    }

    /// <summary>Huye por el ducto. Se puede llamar varias veces sin problema.</summary>
    public void Huir()
    {
        if (estado == Estado.Huyendo || estado == Estado.Oculto || estado == Estado.Capturado) return;
        EnVentanaDeCaptura = false;
        estado = Estado.Huyendo;
    }

    /// <summary>El jugador lo atrapo: se queda en la trampa.</summary>
    public void Capturar()
    {
        EnVentanaDeCaptura = false;
        estado = Estado.Capturado;
        transform.position = posTrampa;
    }

    /// <summary>Reaparece (visible y libre) en la zona de huellas activa o en su posicion inicial.</summary>
    public void Reaparecer()
    {
        transform.position = zonaHuellas != null
            ? zonaHuellas.GetActiveZoneTransform().position
            : posicionInicial;
        SetVisible(true);
        estado = Estado.Libre;
    }

    // ---------------- Estados ----------------

    private void ActualizarAcercamiento()
    {
        if (jugador == null) return;

        // Jugador cerca de la trampa: el raton se queda quieto
        if (DistanciaPlano(jugador.position, posTrampa) <= distanciaAlejamiento) return;

        if (MoverHacia(posTrampa, velocidad))
        {
            estado = Estado.Comiendo;
            EnVentanaDeCaptura = false;
            timer = tiempoParaMorder;
        }
    }

    private void ActualizarComida()
    {
        timer -= Time.deltaTime;
        if (timer > 0f) return;

        if (!EnVentanaDeCaptura)
        {
            // Muerde el cebo: empieza la ventana para jalar la trampa
            EnVentanaDeCaptura = true;
            timer = Random.Range(ventanaMin, ventanaMax);
            AlMorder?.Invoke();
            return;
        }

        // Se acabo la ventana: el jugador llego tarde
        EnVentanaDeCaptura = false;
        AlExpirarVentana?.Invoke();
        Huir(); // por si nadie reacciono al evento
    }

    private void ActualizarHuida()
    {
        if (puntoEscape == null || MoverHacia(puntoEscape.position, velocidadHuida))
        {
            SetVisible(false);
            estado = Estado.Oculto;
            timer = tiempoOculto;
        }
    }

    private void ActualizarOculto()
    {
        timer -= Time.deltaTime;
        if (timer <= 0f) Reaparecer();
    }

    // ---------------- Utilidades ----------------

    /// <summary>Avanza hacia el destino. Devuelve true al llegar.</summary>
    private bool MoverHacia(Vector3 destino, float vel)
    {
        Vector3 pos = transform.position;

        Vector3 plano = new Vector3(destino.x - pos.x, 0f, destino.z - pos.z);
        if (plano.sqrMagnitude > 0.0001f)
        {
            Quaternion objetivo = Quaternion.LookRotation(plano);
            transform.rotation = Quaternion.Slerp(transform.rotation, objetivo, velocidadGiro * Time.deltaTime);
        }

        transform.position = Vector3.MoveTowards(pos, destino, vel * Time.deltaTime);
        return (transform.position - destino).sqrMagnitude <= distanciaLlegada * distanciaLlegada;
    }

    private void SetVisible(bool visible)
    {
        foreach (Renderer r in renderers)
        {
            if (r != null) r.enabled = visible;
        }
    }

    // Distancia ignorando la altura: el pivote del jugador esta ~1 m arriba del piso
    private static float DistanciaPlano(Vector3 a, Vector3 b)
    {
        a.y = 0f;
        b.y = 0f;
        return Vector3.Distance(a, b);
    }
}
