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
        string mode = data.ContainsKey("mode") ? data["mode"] : "guide";
        Quiz quiz = GetComponent<Quiz>();

        if (mode == "quiz" && quiz != null)
        {
            // Desactivar este componente y activar Quiz
            quiz.enabled = true;
            quiz.Setup(data);
            
            if (canvasDialogo != null) canvasDialogo.SetActive(false);
            this.enabled = false;
            return;
        }

        // Modo Guía (default)
        if (quiz != null) quiz.enabled = false;
        this.enabled = true;

        // Extraemos el texto que la IA decidió
        if (data.ContainsKey("texto"))
        {
            dialogoAsignado = data["texto"];
            if (textoComponente != null) textoComponente.text = dialogoAsignado;
        }

        if (canvasDialogo != null) canvasDialogo.SetActive(false);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!enabled) return;

        if (other.CompareTag("Player"))
        {
            canvasDialogo.SetActive(true);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (!enabled) return;

        if (other.CompareTag("Player"))
        {
            canvasDialogo.SetActive(false);
        }
    }
}