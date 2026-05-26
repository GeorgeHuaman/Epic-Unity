using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic;

public class Quiz : MonoBehaviour, IConfigurable
{
    [Header("Datos del Quiz")]
    public string pregunta = "¿Cuál es la capital de Francia?";
    public List<string> respuestas = new List<string> { "Madrid", "París", "Londres", "Berlín" };
    public int indiceCorrecto = 1;

    [Header("Configuración")]
    private bool jugadorCerca = false;
    public bool isCompleted = false;

    public void Setup(Dictionary<string, string> data)
    {
        string mode = data.ContainsKey("mode") ? data["mode"] : "guide";
        Mechanic_NPC guide = GetComponent<Mechanic_NPC>();

        if (mode == "guide" && guide != null)
        {
            guide.enabled = true;
            guide.Setup(data);
            this.enabled = false;
            return;
        }

        // Modo Quiz
        if (guide != null) guide.enabled = false;
        this.enabled = true;
        isCompleted = false;

        if (data.ContainsKey("pregunta")) pregunta = data["pregunta"];
        if (data.ContainsKey("opciones"))
        {
            respuestas = new List<string>(data["opciones"].Split(';'));
        }
        if (data.ContainsKey("respuesta_correcta"))
        {
            int.TryParse(data["respuesta_correcta"], out indiceCorrecto);
        }
    }

    private void Update()
    {
        if (!enabled || isCompleted) return;
        
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
        if (isCompleted) return;

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
        if (!enabled || isCompleted) return;

        // Soporte para tags Player o XR Rig
        if (other.CompareTag("Player") || other.name.Contains("XR"))
        {
            jugadorCerca = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (!enabled) return;

        if (other.CompareTag("Player") || other.name.Contains("XR"))
        {
            jugadorCerca = false;
            if (QuizUIManager.Instance != null) QuizUIManager.Instance.CerrarQuiz();
        }
    }
}
