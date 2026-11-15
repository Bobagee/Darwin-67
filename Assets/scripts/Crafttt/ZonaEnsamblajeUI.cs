using UnityEngine;
using UnityEngine.EventSystems;

public class ZonaEnsamblajeUI : MonoBehaviour, IDropHandler
{
    [SerializeField] private sistemaCrafteo sistemaCraft;

    public void OnDrop(PointerEventData eventData)
    {
        if (eventData.pointerDrag == null || sistemaCraft == null) return;

        PiezaArrastrableUI pieza = eventData.pointerDrag.GetComponent<PiezaArrastrableUI>();
        if (pieza != null)
        {
            sistemaCraft.IntentarAgregarPieza(pieza);
        }
    }
}