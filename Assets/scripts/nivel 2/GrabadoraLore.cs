using UnityEngine;

public class GrabadoraLore : MonoBehaviour
{
    [Header("Contenido de esta Grabadora")]
    [Tooltip("Arrastra la imagen de la grabadora con la hoja y el texto ya dibujado")]
    [SerializeField] private Sprite dibujoGrabadora;
    [SerializeField] private AudioClip clipGrabacion;

    [Header("Interacción y Resplandor")]
    [SerializeField] private float distanciaBrillo = 4.5f;
    [SerializeField] private float distanciaInteractuar = 2.5f;
   
    [SerializeField] private float distanciaCerrar = 4f;
    [Range(15f, 90f)]
    [SerializeField] private float anguloMirada = 50f;
    [SerializeField] private KeyCode teclaInteractuar = KeyCode.E;

    [Header("Visual del Brillo")]
    [SerializeField] private Color colorBrillo = new Color(1f, 0.85f, 0.3f, 1f);
    [SerializeField] private float intensidadBrillo = 1.5f;
    [SerializeField] private Light luzOpcional;

    private Transform referenciaJugador;
    private Camera camaraPrincipal;
    private Renderer[] renderers;
    private Color[] coloresOriginales;
    private bool brillando = false;

  private void Start()
{
    camaraPrincipal = Camera.main;
    GameObject player = GameObject.FindGameObjectWithTag("Player");
    if (player != null)
        referenciaJugador = player.transform;
    else if (camaraPrincipal != null)
        referenciaJugador = camaraPrincipal.transform;
        renderers = GetComponentsInChildren<Renderer>();
        coloresOriginales = new Color[renderers.Length];

        for (int i = 0; i < renderers.Length; i++)
        {
            if (renderers[i].material.HasProperty("_BaseColor"))
                coloresOriginales[i] = renderers[i].material.GetColor("_BaseColor");
            else if (renderers[i].material.HasProperty("_Color"))
                coloresOriginales[i] = renderers[i].material.color;
        }

        if (luzOpcional != null) luzOpcional.enabled = false;
    }

   private bool estaAbiertaEstaGrabadora = false;

    private void Update()
    {
        if (referenciaJugador == null) return;

        bool visorAbierto = VisorGrabadora.Instancia != null && VisorGrabadora.Instancia.EstaAbierto;

        if (estaAbiertaEstaGrabadora)
        {
            ActualizarBrillo(false);

            if (!visorAbierto)
            {
                estaAbiertaEstaGrabadora = false;
                return;
            }

            float distAbierta = Vector3.Distance(referenciaJugador.position, transform.position);
            if (distAbierta > distanciaCerrar)
            {
                Cerrar();
                return;
            }

            if (Input.GetKeyDown(teclaInteractuar))
            {
                Cerrar();
            }
            return;
        }

        if (visorAbierto)
        {
            ActualizarBrillo(false);
            return;
        }

        float distancia = Vector3.Distance(referenciaJugador.position, transform.position);
        bool mirando = MirandoGrabadora();
        if (distancia <= distanciaInteractuar && mirando) UIInteraccion.TextoExtra = "Pulsa [E] para escuchar";

        ActualizarBrillo(distancia <= distanciaBrillo && mirando);

        if (distancia <= distanciaInteractuar && mirando && Input.GetKeyDown(teclaInteractuar))
        {
            Activar();
        }
    }

    private void ActualizarBrillo(bool activar)
    {
        if (brillando == activar) return;
        brillando = activar;

        if (luzOpcional != null)
            luzOpcional.enabled = activar;

        for (int i = 0; i < renderers.Length; i++)
        {
            if (renderers[i] == null) continue;
            Material mat = renderers[i].material;

            if (activar)
            {
                mat.EnableKeyword("_EMISSION");
                mat.SetColor("_EmissionColor", colorBrillo * intensidadBrillo);
                if (mat.HasProperty("_BaseColor"))
                    mat.SetColor("_BaseColor", Color.Lerp(coloresOriginales[i], colorBrillo, 0.35f));
            }
            else
            {
                mat.SetColor("_EmissionColor", Color.black);
                mat.DisableKeyword("_EMISSION");
                if (mat.HasProperty("_BaseColor"))
                    mat.SetColor("_BaseColor", coloresOriginales[i]);
            }
        }
    }

    private bool MirandoGrabadora()
    {
        if (camaraPrincipal == null) return true;
        Vector3 dir = (transform.position - camaraPrincipal.transform.position).normalized;
        return Vector3.Angle(camaraPrincipal.transform.forward, dir) <= anguloMirada;
    }

   public void Activar()
    {
        ActualizarBrillo(false);
        estaAbiertaEstaGrabadora = true;

        if (VisorGrabadora.Instancia != null)
        {
            VisorGrabadora.Instancia.ReproducirGrabadora(dibujoGrabadora, clipGrabacion);
        }
    }
public void Cerrar()
    {
        estaAbiertaEstaGrabadora = false;

        if (VisorGrabadora.Instancia != null)
        {
            VisorGrabadora.Instancia.CerrarGrabadora();
        }
    }
}