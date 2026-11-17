using UnityEngine;
using UnityEngine.EventSystems;

public class PlanosUI : MonoBehaviour, IPointerClickHandler
{
    [Header("Tamaños del Plano (Editable en Inspector)")]
    [SerializeField] private Vector2 tamanoContraido = new Vector2(180f, 180f);
    [SerializeField] private Vector2 tamanoExpandido = new Vector2(650f, 650f);
    [SerializeField] private float velocidadAnimacion = 12f;

    private RectTransform rectTransform;
    private bool expandido = false;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        rectTransform.sizeDelta = tamanoContraido;
    }

    private void OnEnable()
    {
        expandido = false;
        if (rectTransform != null)
            rectTransform.sizeDelta = tamanoContraido;
    }

    private void Update()
    {
        Vector2 objetivo = expandido ? tamanoExpandido : tamanoContraido;
        rectTransform.sizeDelta = Vector2.Lerp(rectTransform.sizeDelta, objetivo, Time.unscaledDeltaTime * velocidadAnimacion);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        expandido = !expandido;
    }
}