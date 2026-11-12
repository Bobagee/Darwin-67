using UnityEngine;
using System;

public class TiempoBajoLuz : MonoBehaviour
{
    [Header("Parámetros de Detección")]
    [Tooltip("Segundos bajo el cono de luz antes de ser capturado")]
    [SerializeField] private float detectionTimeLimit = 2.0f;

    [Header("Referencias de Escena")]
    [SerializeField] private Transform playerTransform;
    [SerializeField] private Light uvSpotLight;
    [SerializeField] private Patrulla patrulla;

    [Header("Física y Coberturas")]
    [Tooltip("Capas de objetos que tapan la luz (ej. Paredes, Cajas)")]
    [SerializeField] private LayerMask obstacleMask = ~0; 

    public static event Action OnPlayerCaught;
    private float currentDetectionTimer = 0f;

    public float DetectionProgress => currentDetectionTimer / detectionTimeLimit;

    private void Update()
    {
        if (playerTransform == null || uvSpotLight == null) return;

        if (Escondite.IsPlayerHidden)
        {
            ResetTimer();
            return;
        }

        if (EstaBajoElHazDeLuz())
        {
            if (patrulla != null)
            {
                patrulla.DetenerPorDeteccion();
            }

            currentDetectionTimer += Time.deltaTime;

            if (currentDetectionTimer >= detectionTimeLimit)
            {
                OnPlayerCaught?.Invoke();
                ResetTimer();
            }
        }
        else
        {
            ResetTimer();
        }
    }

    private bool EstaBajoElHazDeLuz()
    {
        Vector3 origenLuz = uvSpotLight.transform.position;
        Vector3 destinoJugador = playerTransform.position;
        Vector3 direccionAHaciaJugador = destinoJugador - origenLuz;
        float distancia = direccionAHaciaJugador.magnitude;

        if (distancia > uvSpotLight.range) 
            return false;

        float anguloEntreLuzYJugador = Vector3.Angle(uvSpotLight.transform.forward, direccionAHaciaJugador.normalized);
        if (anguloEntreLuzYJugador > (uvSpotLight.spotAngle / 2f)) 
            return false;

        if (Physics.Raycast(origenLuz, direccionAHaciaJugador.normalized, out RaycastHit hit, distancia, obstacleMask))
        {
            if (hit.transform != playerTransform && !hit.transform.IsChildOf(playerTransform))
            {
                return false;
            }
        }

        return true;
    }

    private void ResetTimer()
    {
        currentDetectionTimer = Mathf.Max(0f, currentDetectionTimer - Time.deltaTime * 2f);
    }
}