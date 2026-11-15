using UnityEngine;
using UnityEngine.Events;

public class PickUpItem : Iteraccion
{
    [Header("Identificación del Objeto")]
    [SerializeField] private ItemTipo tipoObjeto;
    [SerializeField] private bool reaparecerSiFallaTrampa = true;

    [Header("Resplandor al Mirar")]
    [SerializeField] private Color colorBrillo = new Color(1f, 0.92f, 0.45f, 1f);
    [SerializeField] private float intensidadBrillo = 1.5f;
    [SerializeField] private GameObject luzOContornoOpcional;

    [Header("Eventos")]
    public UnityEvent<ItemTipo> OnItemPickedUp;

    private Vector3 posicionInicial;
    private Quaternion rotacionInicial;
    private bool recolectado;

    private Renderer[] renderers;
    private Color[] coloresOriginales;

    public ItemTipo Tipo => tipoObjeto;
    public bool Recolectado => recolectado;

    private void Awake()
    {
        posicionInicial = transform.position;
        rotacionInicial = transform.rotation;

        renderers = GetComponentsInChildren<Renderer>();
        coloresOriginales = new Color[renderers.Length];

        for (int i = 0; i < renderers.Length; i++)
        {
            if (renderers[i].material.HasProperty("_BaseColor"))
                coloresOriginales[i] = renderers[i].material.GetColor("_BaseColor");
            else if (renderers[i].material.HasProperty("_Color"))
                coloresOriginales[i] = renderers[i].material.color;
        }

        if (luzOContornoOpcional != null)
            luzOContornoOpcional.SetActive(false);

        if (reaparecerSiFallaTrampa)
            Escape.OnTrapFailed += Reaparecer;
    }

    private void OnDestroy()
    {
        if (reaparecerSiFallaTrampa)
            Escape.OnTrapFailed -= Reaparecer;
    }

    public override void OnMirar(bool mirando)
    {
        bool activar = mirando && !recolectado;

        if (luzOContornoOpcional != null)
            luzOContornoOpcional.SetActive(activar);

        for (int i = 0; i < renderers.Length; i++)
        {
            Material mat = renderers[i].material;
            if (activar)
            {
                mat.EnableKeyword("_EMISSION");
                mat.SetColor("_EmissionColor", colorBrillo * intensidadBrillo);

                Color resaltado = Color.Lerp(coloresOriginales[i], colorBrillo, 0.45f);
                if (mat.HasProperty("_BaseColor")) mat.SetColor("_BaseColor", resaltado);
                if (mat.HasProperty("_Color")) mat.color = resaltado;
            }
            else
            {
                mat.SetColor("_EmissionColor", Color.black);
                if (mat.HasProperty("_BaseColor")) mat.SetColor("_BaseColor", coloresOriginales[i]);
                if (mat.HasProperty("_Color")) mat.color = coloresOriginales[i];
            }
        }
    }

    public override void Interactuar()
    {
        if (recolectado) return;

        OnMirar(false);
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
        OnMirar(false);
        gameObject.SetActive(true);
    }
}