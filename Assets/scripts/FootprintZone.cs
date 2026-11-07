using UnityEngine;

public class FootprintZone : MonoBehaviour
{
    [Header("Referencias de Luz y Zonas")]
    [SerializeField] private UVLight playerUVLight;
    [SerializeField] private GameObject[] zones; 
     [SerializeField] private float zoneDuration = 8f;

    [Header("Configuración Visual de Huellas")]
    [SerializeField] private float fadeSpeed = 4f;

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
            Vector3 dirToZone = (zones[activeZoneIndex].transform.position - playerUVLight.transform.position).normalized;
            float angle = Vector3.Angle(playerUVLight.transform.forward, dirToZone);

            if (angle <= playerUVLight.Spotlight.spotAngle * 0.5f)
            {
                shouldReveal = true;
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