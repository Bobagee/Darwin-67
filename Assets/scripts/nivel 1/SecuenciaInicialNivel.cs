using System.Collections;
using UnityEngine;

public class SecuenciaInicialNivel : MonoBehaviour
{
    [SerializeField] private Voz voz;
    [SerializeField] private PlayerController jugador;

    [SerializeField] private float segundosExtraAlInicio = 0.5f;

    private IEnumerator Start()
    {
        yield return new WaitForSecondsRealtime(segundosExtraAlInicio);

        if (voz != null)
            voz.DetenerTodo();

        if (jugador != null)
            jugador.BloquearAcciones(true);

        if (voz != null)
        {
            yield return StartCoroutine(
                voz.ReproducirSecuenciaInicial(() =>
                {
                    if (jugador != null)
                        jugador.BloquearAcciones(false);
                }));
        }
        else if (jugador != null)
        {
            jugador.BloquearAcciones(false);
        }
    }
}