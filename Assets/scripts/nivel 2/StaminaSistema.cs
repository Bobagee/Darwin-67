using UnityEngine;
using UnityEngine.UI;

public class StaminaSistema : MonoBehaviour
{
    [Header("Valores de Estamina")]
    [SerializeField] private float staminaMaxima = 100f;
    [SerializeField] private float gastoPorSegundo = 28f;
    [SerializeField] private float regeneracionPorSegundo = 18f;
    [SerializeField] private float retrasoRegeneracion = 1.0f;
    [Tooltip("Porcentaje mínimo (0 a 1) que debe recuperar tras agotarse para volver a correr y mover rápido el ratón")]
    [Range(0.1f, 1f)]
    [SerializeField] private float umbralRecuperacionAgotado = 0.35f;

    [Header("Penalización al Agotarse (Ratón Lento)")]
    [SerializeField] private PlayerController playerController;
    [Tooltip("Multiplicador de sensibilidad del ratón al estar agotado (ej. 0.25 = 25% de velocidad)")]
    [Range(0.05f, 1f)]
    [SerializeField] private float multiplicadorRatonAgotado = 0.25f;
    [Tooltip("Multiplicador de velocidad al caminar agotado (ej. 0.55 = 55% de velocidad)")]
    [Range(0.2f, 1f)]
    [SerializeField] private float multiplicadorCaminarAgotado = 0.55f;

    [Header("Barra UI Sencilla")]
    [SerializeField] private Image barraRelleno;
    [SerializeField] private Color colorNormal = new Color(0.2f, 0.8f, 1f, 1f);
    [SerializeField] private Color colorAgotado = new Color(0.95f, 0.25f, 0.25f, 1f);
    [SerializeField] private bool ocultarCuandoEstaLlena = true;

    private float timerSinCorrer;
    private bool estaAgotado = false;

    public float CurrentStamina { get; private set; }
    public bool EstaAgotado => estaAgotado;

    private void Start()
    {
        CurrentStamina = staminaMaxima;

        if (playerController == null)
            playerController = FindFirstObjectByType<PlayerController>();

        ActualizarBarraUI();
    }

    private void Update()
    {
        // Coincide exactamente con la lógica de correr hacia adelante de PlayerController
        bool intentandoCorrer = Input.GetKey(KeyCode.LeftShift) && Input.GetAxis("Vertical") > 0.1f;

        if (intentandoCorrer && CurrentStamina > 0f && !estaAgotado)
        {
            CurrentStamina -= gastoPorSegundo * Time.deltaTime;
            timerSinCorrer = 0f;

            if (CurrentStamina <= 0f)
            {
                CurrentStamina = 0f;
                ActivarAgotamiento(true);
            }
        }
        else
        {
            timerSinCorrer += Time.deltaTime;
            if (timerSinCorrer >= retrasoRegeneracion && CurrentStamina < staminaMaxima)
            {
                CurrentStamina += regeneracionPorSegundo * Time.deltaTime;
                if (CurrentStamina > staminaMaxima)
                    CurrentStamina = staminaMaxima;

                if (estaAgotado && (CurrentStamina / staminaMaxima) >= umbralRecuperacionAgotado)
                {
                    ActivarAgotamiento(false);
                }
            }
        }

        ActualizarBarraUI();
    }

    private void ActivarAgotamiento(bool agotado)
    {
        estaAgotado = agotado;

        if (playerController != null)
        {
            if (agotado)
                playerController.SetMultiplicadorFatiga(multiplicadorCaminarAgotado, multiplicadorRatonAgotado);
            else
                playerController.SetMultiplicadorFatiga(1f, 1f);
        }
    }

   private void ActualizarBarraUI()
    {
        if (barraRelleno == null) return;

        float porcentaje = Mathf.Clamp01(CurrentStamina / staminaMaxima);
        barraRelleno.rectTransform.localScale = new Vector3(porcentaje, 1f, 1f);
        barraRelleno.color = estaAgotado ? colorAgotado : colorNormal;

        if (ocultarCuandoEstaLlena && barraRelleno.transform.parent != null)
        {
            barraRelleno.transform.parent.gameObject.SetActive(porcentaje < 0.999f);
        }
    }
}