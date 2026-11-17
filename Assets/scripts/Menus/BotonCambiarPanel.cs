using UnityEngine;
using UnityEngine.UI;



public class BotonCambiarPanel : MonoBehaviour
{
    
    [SerializeField] private GameObject panelAMostrar;
   
    [SerializeField] private GameObject panelAOcultar;

    private Button boton;

    private void Awake()
    {
        boton = GetComponent<Button>();
        boton.onClick.AddListener(CambiarPanel); //Igual que en load scene, ahorra tiempo
    }


    public void CambiarPanel()
    {
        if (panelAOcultar != null) panelAOcultar.SetActive(false);
        if (panelAMostrar != null) panelAMostrar.SetActive(true);
    }
}