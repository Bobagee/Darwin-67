using UnityEngine;
using UnityEngine.Events;

public class StaminaSistema : MonoBehaviour
{
    [Header("Configuración de Estamina")]
    [SerializeField] private float maxStamina = 100f;
    [SerializeField] private float staminaDrainRate = 25f;
    [SerializeField] private float staminaRegenRate = 35f;
    [SerializeField] private float sprintSpeedMultiplier = 1.6f;

    [Header("Eventos para UI")]
    public UnityEvent<float, float> OnStaminaChanged;

    private float currentStamina;
    private bool isSprinting;

    public float CurrentStamina => currentStamina;
    public float SprintMultiplier => isSprinting ? sprintSpeedMultiplier : 1.0f;

    private void Start()
    {
        currentStamina = maxStamina;
        OnStaminaChanged?.Invoke(currentStamina, maxStamina);
    }

    private void Update()
    {
        bool wantsToSprint = Input.GetKey(KeyCode.LeftShift) && Input.GetAxisRaw("Vertical") > 0;

        if (wantsToSprint && currentStamina > 0f)
        {
            isSprinting = true;
            currentStamina -= staminaDrainRate * Time.deltaTime;
            currentStamina = Mathf.Max(currentStamina, 0f);
        }
        else
        {
            isSprinting = false;
            if (currentStamina < maxStamina)
            {
                currentStamina += staminaRegenRate * Time.deltaTime;
                currentStamina = Mathf.Min(currentStamina, maxStamina);
            }
        }

        OnStaminaChanged?.Invoke(currentStamina, maxStamina);
    }
}