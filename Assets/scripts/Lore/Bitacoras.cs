using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Bitacoras : MonoBehaviour
{
    public static Bitacoras Instancia { get; private set; }

    private class DatosArchivo
    {
        public Sprite imagen;
        public Color color;
    }

    [Header("Jugador (Bloquear cámara al leer)")]
    [SerializeField] private PlayerController playerController;

    [Header("Visor de Archivos")]
    [SerializeField] private GameObject panelBitacora;
    [SerializeField] private Image visorImagenArchivo;

    [Header("Botones de los 5 Archivos (Orden 1 al 5)")]
    [SerializeField] private Button[] botonesArchivos;

    [Header("Controles")]
    [SerializeField] private KeyCode teclaAbrirMenu = KeyCode.J;
    [SerializeField] private KeyCode teclaCerrar = KeyCode.Escape;

    // Vive solo en memoria durante la partida actual
    private readonly Dictionary<int, DatosArchivo> archivosDesbloqueados = new Dictionary<int, DatosArchivo>();
    private int archivoActual = -1;
    private int frameApertura = -1;

    public bool EstaAbierto => panelBitacora != null && panelBitacora.activeSelf;

    private void Awake()
    {
        Instancia = this;

        if (panelBitacora != null)
            panelBitacora.SetActive(false);

        ConfigurarBotones();
        ActualizarBotones();
    }

    private void Update()
    {
        if (Time.frameCount == frameApertura) return;

        if (EstaAbierto)
        {
            if (Input.GetKeyDown(teclaCerrar) || Input.GetKeyDown(teclaAbrirMenu))
            {
                CerrarPanel();
            }
        }
        else
        {
            if (Input.GetKeyDown(teclaAbrirMenu) && archivosDesbloqueados.Count > 0)
            {
                AbrirPanel();
                MostrarArchivo(archivoActual > 0 ? archivoActual : ObtenerPrimerDesbloqueado());
            }
        }
    }

    private void ConfigurarBotones()
    {
        if (botonesArchivos == null) return;

        for (int i = 0; i < botonesArchivos.Length; i++)
        {
            int numero = i + 1;
            if (botonesArchivos[i] != null)
            {
                botonesArchivos[i].onClick.AddListener(() => MostrarArchivo(numero));
            }
        }
    }

    public void RegistrarYAbrir(CarpetaLore carpeta)
    {
        if (carpeta == null) return;

        if (!archivosDesbloqueados.ContainsKey(carpeta.NumeroArchivo))
        {
            archivosDesbloqueados[carpeta.NumeroArchivo] = new DatosArchivo
            {
                imagen = carpeta.ImagenArchivo,
                color = carpeta.ColorGraybox
            };
        }

        ActualizarBotones();
        AbrirPanel();
        MostrarArchivo(carpeta.NumeroArchivo);
    }

    public void MostrarArchivo(int numero)
    {
        if (!archivosDesbloqueados.TryGetValue(numero, out DatosArchivo datos))
            return;

        archivoActual = numero;

        if (visorImagenArchivo != null)
        {
            visorImagenArchivo.sprite = datos.imagen;
            visorImagenArchivo.color = datos.color;
            visorImagenArchivo.preserveAspect = (datos.imagen != null);
        }
    }

    private int ObtenerPrimerDesbloqueado()
    {
        for (int i = 1; i <= 5; i++)
        {
            if (archivosDesbloqueados.ContainsKey(i)) return i;
        }
        return 1;
    }

    public void AbrirPanel()
    {
        if (panelBitacora == null) return;

        frameApertura = Time.frameCount;
        panelBitacora.SetActive(true);

        if (playerController != null) playerController.enabled = false;
        Time.timeScale = 0f;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void CerrarPanel()
    {
        if (panelBitacora == null) return;

        panelBitacora.SetActive(false);

        if (playerController != null) playerController.enabled = true;
        Time.timeScale = 1f;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void ActualizarBotones()
    {
        if (botonesArchivos == null) return;

        for (int i = 0; i < botonesArchivos.Length; i++)
        {
            int numero = i + 1;
            if (botonesArchivos[i] != null)
            {
                botonesArchivos[i].interactable = archivosDesbloqueados.ContainsKey(numero);
            }
        }
    }
    public void AbrirDesdeBoton()
{
    if (archivosDesbloqueados.Count == 0) return;
    AbrirPanel();
    MostrarArchivo(archivoActual > 0 ? archivoActual : ObtenerPrimerDesbloqueado());
}
}