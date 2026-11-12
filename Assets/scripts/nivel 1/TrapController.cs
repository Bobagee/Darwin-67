using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Controla la trampa del Nivel 1.
/// - Colocar (tecla G): pone la trampa en la posicion del raton (con una separacion opcional).
/// - Atrapar (barra espaciadora): solo funciona si el jugador esta a menos de distanciaAtrapar
///   metros de la trampa. CatchResult decide si el raton estaba o no.
/// </summary>
public class TrapController : MonoBehaviour
{
    [Header("Referencias")]
    [SerializeField] private MouseBaitAI raton;
    [SerializeField] private CatchResult resultado;
    [Tooltip("Si se deja vacio se busca el PlayerController de la escena")]
    [SerializeField] private Transform jugador;
    [Tooltip("Objeto de la trampa en la escena (debe ser OTRO objeto, no el que lleva este script). Se oculta al inicio y se activa al colocarla")]
    [SerializeField] private GameObject trampa;

    [Header("Colocacion")]
    [SerializeField] private KeyCode teclaColocar = KeyCode.G;
    [Tooltip("Distancia maxima jugador - raton para poder colocarla (representa la ubicacion aproximada). 0 = sin limite")]
    [SerializeField] private float distanciaMaxColocacion = 4f;
    [Tooltip("Metros que separan la trampa del raton, hacia el jugador (cerca de la escotilla). 0 = justo en el raton")]
    [SerializeField] private float separacionDelRaton = 1.5f;
    [SerializeField] private bool ajustarAlSuelo = true;
    [SerializeField] private LayerMask capaSuelo = ~0;

    [Header("Boton de atrapar")]
    [SerializeField] private KeyCode teclaAtrapar = KeyCode.Space;
    [Tooltip("Metros (en el plano) maximos entre el jugador y la trampa para poder activarla")]
    [SerializeField] private float distanciaAtrapar = 1f;

    [Header("Crafteo")]
    [Tooltip("Si esta activo, no se puede colocar la trampa hasta recoger todos los materiales. Desactivalo para probar")]
    [SerializeField] private bool requerirMateriales = true;
    [SerializeField] private ItemTipo[] materialesRequeridos = { ItemTipo.Palo, ItemTipo.Caja, ItemTipo.Queso, ItemTipo.Hilo };

    [Header("Eventos")]
    public UnityEvent alColocarTrampa;

    private bool colocada;
    private bool bloqueada;
    private readonly HashSet<ItemTipo> recolectados = new HashSet<ItemTipo>();

    public bool EstaColocada => colocada;

    /// <summary>True si ya se recogieron todos los materiales (o si no se requieren).</summary>
    public bool MaterialesListos
    {
        get
        {
            if (!requerirMateriales) return true;
            foreach (ItemTipo tipo in materialesRequeridos)
            {
                if (!recolectados.Contains(tipo)) return false;
            }
            return true;
        }
    }

    /// <summary>Conectalo al evento OnItemPickedUp de cada PickUpItem (opcion Dynamic ItemTipo).</summary>
    public void RegistrarMaterial(ItemTipo tipo)
    {
        recolectados.Add(tipo);
    }

    /// <summary>Conectalo al evento alSoltarQueso de CatchResult: escucha cuando recoges el queso extra.</summary>
    public void EscucharQueso(PickUpItem item)
    {
        if (item != null) item.OnItemPickedUp.AddListener(RegistrarMaterial);
    }

    private void Start()
    {
        if (jugador == null)
        {
            PlayerController pc = FindFirstObjectByType<PlayerController>();
            if (pc != null) jugador = pc.transform;
        }
        if (trampa != null) trampa.SetActive(false);
    }

    private void OnEnable()
    {
        if (raton != null) raton.AlExpirarVentana += AlExpirarVentana;
    }

    private void OnDisable()
    {
        if (raton != null) raton.AlExpirarVentana -= AlExpirarVentana;
    }

    private void Update()
    {
        if (bloqueada) return;

        if (!colocada)
        {
            if (Input.GetKeyDown(teclaColocar)) ColocarTrampa();
        }
        else if (Input.GetKeyDown(teclaAtrapar))
        {
            IntentarAtrapar();
        }
    }

    /// <summary>Coloca la trampa junto al raton. Devuelve false si no se pudo.</summary>
    public bool ColocarTrampa()
    {
        if (colocada || bloqueada || !MaterialesListos) return false;
        if (raton == null || trampa == null || jugador == null) return false;
        if (!raton.EstaLibre) return false; // no hay raton en la zona

        Vector3 posRaton = raton.transform.position;

        if (distanciaMaxColocacion > 0f && DistanciaPlano(jugador.position, posRaton) > distanciaMaxColocacion)
            return false;

        Vector3 haciaJugador = jugador.position - posRaton;
        haciaJugador.y = 0f;
        Vector3 dir = haciaJugador.sqrMagnitude > 0.0001f ? haciaJugador.normalized : raton.transform.forward;

        Vector3 pos = posRaton + dir * separacionDelRaton;

        if (ajustarAlSuelo && Physics.Raycast(pos + Vector3.up * 2f, Vector3.down, out RaycastHit hit, 6f,
                capaSuelo, QueryTriggerInteraction.Ignore))
        {
            pos = hit.point;
        }

        trampa.transform.position = pos;
        trampa.SetActive(true);
        colocada = true;

        // El queso se usa como cebo: si falla, hay que recoger el queso extra
        recolectados.Remove(ItemTipo.Queso);

        raton.ConfigurarTrampa(pos);
        alColocarTrampa?.Invoke();
        return true;
    }

    /// <summary>Boton de la trampa. Tambien se puede llamar desde un boton en el mundo o UI.</summary>
    public void IntentarAtrapar()
    {
        if (!colocada || bloqueada || trampa == null || jugador == null) return;
        if (DistanciaPlano(jugador.position, trampa.transform.position) > distanciaAtrapar) return;

        ResolverIntento();
    }

    /// <summary>Quita la trampa para poder colocarla de nuevo (tras un fallo o al reiniciar el nivel).</summary>
    public void Reiniciar()
    {
        colocada = false;
        bloqueada = false;
        if (trampa != null) trampa.SetActive(false);
    }

    // La ventana de captura se acabo sin que el jugador jalara la trampa
    private void AlExpirarVentana()
    {
        if (colocada && !bloqueada) ResolverIntento();
    }

    private void ResolverIntento()
    {
        bool atrapado = resultado != null && resultado.Evaluar(trampa.transform.position);

        if (atrapado) bloqueada = true; // ya gano: no se puede volver a usar
        else Reiniciar();
    }

    // Distancia ignorando la altura: el pivote del jugador esta ~1 m arriba del piso
    private static float DistanciaPlano(Vector3 a, Vector3 b)
    {
        a.y = 0f;
        b.y = 0f;
        return Vector3.Distance(a, b);
    }
}