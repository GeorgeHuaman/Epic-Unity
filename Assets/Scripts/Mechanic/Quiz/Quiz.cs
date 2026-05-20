using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic;

public class Quiz : MonoBehaviour
{
    [Header("Datos del Quiz")]
    public string pregunta = "¿Cuál es la capital de Francia?";
    public List<string> respuestas = new List<string> { "Madrid", "París", "Londres", "Berlín" };
    public int indiceCorrecto = 1;

    [Header("Configuración")]
    private bool jugadorCerca = false;

    private void Update()
    {
        // Interacción para PC (Tecla E - Usando el nuevo Input System de forma simple)
        if (jugadorCerca && Keyboard.current != null && Keyboard.current.eKey.wasPressedThisFrame)
        {
            Interact();
        }
    }

    /// <summary>
    /// Método público para ser llamado desde VR u otros sistemas de interacción.
    /// </summary>
    public void Interact()
    {
        if (QuizUIManager.Instance != null)
        {
            QuizUIManager.Instance.MostrarQuiz(this);
        }
        else
        {
            Debug.LogError("No se encontró QuizUIManager en la escena. Asegúrate de tener el objeto con el script.");
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        // Soporte para tags Player o XR Rig
        if (other.CompareTag("Player") || other.name.Contains("XR"))
        {
            jugadorCerca = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player") || other.name.Contains("XR"))
        {
            jugadorCerca = false;
            if (QuizUIManager.Instance != null) QuizUIManager.Instance.CerrarQuiz();
        }
    }
}
