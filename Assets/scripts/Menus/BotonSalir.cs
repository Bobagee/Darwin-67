using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class BotonSalir : MonoBehaviour
{
    private Button boton;

    private void Awake()
    {
        boton = GetComponent<Button>();
        boton.onClick.AddListener(Salir);
    }

  
    public void Salir()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif

    }
}