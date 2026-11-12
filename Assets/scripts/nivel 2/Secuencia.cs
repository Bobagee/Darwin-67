using System.Collections;
using UnityEngine;

public class Secuencia : MonoBehaviour
{
    [Header("Referencias de Captura")]
    [SerializeField] private Transform playerTransform;
    [SerializeField] private Transform puntoJaulaOInicio;
    [SerializeField] private GameObject panelAtrapadoUI;
    [SerializeField] private float duracionSecuencia = 2.0f;

    private bool enSecuencia = false;

    private void OnEnable()
    {
        TiempoBajoLuz.OnPlayerCaught += IniciarSecuenciaCaptura;
    }

    private void OnDisable()
    {
        TiempoBajoLuz.OnPlayerCaught -= IniciarSecuenciaCaptura;
    }

    private void IniciarSecuenciaCaptura()
    {
        if (!enSecuencia)
        {
            StartCoroutine(RutinaCaptura());
        }
    }

    private IEnumerator RutinaCaptura()
    {
        enSecuencia = true;

        PlayerController controller = playerTransform.GetComponent<PlayerController>();
        CharacterController cc = playerTransform.GetComponent<CharacterController>();

        if (controller != null) controller.enabled = false;
        if (panelAtrapadoUI != null) panelAtrapadoUI.SetActive(true);

        yield return new WaitForSeconds(duracionSecuencia);

        if (cc != null) cc.enabled = false;
        if (puntoJaulaOInicio != null)
        {
            playerTransform.position = puntoJaulaOInicio.position;
            playerTransform.rotation = puntoJaulaOInicio.rotation;
        }
        if (cc != null) cc.enabled = true;

        if (panelAtrapadoUI != null) panelAtrapadoUI.SetActive(false);
        if (controller != null) controller.enabled = true;

        enSecuencia = false;
    }
}