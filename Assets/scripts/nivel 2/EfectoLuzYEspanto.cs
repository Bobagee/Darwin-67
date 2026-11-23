using UnityEngine;
using UnityEngine.UI;

public class EfectoLuzYEspanto : MonoBehaviour
{
    [Header("Referencias")]
    [SerializeField] private Transform luzGuardia;
    [SerializeField] private Transform camaraJugador;
    [SerializeField] private Image filtroPantallaMorada;

    [Header("Detección de Luz")]
    [SerializeField] private float rangoMaximoLuz = 12f;
    [SerializeField] private float anguloConoLuz = 28f;
    [SerializeField] private LayerMask capasObstaculos = ~0;

    [Header("Tinte Morado en Pantalla")]
    [SerializeField] private float opacidadMaxima = 0.5f;
    [SerializeField] private float velocidadTransicion = 4f;

    [Header("Vibración de Espanto")]
    [SerializeField] private float intensidadVibracion = 0.045f;
    [SerializeField] private float frecuenciaVibracion = 38f;

    private float nivelExposicion = 0f;
    private Vector3 offsetVibracionAnterior = Vector3.zero;

    private void Start()
    {
        if (camaraJugador == null && Camera.main != null)
            camaraJugador = Camera.main.transform;

        if (filtroPantallaMorada != null)
        {
            Color c = filtroPantallaMorada.color;
            c.a = 0f;
            filtroPantallaMorada.color = c;
            filtroPantallaMorada.gameObject.SetActive(false);
        }
    }

    private void LateUpdate()
    {
        // Quita el desplazamiento del frame anterior para no interferir con PlayerController ni Ventila
        if (camaraJugador != null)
        {
            camaraJugador.localPosition -= offsetVibracionAnterior;
            offsetVibracionAnterior = Vector3.zero;
        }

        bool bajoLaLuz = ComprobarSiEstaBajoLuz();

        float objetivo = bajoLaLuz ? 1f : 0f;
        nivelExposicion = Mathf.MoveTowards(nivelExposicion, objetivo, Time.deltaTime * velocidadTransicion);

        // 1. Actualiza opacidad morada en pantalla
        if (filtroPantallaMorada != null)
        {
            Color c = filtroPantallaMorada.color;
            c.a = nivelExposicion * opacidadMaxima;
            filtroPantallaMorada.color = c;
            filtroPantallaMorada.gameObject.SetActive(c.a > 0.005f);
        }

        // 2. Aplica vibración de espanto proporcional a qué tanto te da la luz
        if (camaraJugador != null && nivelExposicion > 0.01f)
        {
            float fuerza = intensidadVibracion * nivelExposicion;
            float shakeX = (Mathf.PerlinNoise(Time.time * frecuenciaVibracion, 0f) - 0.5f) * 2f * fuerza;
            float shakeY = (Mathf.PerlinNoise(0f, Time.time * frecuenciaVibracion) - 0.5f) * 2f * fuerza;

            offsetVibracionAnterior = new Vector3(shakeX, shakeY, 0f);
            camaraJugador.localPosition += offsetVibracionAnterior;
        }
    }

    private bool ComprobarSiEstaBajoLuz()
    {
        if (luzGuardia == null) return false;

        Vector3 origenLuz = luzGuardia.position;
        Vector3 destinoJugador = transform.position + Vector3.up * 0.8f;
        Vector3 direccion = destinoJugador - origenLuz;
        float distancia = direccion.magnitude;

        if (distancia > rangoMaximoLuz) return false;

        float angulo = Vector3.Angle(luzGuardia.forward, direccion.normalized);
        if (angulo > anguloConoLuz) return false;

        // Comprueba que no haya una pared o escondite bloqueando la luz
        if (Physics.Raycast(origenLuz, direccion.normalized, out RaycastHit hit, distancia, capasObstaculos))
        {
            if (!hit.transform.IsChildOf(transform) && hit.transform != transform)
                return false;
        }

        return true;
    }
}