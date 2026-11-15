using UnityEngine;
using UnityEngine.EventSystems;

[RequireComponent(typeof(CanvasGroup))]
public class PiezaArrastrableUI : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [Header("Identificación")]
    [SerializeField] private string itemID; // "Palo", "Hilo", "Caja", "Queso"

    public string ItemID => itemID;

    private RectTransform rectTransform;
    private RectTransform padreRect;
    private CanvasGroup canvasGroup;
    private Vector3 posicionInicial;
    private bool colocadaConExito;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        padreRect = rectTransform.parent as RectTransform;
        canvasGroup = GetComponent<CanvasGroup>();
        posicionInicial = rectTransform.localPosition;
    }

    public void ReiniciarPieza()
    {
        colocadaConExito = false;
        rectTransform.localPosition = posicionInicial;
        canvasGroup.blocksRaycasts = true;
        canvasGroup.alpha = 1f;
        gameObject.SetActive(true);
    }

    public void MarcarComoUsada()
    {
        colocadaConExito = true;
        gameObject.SetActive(false);
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        colocadaConExito = false;
        canvasGroup.blocksRaycasts = false;
        canvasGroup.alpha = 0.8f;
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (padreRect == null) return;

        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
            padreRect,
            eventData.position,
            eventData.pressEventCamera,
            out Vector2 puntoLocal))
        {
            rectTransform.localPosition = new Vector3(puntoLocal.x, puntoLocal.y, 0f);
        }
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        canvasGroup.blocksRaycasts = true;
        canvasGroup.alpha = 1f;

        if (!colocadaConExito)
        {
            rectTransform.localPosition = posicionInicial;
        }
    }
}