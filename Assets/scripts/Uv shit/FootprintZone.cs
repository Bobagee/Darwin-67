using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class FootprintZone : MonoBehaviour
{
    [Header("Referencias del Jugador y Luz")]
    [SerializeField] private UvLights playerUVLight;

    [Header("Sincronización con Darwin")]
    [SerializeField] private Transform darwinTransform;
    [Tooltip("Oculta a Darwin mientras espera escondido y lo muestra en cuanto sale hacia la trampa")]
    [SerializeField] private bool ocultarDarwinMientrasEspera = true;

    [Header("Zonas de Huellas (Arrastra los objetos aqui)")]
    [SerializeField] private GameObject[] zones;

    [Header("Tiempos y Visibilidad Sutil")]
    [Tooltip("Segundos que dura activa una zona antes de cambiar a otra")]
    [SerializeField] private float zoneDuration = 25f;

    [Tooltip("Velocidad con la que aparece/desaparece la huella (Fade)")]
    [SerializeField] private float fadeSpeed = 4f;

    [Tooltip("Opacidad máxima bajo luz UV (0.35 - 0.5 para que sea sutil y no brille de más)")]
    [Range(0.05f, 1f)]
    [SerializeField] private float maxAlpha = 0.45f;

    private int activeZoneIndex = -1;
    private float zoneTimer;
    private Renderer[] currentZoneRenderers;
    private float currentAlpha = 0f;
    private bool siendoAlumbrada = false;

    private Renderer[] darwinRenderers;
    private NavMeshAgent darwinAgent;
    private MouseBaitAI darwinAI;
    private Vector3 posicionSpawnActual;
    private bool darwinVisible = false;

    private void Awake()
    {
        if (darwinTransform != null)
        {
            darwinRenderers = darwinTransform.GetComponentsInChildren<Renderer>(true);
            darwinAgent = darwinTransform.GetComponent<NavMeshAgent>();
            darwinAI = darwinTransform.GetComponent<MouseBaitAI>();
        }

        Escape.OnTrapFailed += AlFallarTrampa;
    }

    private void OnDestroy()
    {
        Escape.OnTrapFailed -= AlFallarTrampa;
    }

    private void Start()
    {
        SelectRandomZone();
    }

    private void Update()
    {
        HandleZoneTimer();
        HandleFootprintVisibility();
        HandleDarwinVisibility();
    }

    private void HandleZoneTimer()
    {
        // No cambiar de zona si el jugador está mirando las huellas o si Darwin ya está en acción
        bool darwinOcupado = darwinAI != null && !darwinAI.EstaLibre;
        if (siendoAlumbrada || darwinOcupado) return;

        zoneTimer -= Time.deltaTime;
        if (zoneTimer <= 0f)
        {
            SelectRandomZone();
        }
    }

    public void SelectRandomZone()
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
        currentAlpha = 0f;
        siendoAlumbrada = false;

        GameObject zonaActiva = zones[activeZoneIndex];
        if (zonaActiva != null)
        {
            zonaActiva.SetActive(true);
            currentZoneRenderers = zonaActiva.GetComponentsInChildren<Renderer>();
            AplicarAlphaHuellas(0f);
            SincronizarPosicionDarwin(zonaActiva.transform);
        }

        zoneTimer = zoneDuration;
    }

    private void SincronizarPosicionDarwin(Transform zonaTransform)
    {
        if (darwinTransform == null || zonaTransform == null) return;

        // Si dentro de la zona creas un hijo vacío llamado "PuntoSalida", Darwin saldrá desde ahí;
        // si no existe, usará el centro de la zona.
        Transform puntoSalida = zonaTransform.Find("PuntoSalida");
        Transform destino = puntoSalida != null ? puntoSalida : zonaTransform;

        posicionSpawnActual = destino.position;

        if (darwinAgent != null && darwinAgent.enabled)
            darwinAgent.Warp(destino.position);
        else
            darwinTransform.position = destino.position;

        darwinTransform.rotation = destino.rotation;

        if (ocultarDarwinMientrasEspera)
            SetDarwinRenderers(false);
    }

    private void HandleDarwinVisibility()
    {
        if (!ocultarDarwinMientrasEspera || darwinTransform == null || darwinVisible) return;

        bool empezoAMoverse = Vector3.Distance(
            new Vector3(darwinTransform.position.x, 0f, darwinTransform.position.z),
            new Vector3(posicionSpawnActual.x, 0f, posicionSpawnActual.z)
        ) > 0.2f;

        bool dejoDeEstarLibre = darwinAI != null && !darwinAI.EstaLibre;

        if (empezoAMoverse || dejoDeEstarLibre)
        {
            SetDarwinRenderers(true);
        }
    }

    private void SetDarwinRenderers(bool visible)
    {
        darwinVisible = visible;
        if (darwinRenderers == null) return;

        foreach (Renderer r in darwinRenderers)
        {
            if (r != null) r.enabled = visible;
        }
    }

    private void HandleFootprintVisibility()
    {
        if (currentZoneRenderers == null || playerUVLight == null) return;

        siendoAlumbrada = false;

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
                    siendoAlumbrada = true;
                }
            }
        }

        float targetAlpha = siendoAlumbrada ? maxAlpha : 0f;
        currentAlpha = Mathf.MoveTowards(currentAlpha, targetAlpha, fadeSpeed * Time.deltaTime);
        AplicarAlphaHuellas(currentAlpha);
    }

    private void AplicarAlphaHuellas(float alpha)
    {
        if (currentZoneRenderers == null) return;

        foreach (Renderer rend in currentZoneRenderers)
        {
            if (rend == null) continue;

            Material mat = rend.material;
            if (mat.HasProperty("_BaseColor"))
            {
                Color c = mat.GetColor("_BaseColor");
                c.a = alpha;
                mat.SetColor("_BaseColor", c);
            }
            else if (mat.HasProperty("_Color"))
            {
                Color c = mat.color;
                c.a = alpha;
                mat.color = c;
            }
        }
    }

    private void AlFallarTrampa()
    {
        StartCoroutine(ReubicarTrasEscape());
    }

    private IEnumerator ReubicarTrasEscape()
    {
        yield return new WaitForSeconds(2.5f);
        SelectRandomZone();
    }

    public Transform GetActiveZoneTransform()
    {
        if (activeZoneIndex >= 0 && activeZoneIndex < zones.Length)
            return zones[activeZoneIndex].transform;
        return transform;
    }
}