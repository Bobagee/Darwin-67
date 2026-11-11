using UnityEngine;
using UnityEngine.Events;

public class PickUpItem : Iteraccion
{
    [Header("Identificación del Objeto")]
    [SerializeField] private ItemTipo tipoObjeto;

    [Header("Eventos")]
    [Tooltip("Evento al recoger. Regina puede conectar aquí la UI del Inventario")]
    public UnityEvent<ItemTipo> OnItemPickedUp;

    private Vector3 posicionInicial;
    private Quaternion rotacionInicial;
    private bool recolectado;

    public ItemTipo Tipo => tipoObjeto;
    public bool Recolectado => recolectado;

    private void Awake()
    {
        posicionInicial = transform.position;
        rotacionInicial = transform.rotation;
    }

    public override void Interactuar()
    {
        if (recolectado) return;

        recolectado = true;
        OnItemPickedUp?.Invoke(tipoObjeto);
        gameObject.SetActive(false);
    }

  
    public void Reaparecer()
    {
        transform.position = posicionInicial;
        transform.rotation = rotacionInicial;
        recolectado = false;
        gameObject.SetActive(true);
    }
}