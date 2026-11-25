using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Toggle))]
public class OpcionVoces : MonoBehaviour
{
    private const string Clave = "opcion_voces";

    private void Start()
    {
        Toggle toggle = GetComponent<Toggle>();
        toggle.isOn = PlayerPrefs.GetInt(Clave, 1) == 1;
        toggle.onValueChanged.AddListener(Cambiar);
    }

    private void Cambiar(bool activa)
    {
        PlayerPrefs.SetInt(Clave, activa ? 1 : 0);

        Voz[] voces = FindObjectsByType<Voz>(
            FindObjectsInactive.Include,
            FindObjectsSortMode.None);

        foreach (Voz voz in voces)
            voz.ActivarVoces(activa);
    }
}
