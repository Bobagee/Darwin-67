using UnityEngine;
using UnityEngine.UI;

public class VisorGrabadora : MonoBehaviour
{
    public static VisorGrabadora Instancia { get; private set; }

    [Header("Interfaz de Grabadora")]
    [SerializeField] private GameObject panelGrabadora;
    [SerializeField] private Image imagenGrabadoraYHoja;

    [Header("Audio")]
    [SerializeField] private AudioSource audioSource;

    [Header("Opciones")]
    [SerializeField] private KeyCode teclaCerrar = KeyCode.Escape;
    [SerializeField] private bool permitirMoverMientrasEscucha = true;
    [SerializeField] private MonoBehaviour scriptControlJugador;

    private int frameApertura = -1;

    public bool EstaAbierto => panelGrabadora != null && panelGrabadora.activeSelf;

    private void Awake()
    {
        Instancia = this;
        if (panelGrabadora != null)
            panelGrabadora.SetActive(false);
    }

    private void Update()
    {
        if (!EstaAbierto || Time.frameCount == frameApertura) return;

        if (Input.GetKeyDown(KeyCode.E) || Input.GetKeyDown(teclaCerrar))
        {
            CerrarGrabadora();
        }
    }

    public void ReproducirGrabadora(Sprite dibujoGrabadora, AudioClip clip)
    {
        if (panelGrabadora == null) return;

        frameApertura = Time.frameCount;
        panelGrabadora.SetActive(true);

        if (imagenGrabadoraYHoja != null && dibujoGrabadora != null)
        {
            imagenGrabadoraYHoja.sprite = dibujoGrabadora;
            imagenGrabadoraYHoja.preserveAspect = true;
        }

        if (audioSource != null)
        {
            audioSource.Stop();
            if (clip != null)
            {
                audioSource.clip = clip;
                audioSource.Play();
            }
        }

        if (!permitirMoverMientrasEscucha)
        {
            if (scriptControlJugador != null) scriptControlJugador.enabled = false;
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }

    public void CerrarGrabadora()
    {
        if (panelGrabadora == null) return;

        panelGrabadora.SetActive(false);

        if (audioSource != null && audioSource.isPlaying)
            audioSource.Stop();

        if (!permitirMoverMientrasEscucha)
        {
            if (scriptControlJugador != null) scriptControlJugador.enabled = true;
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }
}