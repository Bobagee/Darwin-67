using UnityEngine;

public class GrabadoraLore : MonoBehaviour
{
    [Header("Contenido de esta Grabadora")]
    [Tooltip("Arrastra el dibujo de la grabadora con la hoja (en graybox puedes dejar el placeholder del panel)")]
    [SerializeField] private Sprite dibujoGrabadora;
    [TextArea(5, 12)]
    [SerializeField] private string textoTranscripcion = "Grabación #01: El sujeto 67 logró escapar de la jaula...";
    [SerializeField] private AudioClip clipGrabacion;

    [Header("Interacción")]
    [SerializeField] private float distanciaInteractuar = 2.5f;
    [Range(15f, 90f)]
    [SerializeField] private float anguloMirada = 55f;
    [SerializeField] private KeyCode teclaInteractuar = KeyCode.E;

    private Transform referenciaJugador;
    private Camera camaraPrincipal;

    private void Start()
    {
        camaraPrincipal = Camera.main;
        if (camaraPrincipal != null)
        {
            referenciaJugador = camaraPrincipal.transform;
        }
        else
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null) referenciaJugador = player.transform;
        }
    }

    private void Update()
    {
        if (referenciaJugador == null) return;
        if (VisorGrabadora.Instancia != null && VisorGrabadora.Instancia.EstaAbierto) return;

        float distancia = Vector3.Distance(referenciaJugador.position, transform.position);
        if (distancia <= distanciaInteractuar && Input.GetKeyDown(teclaInteractuar))
        {
            if (MirandoGrabadora())
            {
                Activar();
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
        if (VisorGrabadora.Instancia != null)
        {
            VisorGrabadora.Instancia.ReproducirGrabadora(dibujoGrabadora, textoTranscripcion, clipGrabacion);
        }
    }
}