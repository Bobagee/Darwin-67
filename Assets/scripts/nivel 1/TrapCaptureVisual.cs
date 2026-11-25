using UnityEngine;

public class TrapCaptureVisual : MonoBehaviour
{
    [SerializeField] private GameObject modeloArmado;
    [SerializeField] private GameObject modeloCerrado;

    public void MostrarArmada()
    {
        if (modeloArmado != null) modeloArmado.SetActive(true);
        if (modeloCerrado != null) modeloCerrado.SetActive(false);
    }

    public void MostrarCaptura()
    {
        if (modeloArmado != null) modeloArmado.SetActive(false);
        if (modeloCerrado != null) modeloCerrado.SetActive(true);
    }
}