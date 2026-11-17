using UnityEngine;
using UnityEngine.UI;


public class OpcionBrillo : MonoBehaviour
{
    private const string Clave = "opcion_brillo";
    private const float BrilloMinimo = 0.3f; // para que nunca quede todo negro

    private static Image capaOscura;

    //Guardar el brillo
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void AplicarAlIniciar()
    {
        Aplicar(PlayerPrefs.GetFloat(Clave, 1f));
    }

    private static void Aplicar(float brillo)
    {
        if (capaOscura == null) CrearCapa();
        capaOscura.color = new Color(0f, 0f, 0f, 1f - brillo);
    }

    private static void CrearCapa()
    {
        GameObject raiz = new GameObject("CapaBrillo");
        DontDestroyOnLoad(raiz);

        Canvas canvas = raiz.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 999; 

        GameObject imagen = new GameObject("Oscuro");
        imagen.transform.SetParent(raiz.transform, false);

        capaOscura = imagen.AddComponent<Image>();
        capaOscura.raycastTarget = false; // no bloquea los clics

        RectTransform rt = capaOscura.rectTransform;
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
    }

    private void Start()
    {
        Slider slider = GetComponent<Slider>();
        slider.minValue = BrilloMinimo;
        slider.maxValue = 1f;
        slider.value = PlayerPrefs.GetFloat(Clave, 1f);
        slider.onValueChanged.AddListener(Cambiar);
    }

    private void Cambiar(float brillo)
    {
        Aplicar(brillo);
        PlayerPrefs.SetFloat(Clave, brillo);
    }
}