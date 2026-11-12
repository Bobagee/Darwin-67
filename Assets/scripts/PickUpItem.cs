using UnityEngine;
using UnityEngine.Events;

public class PickUpItem : Iteraccion
{
    [Header("Identificación del Objeto")]
    [SerializeField] private ItemTipo tipoObjeto;
    [SerializeField] private bool reaparecerSiFallaTrampa = true;

[Header("Feedback Visual (Resplandor)")]
[SerializeField] private GameObject efectoResplandor;
[SerializeField] private Renderer meshRenderer;      
[SerializeField] private Color colorBrillo = new Color(1f, 0.9f, 0.4f, 1f);

    [Header("Eventos")]
    [Tooltip("Opcional: por si quieres conectar sonidos o partículas al recogerlo")]
    public UnityEvent<ItemTipo> OnItemPickedUp;

    private Vector3 posicionInicial;
    private Quaternion rotacionInicial;
    private bool recolectado;

public override void OnMirar(bool mirando)
{
    if (efectoResplandor != null)
        efectoResplandor.SetActive(mirando && !recolectado);

    if (meshRenderer != null)
    {
        if (mirando && !recolectado)
        {
            meshRenderer.material.EnableKeyword("_EMISSION");
            meshRenderer.material.SetColor("_EmissionColor", colorBrillo * 0.4f);
        }
        else
        {
            meshRenderer.material.SetColor("_EmissionColor", Color.black);
        }
    }
}

    public ItemTipo Tipo => tipoObjeto;
    public bool Recolectado => recolectado;

    private void Awake()
    {
        posicionInicial = transform.position;
        rotacionInicial = transform.rotation;

        if (reaparecerSiFallaTrampa)
        {
            Escape.OnTrapFailed += Reaparecer;
        }
    }

    private void OnDestroy()
    {
        if (reaparecerSiFallaTrampa)
        {
            Escape.OnTrapFailed -= Reaparecer;
        }
    }

    public override void Interactuar()
    {
        if (recolectado) return;

        recolectado = true;

        if (InventarioUI.Instance != null)
        {
            InventarioUI.Instance.RegistrarObjeto(tipoObjeto.ToString());
        }

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