using System;
using UnityEngine;

public class UvLights : MonoBehaviour
{
    [Header("Referencias del Inspector")]
    [Tooltip("El componente Light de tipo Spot")]
    [SerializeField] private Light uvSpotlight;
    
    [Tooltip("El objeto 3D que tiene el script Conito adjunto")]
    [SerializeField] private GameObject visualConeMesh;

    [Header("Controles")]
    [Tooltip("Tecla para encender/apagar la luz")]
    [SerializeField] private KeyCode toggleKey = KeyCode.F;

    public static event Action<bool> OnUVToggle;

    private bool isOn;

    public bool IsOn => isOn;
    public Light Spotlight => uvSpotlight;

    private void Start()
    {
        SetLightState(false);
    }

    private void Update()
    {
        if (Input.GetKeyDown(toggleKey))
        {
            isOn = !isOn;
            SetLightState(isOn);
            OnUVToggle?.Invoke(isOn);
        }
    }

    private void SetLightState(bool active)
    {
        if (uvSpotlight != null) uvSpotlight.enabled = active;
        if (visualConeMesh != null) visualConeMesh.SetActive(active);
    }
}