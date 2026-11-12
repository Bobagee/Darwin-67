using UnityEngine;

public class FootprintZone : MonoBehaviour
{
    [Header("Referencias del Jugador y Luz")]
    [SerializeField] private UvLights playerUVLight;

    [Header("Zonas de Huellas (Arrastra los objetos aqui)")]
    [SerializeField] private GameObject[] zones;

    [Header("Tiempos y Velocidades (Inspector)")]
    [Tooltip("Segundos que dura activa una zona antes de cambiar a otra")]
    [SerializeField] private float zoneDuration = 8f;
    
    [Tooltip("Velocidad con la que aparece/desaparece la huella (Fade)")]
    [SerializeField] private float fadeSpeed = 5f;

    private int activeZoneIndex = -1;
    private float zoneTimer;
    private Renderer[] currentZoneRenderers;
    private float currentAlpha = 0f;

    private void Start()
    {
        SelectRandomZone();
    }

    private void Update()
    {
        HandleZoneTimer();
        HandleFootprintVisibility();
    }

    private void HandleZoneTimer()
    {
        zoneTimer -= Time.deltaTime;
        if (zoneTimer <= 0f)
        {
            SelectRandomZone();
        }
    }

    private void SelectRandomZone()
    {
        if (zones == null || zones.Length == 0) return;

        for (int i = 0; i < zones.Length; i++)
        {
            if (zones[i] != null) zones[i].SetActive(false);
        }

        int newIndex = activeZoneIndex;
        if (zones.Length > 1)
        {
            while (newIndex == activeZoneIndex)
            {
                newIndex = Random.Range(0, zones.Length);
            }
        }
        else
        {
            newIndex = 0;
        }

        activeZoneIndex = newIndex;

        if (zones[activeZoneIndex] != null)
        {
            zones[activeZoneIndex].SetActive(true);
            currentZoneRenderers = zones[activeZoneIndex].GetComponentsInChildren<Renderer>();
        }

        zoneTimer = zoneDuration;
    }

    private void HandleFootprintVisibility()
    {
        if (currentZoneRenderers == null || playerUVLight == null) return;

        bool shouldReveal = false;

        if (playerUVLight.IsOn && playerUVLight.Spotlight != null && zones[activeZoneIndex] != null)
        {
            Light lightComp = playerUVLight.Spotlight;
            Transform lightTransform = lightComp.transform;

            Vector3 zonePosition = zones[activeZoneIndex].transform.position;
            Vector3 dirToZone = zonePosition - lightTransform.position;
            float distanceToZone = dirToZone.magnitude;

            if (distanceToZone <= lightComp.range)
            {
                float angle = Vector3.Angle(lightTransform.forward, dirToZone.normalized);
                if (angle <= lightComp.spotAngle * 0.5f)
                {
                    shouldReveal = true;
                }
            }
        }

        float targetAlpha = shouldReveal ? 1f : 0f;
        currentAlpha = Mathf.MoveTowards(currentAlpha, targetAlpha, fadeSpeed * Time.deltaTime);

        foreach (Renderer rend in currentZoneRenderers)
        {
            if (rend != null && rend.material.HasProperty("_Color"))
            {
                Color c = rend.material.color;
                c.a = currentAlpha;
                rend.material.color = c;
            }
        }
    }

    public Transform GetActiveZoneTransform()
    {
        if (activeZoneIndex >= 0 && activeZoneIndex < zones.Length)
            return zones[activeZoneIndex].transform;
        return transform;
    }
}