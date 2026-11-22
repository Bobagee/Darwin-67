using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EfectoBajoLuz : MonoBehaviour
{
    [Header("Temblor del Ratón (Arrastra el hijo visual/sprite, NO el objeto raíz)")]
    [SerializeField] private Transform visualRaton;
    [SerializeField] private float intensidadTemblor = 0.04f;
    [SerializeField] private float velocidadTemblor = 50f;

    [Header("Pantalla Morada en Canvas")]
    [SerializeField] private Image pantallaMorada;
    [Range(0f, 1f)]
    [SerializeField] private float opacidadMaxima = 0.35f;
    [SerializeField] private float velocidadTransicion = 6f;

    [Header("Audio (Listo para poner el clip después)")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip clipChillido;
    [SerializeField] private float intervaloChillido = 2.5f;

    [Header("Detección")]
    [SerializeField] private string tagZonaLuz = "Luz";

    private readonly HashSet<Collider> lucesActivas = new HashSet<Collider>();
    private Vector3 posicionLocalInicial;
    private float alphaActual = 0f;
    private float timerChillido = 0f;
    private bool forzadoExterno = false;

    public bool EstaBajoLuz => lucesActivas.Count > 0 || forzadoExterno;

    private void Start()
    {
        if (visualRaton != null)
            posicionLocalInicial = visualRaton.localPosition;

        if (pantallaMorada != null)
        {
            Color c = pantallaMorada.color;
            c.a = 0f;
            pantallaMorada.color = c;
            pantallaMorada.raycastTarget = false;
        }
    }

    private void Update()
    {
        bool bajoLuz = EstaBajoLuz;

        if (visualRaton != null)
        {
            if (bajoLuz)
            {
                float offsetX = (Mathf.PerlinNoise(Time.time * velocidadTemblor, 0f) - 0.5f) * 2f * intensidadTemblor;
                float offsetY = (Mathf.PerlinNoise(0f, Time.time * velocidadTemblor) - 0.5f) * 2f * intensidadTemblor;
                visualRaton.localPosition = posicionLocalInicial + new Vector3(offsetX, offsetY, 0f);
            }
            else
            {
                visualRaton.localPosition = Vector3.Lerp(visualRaton.localPosition, posicionLocalInicial, Time.deltaTime * 15f);
            }
        }

        if (pantallaMorada != null)
        {
            float objetivo = bajoLuz ? opacidadMaxima : 0f;
            alphaActual = Mathf.MoveTowards(alphaActual, objetivo, Time.deltaTime * velocidadTransicion);
            Color c = pantallaMorada.color;
            c.a = alphaActual;
            pantallaMorada.color = c;
        }

        if (bajoLuz)
        {
            timerChillido -= Time.deltaTime;
            if (timerChillido <= 0f)
            {
                ReproducirChillido();
                timerChillido = intervaloChillido;
            }
        }
        else
        {
            timerChillido = 0f;
        }
    }

    private void ReproducirChillido()
    {
        if (audioSource != null && clipChillido != null)
        {
            audioSource.PlayOneShot(clipChillido);
        }
    }

    public void EstablecerBajoLuz(bool estado)
    {
        forzadoExterno = estado;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag(tagZonaLuz))
            lucesActivas.Add(other);
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag(tagZonaLuz))
            lucesActivas.Remove(other);
    }
}