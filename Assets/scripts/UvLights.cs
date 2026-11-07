using System;
using UnityEngine;

public class UVLight : MonoBehaviour
{
    [Header("Configuración de Luz UV")]
    [SerializeField] private Light uvSpotlight;
    [SerializeField] private float maxBattery = 100f;
    [SerializeField] private float drainRate = 5f;
    [SerializeField] private KeyCode toggleKey = KeyCode.F;

    public static event Action<bool> OnUVToggle;

    private float currentBattery;
    private bool isOn;

    public bool IsOn => isOn;
    public float BatteryPercent => currentBattery / maxBattery;
    public Light Spotlight => uvSpotlight;

    private void Start()
    {
        currentBattery = maxBattery;
        if (uvSpotlight != null) uvSpotlight.enabled = false;
    }

    private void Update()
    {
        if (Input.GetKeyDown(toggleKey) && currentBattery > 0)
        {
            isOn = !isOn;
            if (uvSpotlight != null) uvSpotlight.enabled = isOn;
            OnUVToggle?.Invoke(isOn);
        }

        if (isOn)
        {
            currentBattery -= drainRate * Time.deltaTime;
            if (currentBattery <= 0f)
            {
                currentBattery = 0f;
                isOn = false;
                if (uvSpotlight != null) uvSpotlight.enabled = false;
                OnUVToggle?.Invoke(false);
            }
        }
    }
}