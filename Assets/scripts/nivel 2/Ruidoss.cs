using UnityEngine;
using System;

public class Ruidoss : MonoBehaviour
{
    [Header("Configuración de Ruido")]
    [SerializeField] private AudioSource noiseAudio;
    [SerializeField] private float cooldownTime = 2.0f;

    public static event Action<Vector3> OnNoiseGenerated;

    private float lastNoiseTime;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && Time.time >= lastNoiseTime + cooldownTime)
        {
            lastNoiseTime = Time.time;
            if (noiseAudio != null) noiseAudio.Play();

            OnNoiseGenerated?.Invoke(transform.position);
        }
    }
}