using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Video;

/// <summary>
/// Pon este script en el objeto que tiene el VideoPlayer de una escena de cinematica.
/// Cuando el video termina (o el jugador presiona la tecla de saltar) carga la siguiente escena.
/// </summary>
[RequireComponent(typeof(VideoPlayer))]
public class CinematicaAlTerminar : MonoBehaviour
{
    [SerializeField] private string siguienteEscena;

    [SerializeField] private KeyCode teclaSaltar = KeyCode.Space;

    [SerializeField] private float esperaParaSaltar = 1f;

    private VideoPlayer video;
    private bool yaCargo;
    private float tiempo;

    private void Awake()
    {
        video = GetComponent<VideoPlayer>();
        video.isLooping = false;
        video.loopPointReached += AlTerminar;
    }

    private void Start()
    {
        Time.timeScale = 1f;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = false;
    }

    private void OnDestroy()
    {
        if (video != null) video.loopPointReached -= AlTerminar;
    }

    private void Update()
    {
        tiempo += Time.deltaTime;
        if (tiempo >= esperaParaSaltar && Input.GetKeyDown(teclaSaltar))
            Ir();
    }

    private void AlTerminar(VideoPlayer vp)
    {
        Ir();
    }

    private void Ir()
    {
        if (yaCargo || string.IsNullOrEmpty(siguienteEscena)) return;
        yaCargo = true;
        SceneManager.LoadScene(siguienteEscena);
    }
}