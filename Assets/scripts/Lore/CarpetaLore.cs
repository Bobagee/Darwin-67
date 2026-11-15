using UnityEngine;

public class CarpetaLore : MonoBehaviour
{
    [Header("Archivo de Bitácora (1 al 5)")]
    [SerializeField] private int numeroArchivo = 1;
    [Tooltip("Arrastra aquí la imagen con el texto ya puesto (déjalo vacío en graybox)")]
    [SerializeField] private Sprite imagenArchivo;
    [Tooltip("Color temporal para diferenciar los 5 archivos en graybox (ponlo en blanco cuando uses la imagen final)")]
    [SerializeField] private Color colorGraybox = Color.white;

    [Header("Brillo y Alcance")]
    [SerializeField] private float distanciaBrillo = 4.5f;
    [SerializeField] private float distanciaRecoger = 3f;
    [Range(15f, 90f)]
    [SerializeField] private float anguloMirada = 50f;
    [SerializeField] private Color colorBrillo = new Color(1f, 0.85f, 0.3f);
    [SerializeField] private float intensidadBrillo = 1.4f;

    [Header("Opciones")]
    [SerializeField] private bool ocultarAlRecoger = true;
    [SerializeField] private KeyCode teclaInteractuar = KeyCode.E;

    private Transform puntoReferencia;
    private Camera camaraJugador;
    private Renderer[] renderers;
    private bool recogida = false;
    private bool brillando = false;

    public int NumeroArchivo => numeroArchivo;
    public Sprite ImagenArchivo => imagenArchivo;
    public Color ColorGraybox => colorGraybox;

    private void Start()
    {
        renderers = GetComponentsInChildren<Renderer>();
        camaraJugador = Camera.main;

        if (camaraJugador != null)
        {
            puntoReferencia = camaraJugador.transform;
        }
        else
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null) puntoReferencia = playerObj.transform;
        }

        SetBrillo(false);
    }

    private void Update()
    {
        if (recogida || puntoReferencia == null) return;
        if (Bitacoras.Instancia != null && Bitacoras.Instancia.EstaAbierto) return;

        float distancia = Vector3.Distance(puntoReferencia.position, transform.position);

        bool debeBrillar = distancia <= distanciaBrillo;
        if (debeBrillar != brillando)
        {
            SetBrillo(debeBrillar);
        }

        if (distancia <= distanciaRecoger && Input.GetKeyDown(teclaInteractuar))
        {
            if (JugadorMirandoHaciaCarpeta())
            {
                Interactuar();
            }
        }
    }

    private bool JugadorMirandoHaciaCarpeta()
    {
        if (camaraJugador == null) return true;

        Vector3 direccion = (transform.position - camaraJugador.transform.position).normalized;
        float angulo = Vector3.Angle(camaraJugador.transform.forward, direccion);
        return angulo <= anguloMirada;
    }

    public void Interactuar()
    {
        if (recogida) return;

        recogida = true;
        SetBrillo(false);

        if (Bitacoras.Instancia != null)
        {
            Bitacoras.Instancia.RegistrarYAbrir(this);
        }

        if (ocultarAlRecoger)
        {
            gameObject.SetActive(false);
        }
    }

    private void SetBrillo(bool activo)
    {
        brillando = activo;
        if (renderers == null) return;

        Color emisionFinal = activo ? colorBrillo * intensidadBrillo : Color.black;

        foreach (Renderer r in renderers)
        {
            if (r == null) continue;
            Material mat = r.material;

            if (activo)
                mat.EnableKeyword("_EMISSION");
            else
                mat.DisableKeyword("_EMISSION");

            if (mat.HasProperty("_EmissionColor"))
                mat.SetColor("_EmissionColor", emisionFinal);
        }
    }
}