using UnityEngine;
using TMPro;
using System.Collections.Generic;

// Hereda de MonoBehaviour e implementa IConfigurable
public class Mechanic_NPC : MonoBehaviour, IConfigurable
{
    [Header("Referencias UI")]
    public GameObject canvasDialogo; // El Canvas que flota sobre el NPC
    public TMP_Text textoComponente;

    private string dialogoAsignado;

    // Esta función es llamada por el WorldBuilder automáticamente
    public void Setup(Dictionary<string, string> data)
    {
        // Extraemos el texto que la IA decidió
        if (data.ContainsKey("texto"))
        {
            dialogoAsignado = data["texto"];
            textoComponente.text = dialogoAsignado;
        }

        canvasDialogo.SetActive(false);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            canvasDialogo.SetActive(true);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            canvasDialogo.SetActive(false);
        }
    }
}
