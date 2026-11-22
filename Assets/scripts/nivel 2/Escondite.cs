using UnityEngine;

public class Escondite : MonoBehaviour
{
    [Header("Puntos de Teletransporte")]
    [SerializeField] private Transform insideSpot;
    [SerializeField] private Transform exitSpot;

    [Header("Tecla y Brillo")]
    [SerializeField] private KeyCode teclaEsconderse = KeyCode.Q;
    [SerializeField] private float distanciaEsconderse = 2.5f;
    [SerializeField] private Color colorBrillo = new Color(0.4f, 0.8f, 1f, 1f);

    public static bool IsPlayerHidden { get; private set; } = false;

    private Transform playerTransform;
    private Renderer[] renderers;
    private bool brillando = false;
    private bool escondidoEnEste = false;

    private void Start()
    {
        IsPlayerHidden = false;
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null) playerTransform = player.transform;
        renderers = GetComponentsInChildren<Renderer>();
    }

    private void Update()
    {
        if (playerTransform == null) return;

        float dist = Vector3.Distance(playerTransform.position, transform.position);
        bool cerca = dist <= distanciaEsconderse;

        ActualizarBrillo(cerca && !IsPlayerHidden);

        if ((cerca && !IsPlayerHidden && Input.GetKeyDown(teclaEsconderse)) ||
            (escondidoEnEste && Input.GetKeyDown(teclaEsconderse)))
        {
            Interactuar();
        }
    }

    public void Interactuar()
    {
        if (playerTransform == null) return;

        CharacterController cc = playerTransform.GetComponent<CharacterController>();
        escondidoEnEste = !escondidoEnEste;
        IsPlayerHidden = escondidoEnEste;

        if (cc != null) cc.enabled = false;

        if (escondidoEnEste && insideSpot != null)
            playerTransform.position = insideSpot.position;
        else if (!escondidoEnEste && exitSpot != null)
            playerTransform.position = exitSpot.position;

        if (cc != null) cc.enabled = true;
    }

    private void ActualizarBrillo(bool activar)
    {
        if (brillando == activar) return;
        brillando = activar;

        foreach (Renderer r in renderers)
        {
            if (r == null) continue;
            if (activar)
            {
                r.material.EnableKeyword("_EMISSION");
                r.material.SetColor("_EmissionColor", colorBrillo * 1.3f);
            }
            else
            {
                r.material.SetColor("_EmissionColor", Color.black);
                r.material.DisableKeyword("_EMISSION");
            }
        }
    }
}