using System.Collections;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Decide el resultado de jalar la trampa.
/// - Si el raton esta en la trampa (ventana de captura abierta): avisa para pasar de nivel.
/// - Si no: el raton huye, se suelta un queso extra, aparece el dibujo de burla, suena la burla
///   y la busqueda continua.
/// </summary>
public class CatchResult : MonoBehaviour
{
    [Header("Referencias")]
    [SerializeField] private MouseBaitAI raton;

    [Header("Si atrapas al raton")]
    [Tooltip("Conecta aqui el metodo del GameManager que pasa al siguiente nivel")]
    public UnityEvent alAtraparRaton;
    /// <summary>Alternativa por codigo: el GameManager puede suscribirse (y des-suscribirse en OnDisable).</summary>
    public static event System.Action OnRatonAtrapado;

    [Header("Si falla: queso extra")]
    [Tooltip("Prefab con PickUpItem de tipo Queso y un Collider")]
    [SerializeField] private GameObject quesoExtraPrefab;
    [SerializeField] private float alturaQueso = 0.3f;
    [Tooltip("Se dispara al soltar el queso, por si el inventario necesita escuchar su PickUpItem")]
    public UnityEvent<PickUpItem> alSoltarQueso;

    [Header("Si falla: dibujo de burla")]
    [Tooltip("Objeto (UI o 3D, distinto al que lleva este script) que se muestra unos segundos")]
    [SerializeField] private GameObject dibujoBurla;
    [SerializeField] private float duracionDibujo = 3f;

    [Header("Si falla: audio de burla")]
    [SerializeField] private AudioSource fuenteAudio;
    [SerializeField] private AudioClip sonidoBurla;

    private Coroutine rutinaDibujo;

    private void Start()
    {
        if (dibujoBurla != null) dibujoBurla.SetActive(false);
    }

    /// <summary>
    /// Devuelve true si atrapo al raton. puntoTrampa es donde esta la trampa (para soltar el queso).
    /// </summary>
    public bool Evaluar(Vector3 puntoTrampa)
    {
        bool atrapado = raton != null && raton.EnVentanaDeCaptura;

        if (atrapado) Ganar();
        else Fallar(puntoTrampa);

        return atrapado;
    }

    private void Ganar()
    {
        raton.Capturar();
        alAtraparRaton?.Invoke();
        OnRatonAtrapado?.Invoke();
    }

    private void Fallar(Vector3 puntoTrampa)
    {
        if (raton != null) raton.Huir();

        SoltarQueso(puntoTrampa);
        MostrarDibujo();
        ReproducirBurla(puntoTrampa);
    }

    private void SoltarQueso(Vector3 punto)
    {
        if (quesoExtraPrefab == null) return;

        GameObject queso = Instantiate(quesoExtraPrefab, punto + Vector3.up * alturaQueso, Quaternion.identity);
        PickUpItem item = queso.GetComponentInChildren<PickUpItem>();
        if (item != null) alSoltarQueso?.Invoke(item);
    }

    private void MostrarDibujo()
    {
        if (dibujoBurla == null) return;

        if (rutinaDibujo != null) StopCoroutine(rutinaDibujo);
        rutinaDibujo = StartCoroutine(RutinaDibujo());
    }

    private IEnumerator RutinaDibujo()
    {
        dibujoBurla.SetActive(true);
        yield return new WaitForSeconds(duracionDibujo);
        dibujoBurla.SetActive(false);
        rutinaDibujo = null;
    }

    private void ReproducirBurla(Vector3 punto)
    {
        if (sonidoBurla == null) return;

        if (fuenteAudio != null) fuenteAudio.PlayOneShot(sonidoBurla);
        else AudioSource.PlayClipAtPoint(sonidoBurla, punto);
    }
}