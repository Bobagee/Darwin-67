using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Toggle))]
public class OpcionSubtitulos : MonoBehaviour
{
    private const string Clave = "opcion_subtitulos";

    private void Start()
    {
        Toggle toggle = GetComponent<Toggle>();
        toggle.isOn = PlayerPrefs.GetInt(Clave, 1) == 1;
        toggle.onValueChanged.AddListener(Cambiar);
    }

    private void Cambiar(bool activa)
    {
        PlayerPrefs.SetInt(Clave, activa ? 1 : 0);
    }
}