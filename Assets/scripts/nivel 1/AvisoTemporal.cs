using System.Collections;
using UnityEngine;

/// <summary>
/// Pon este script en cualquier texto o panel de la UI: aparece con un fundido, se queda unos
/// segundos y se apaga solo. Sirve para avisos como "Presiona [TAB] para ver tu lista".
/// Agrega el CanvasGroup por si solo.
/// </summary>
[RequireComponent(typeof(CanvasGroup))]
public class AvisoTemporal : MonoBehaviour
{
    [SerializeField] private float esperar = 0f;

    [SerializeField] private float visible = 10f;

    [SerializeField] private float fundido = 1f;

    private IEnumerator Start()
    {
        CanvasGroup grupo = GetComponent<CanvasGroup>();
        grupo.alpha = 0f;

        yield return new WaitForSeconds(esperar);
        yield return Fundir(grupo, 0f, 1f);
        yield return new WaitForSeconds(visible);
        yield return Fundir(grupo, 1f, 0f);

        gameObject.SetActive(false);
    }

    private IEnumerator Fundir(CanvasGroup grupo, float desde, float hasta)
    {
        float t = 0f;
        while (t < fundido)
        {
            t += Time.deltaTime;
            grupo.alpha = Mathf.Lerp(desde, hasta, t / fundido);
            yield return null;
        }
        grupo.alpha = hasta;
    }
}
