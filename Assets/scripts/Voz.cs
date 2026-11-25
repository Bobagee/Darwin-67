using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;


public class Voz : MonoBehaviour
{
    [System.Serializable]
    public class Linea
    {
        public AudioClip audio;
        [TextArea(1, 3)] public string subtitulo;
        [Tooltip("Desmarcado: la linea solo suena una vez por partida")]
        public bool repetible;
    }

    [Header("Salida")]
  
    [SerializeField] private AudioSource fuente;

    [SerializeField] private GameObject panelSubtitulos;
    [SerializeField] private TMP_Text textoSubtitulos;

    [Header("Todas las lineas (audio + subtitulo)")]
    [SerializeField] private List<Linea> lineas = new List<Linea>();

    [Header("Cuando falla la trampa: Aleks, en orden")]
   
    [SerializeField] private AudioClip[] reaccionesAleksAlFallar;

    [Header("Cuando falla la trampa: burlas de Darwin (al azar)")]
    [SerializeField] private AudioClip[] burlasDarwin;

    [Header("Cola")]
   
    [SerializeField] private int maxEnEspera = 2;

    private const string ClaveSubtitulos = "opcion_subtitulos"; // la misma que usa OpcionSubtitulos

    private readonly Dictionary<AudioClip, Linea> porAudio = new Dictionary<AudioClip, Linea>();
    private readonly HashSet<AudioClip> yaDichas = new HashSet<AudioClip>();
    private readonly List<AudioClip> cola = new List<AudioClip>();
    private bool reproduciendo;
    private int indiceFallo;
    private int ultimaBurla = -1;

    private void Awake()
    {
        foreach (Linea l in lineas)
            if (l.audio != null) porAudio[l.audio] = l;

        if (panelSubtitulos != null) panelSubtitulos.SetActive(false);
    }

   
    public void Decir(AudioClip audio)
    {
        if (audio == null) return;
        if (cola.Contains(audio)) return;
        if (porAudio.TryGetValue(audio, out Linea l) && !l.repetible && yaDichas.Contains(audio)) return;

        cola.Add(audio);
        while (cola.Count > maxEnEspera + (reproduciendo ? 0 : 1))
            cola.RemoveAt(0);

        if (!reproduciendo) StartCoroutine(Reproducir());
    }

   
    public void DecirReaccionAleksAlFallar()
    {
        if (reaccionesAleksAlFallar == null || reaccionesAleksAlFallar.Length == 0) return;

        int i = Mathf.Min(indiceFallo, reaccionesAleksAlFallar.Length - 1);
        indiceFallo++;
        Decir(reaccionesAleksAlFallar[i]);
    }

   
    public void DecirBurlaDarwin()
    {
        if (burlasDarwin == null || burlasDarwin.Length == 0) return;

        int i = Random.Range(0, burlasDarwin.Length);
        if (burlasDarwin.Length > 1 && i == ultimaBurla) i = (i + 1) % burlasDarwin.Length;
        ultimaBurla = i;
        Decir(burlasDarwin[i]);
    }

    private IEnumerator Reproducir()
    {
        reproduciendo = true;

        while (cola.Count > 0)
        {
            AudioClip audio = cola[0];
            cola.RemoveAt(0);
            yaDichas.Add(audio);

            if (fuente != null)
            {
                fuente.clip = audio;
                fuente.Play();
            }

            porAudio.TryGetValue(audio, out Linea l);
            bool verTexto = l != null && !string.IsNullOrEmpty(l.subtitulo)
                            && PlayerPrefs.GetInt(ClaveSubtitulos, 1) == 1;

            if (verTexto && textoSubtitulos != null)
            {
                textoSubtitulos.text = l.subtitulo;
                if (panelSubtitulos != null) panelSubtitulos.SetActive(true);
            }

            yield return new WaitForSeconds(audio.length);

            if (panelSubtitulos != null) panelSubtitulos.SetActive(false);
        }

        reproduciendo = false;
    }
}