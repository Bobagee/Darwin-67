using UnityEngine;

public class SonidoPatrulla : MonoBehaviour
{
    [Header("Audio 3D del Científico")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip[] clipsPasos;
    [SerializeField] private AudioClip[] clipsMurmullosPatrulla;

    [Header("Frecuencia")]
    [SerializeField] private float distanciaPorPaso = 1.8f;
    [SerializeField] private float intervaloMurmullo = 7f;

    private Vector3 ultimaPosicion;
    private float distanciaAcumulada;
    private float timerMurmullo;

    private void Start()
    {
        ultimaPosicion = transform.position;
        timerMurmullo = intervaloMurmullo;
    }

    private void Update()
    {
        float desplazado = Vector3.Distance(transform.position, ultimaPosicion);
        distanciaAcumulada += desplazado;
        ultimaPosicion = transform.position;

        if (distanciaAcumulada >= distanciaPorPaso)
        {
            distanciaAcumulada = 0f;
            ReproducirAleatorio(clipsPasos);
        }

        timerMurmullo -= Time.deltaTime;
        if (timerMurmullo <= 0f)
        {
            timerMurmullo = intervaloMurmullo;
            ReproducirAleatorio(clipsMurmullosPatrulla);
        }
    }

    private void ReproducirAleatorio(AudioClip[] lista)
    {
        if (audioSource == null || lista == null || lista.Length == 0) return;

        AudioClip elegido = lista[Random.Range(0, lista.Length)];
        if (elegido != null)
        {
            audioSource.PlayOneShot(elegido);
        }
    }
}