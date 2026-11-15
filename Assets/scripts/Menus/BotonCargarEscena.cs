using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;



public class BotonCargarEscena : MonoBehaviour
{
    [SerializeField] private string nombreEscena;

    private Button boton;

    private void Awake()
    {
        boton = GetComponent<Button>();
        boton.onClick.AddListener(CargarEscena); 
        //Evitar el proceso de hacer el proceso de hacer el + en el inspector :P
    }

    public void CargarEscena()
    {
        Time.timeScale = 1f;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        SceneManager.LoadScene(nombreEscena);
    }
}