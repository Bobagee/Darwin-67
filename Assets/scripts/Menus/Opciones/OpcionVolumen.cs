using UnityEngine;
using UnityEngine.UI;


public class OpcionVolumen : MonoBehaviour
{
    private const string Clave = "opcion_volumen";

    // Guardar el volumen sin necesidad de abrir Opciones
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void AplicarAlIniciar()
    {
        AudioListener.volume = PlayerPrefs.GetFloat(Clave, 1f);
    }

    private void Start()
    {
        Slider slider = GetComponent<Slider>();
        slider.value = PlayerPrefs.GetFloat(Clave, 1f);
        slider.onValueChanged.AddListener(Cambiar);
    }

    private void Cambiar(float volumen)
    {
        AudioListener.volume = volumen;
        PlayerPrefs.SetFloat(Clave, volumen);
    }
}