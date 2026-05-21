using UnityEngine;
using TMPro;
using System.Collections.Generic;

public class Mechanic_Victory : MonoBehaviour, IConfigurable
{
    [Header("Referencias UI")]
    public GameObject panelVictoria;
    public TMP_Text mensajeDespedida;

    private string mensajeFinal = "¡Felicidades, has completado la clase!";

    public void Setup(Dictionary<string, string> data)
    {
        if (data.ContainsKey("mensaje_final"))
        {
            mensajeFinal = data["mensaje_final"];
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            CompletarNivel(other.gameObject);
        }
    }

    private void CompletarNivel(GameObject jugador)
    {
        // 1. Mostrar pantalla de victoria
        mensajeDespedida.text = mensajeFinal;
        panelVictoria.SetActive(true);

        // 2. Detener al jugador y liberar el mouse
        jugador.GetComponent<PlayerMovement>().SetMovement(false);

        // Enviar la métrica de victoria
        MetricaEvento metrica = new MetricaEvento
        {
            nombre_alumno = SessionData.NombreAlumno,
            tipo_evento = "NIVEL_COMPLETADO",
            detalle = "El alumno completó la clase con éxito."
        };
        StartCoroutine(FindObjectOfType<CloudManager>().EnviarMetrica(metrica));

    }
}
