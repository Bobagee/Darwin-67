using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// Pon este script en el Toggle de la musica.
/// Silencia todos los AudioSource cuyo objeto se llame "Musica..." (por ejemplo MusicaFondo),
/// en cualquier escena, y recuerda la opcion aunque cierres el juego.
/// </summary>
[RequireComponent(typeof(Toggle))]
public class OpcionMusica : MonoBehaviour
{
    private const string Clave = "opcion_musica";
    private const string PrefijoNombre = "Musica";
    
 //Guardar preferencias
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void AplicarAlIniciar()
    {
        Aplicar();
        SceneManager.sceneLoaded += (escena, modo) => Aplicar();
    }

    private static void Aplicar()
    {
        bool activa = PlayerPrefs.GetInt(Clave, 1) == 1;

        AudioSource[] fuentes = FindObjectsByType<AudioSource>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        foreach (AudioSource fuente in fuentes)
        {
            if (fuente.gameObject.name.StartsWith(PrefijoNombre)) fuente.mute = !activa;
        }
    }

    private void Start()
    {
        Toggle toggle = GetComponent<Toggle>();
        toggle.isOn = PlayerPrefs.GetInt(Clave, 1) == 1;
        toggle.onValueChanged.AddListener(Cambiar);
    }

    private void Cambiar(bool activa)
    {
        PlayerPrefs.SetInt(Clave, activa ? 1 : 0);
        Aplicar();
    }
}