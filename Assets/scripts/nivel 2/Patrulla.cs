using UnityEngine;

public class Patrulla : MonoBehaviour
{
    [Header("Ruta de Patrullaje")]
    [SerializeField] private Transform[] waypoints;
    [SerializeField] private float moveSpeed = 3.0f;
    [SerializeField] private float alertSpeed = 5.5f;
    [SerializeField] private float waitAtWaypoint = 1.5f;

    [Header("Pausa al Detectar al Jugador")]
    [Tooltip("Tiempo en segundos que la patrulla se detiene al iluminar a Darwin")]
    [SerializeField] private float tiempoPausaDeteccion = 1.5f;

    private int currentWaypointIndex = 0;
    private Vector3 targetPosition;
    private bool isInvestigatingNoise = false;
    private float waitTimer = 0f;
    private float pauseTimer = 0f;

    public Vector3 CurrentPosition => transform.position;

    private void OnEnable()
    {
        Ruidoss.OnNoiseGenerated += InvestigateNoise;
    }

    private void OnDisable()
    {
        Ruidoss.OnNoiseGenerated -= InvestigateNoise;
    }

    private void Start()
    {
        if (waypoints != null && waypoints.Length > 0)
            targetPosition = waypoints[0].position;
    }

    private void Update()
    {
        if (pauseTimer > 0f)
        {
            pauseTimer -= Time.deltaTime;
            return;
        }

        float speed = isInvestigatingNoise ? alertSpeed : moveSpeed;
        transform.position = Vector3.MoveTowards(transform.position, targetPosition, speed * Time.deltaTime);

        Vector3 dir = (targetPosition - transform.position).normalized;
        if (dir != Vector3.zero)
        {
            Quaternion targetRot = Quaternion.LookRotation(new Vector3(dir.x, 0, dir.z));
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, Time.deltaTime * 4f);
        }

        if (Vector3.Distance(transform.position, targetPosition) < 0.2f)
        {
            if (isInvestigatingNoise)
            {
                waitTimer += Time.deltaTime;
                if (waitTimer >= waitAtWaypoint)
                {
                    isInvestigatingNoise = false;
                    waitTimer = 0f;
                    ResumePatrol();
                }
            }
            else if (waypoints != null && waypoints.Length > 0)
            {
                waitTimer += Time.deltaTime;
                if (waitTimer >= waitAtWaypoint)
                {
                    waitTimer = 0f;
                    currentWaypointIndex = (currentWaypointIndex + 1) % waypoints.Length;
                    targetPosition = waypoints[currentWaypointIndex].position;
                }
            }
        }
    }

    public void DetenerPorDeteccion()
    {
        if (pauseTimer <= 0f)
        {
            pauseTimer = tiempoPausaDeteccion;
        }
    }

    public void InvestigateNoise(Vector3 noisePosition)
    {
        targetPosition = noisePosition;
        isInvestigatingNoise = true;
        waitTimer = 0f;
    }

    private void ResumePatrol()
    {
        if (waypoints != null && waypoints.Length > 0)
            targetPosition = waypoints[currentWaypointIndex].position;
    }
}