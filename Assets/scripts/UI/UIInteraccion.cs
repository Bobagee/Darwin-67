using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIInteraccion : MonoBehaviour
{
    [Header("Referencias")]
    [SerializeField] private DetectorI detector;
    [SerializeField] private Image mira;
    [SerializeField] private TMP_Text textoPrompt;

    [Header("Mira")]
    [SerializeField] private Color colorNormal = new Color(1f, 1f, 1f, 0.6f);
    [SerializeField] private Color colorResaltado = new Color(1f, 0.92f, 0.45f, 1f);

    [SerializeField] private bool ocultarConCursorLibre = true;
    public static string TextoExtra = "";
    private void Start()
    {
        if (detector == null) detector = FindFirstObjectByType<DetectorI>();
        if (textoPrompt != null) textoPrompt.gameObject.SetActive(false);
    }

    private void LateUpdate()
    {
        bool cursorLibre = ocultarConCursorLibre && Cursor.lockState != CursorLockMode.Locked;

        string texto = "";
        if (!cursorLibre && detector != null && detector.ObjetoActual != null)
            texto = detector.ObjetoActual.TextoPrompt;
        if (string.IsNullOrEmpty(texto) && !cursorLibre)
            texto = TextoExtra;
        TextoExtra = "";

        bool hayTexto = !string.IsNullOrEmpty(texto);

        if (mira != null)
        {
            mira.enabled = !cursorLibre;
            mira.color = hayTexto ? colorResaltado : colorNormal;
        }

        if (textoPrompt != null)
        {
            if (hayTexto && textoPrompt.text != texto) textoPrompt.text = texto;
            if (textoPrompt.gameObject.activeSelf != hayTexto) textoPrompt.gameObject.SetActive(hayTexto);
        }
    }
}