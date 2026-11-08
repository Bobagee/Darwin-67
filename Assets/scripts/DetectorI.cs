using UnityEngine;

public class DetectorI : MonoBehaviour
{
    [Header("Configuración de Raycast")]
    [SerializeField] private Camera camaraJugador;
    [SerializeField] private float distanciaInteraccion = 2.5f;
    [SerializeField] private LayerMask capaInteractuable = ~0;

    [Header("Controles")]
    [SerializeField] private KeyCode teclaInteraccion = KeyCode.E;

    private Iteraccion objetoActual;

    private void Update()
    {
        BuscarObjeto();

        if (Input.GetKeyDown(teclaInteraccion) && objetoActual != null)
        {
            objetoActual.Interactuar();
        }
    }

    private void BuscarObjeto()
    {
        if (camaraJugador == null) camaraJugador = Camera.main;

        Ray ray = new Ray(camaraJugador.transform.position, camaraJugador.transform.forward);

        if (Physics.Raycast(ray, out RaycastHit hit, distanciaInteraccion, capaInteractuable))
        {
            Iteraccion interactable = hit.collider.GetComponentInParent<Iteraccion>();

            if (interactable != null)
            {
                objetoActual = interactable;
                return;
            }
        }

        objetoActual = null;
    }
}