using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Mechanic_QuizDoor : MonoBehaviour, IConfigurable
{
    [Header("Referencias F�sicas")]
    public GameObject mallaPuerta; // La puerta f�sica que bloquea el paso

    [Header("Referencias UI")]
    public GameObject canvasQuiz;
    public TMP_Text preguntaTexto;
    public TMP_InputField respuestaInput;
    public Button enviarBoton;

    private string pregunta;
    private string respuestaCorrecta;

    public void Setup(Dictionary<string, string> data)
    {
        // 1. Extraer datos del JSON (IA)
        if (data.ContainsKey("pregunta")) pregunta = data["pregunta"];
        if (data.ContainsKey("respuesta_correcta")) respuestaCorrecta = data["respuesta_correcta"];

        // 2. Configurar la UI
        preguntaTexto.text = pregunta;
        enviarBoton.onClick.AddListener(VerificarRespuesta);

        canvasQuiz.SetActive(false); // Ocultar UI al inicio
    }

    private Coroutine deactivationCoroutine;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if (deactivationCoroutine != null)
            {
                StopCoroutine(deactivationCoroutine);
                deactivationCoroutine = null;
            }
            canvasQuiz.SetActive(true);
            other.GetComponent<PlayerMovement>().SetMovement(false);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            deactivationCoroutine = StartCoroutine(DesactivarQuizAlFinalDelFrame());
            other.GetComponent<PlayerMovement>().SetMovement(true);
        }
    }

    private IEnumerator DesactivarQuizAlFinalDelFrame()
    {
        yield return new WaitForEndOfFrame();
        canvasQuiz.SetActive(false);
        deactivationCoroutine = null;
    }

    private void VerificarRespuesta()
    {
        // Limpiamos espacios y pasamos a min�sculas para evitar errores tontos del alumno
        string respuestaAlumno = respuestaInput.text.Trim().ToLower();
        string respuestaMeta = respuestaCorrecta.Trim().ToLower();

        if (respuestaAlumno == respuestaMeta)
        {
            AbrirPuerta();
        }
        else
        {
            // Feedback de error (podr�a reproducir un sonido o cambiar color)
            respuestaInput.text = "";
            respuestaInput.placeholder.GetComponent<TMP_Text>().text = "Incorrecto. Intenta de nuevo...";
            respuestaInput.placeholder.GetComponent<TMP_Text>().color = Color.red;
        }
    }

    private void AbrirPuerta()
    {
        // Ocultamos el quiz
        canvasQuiz.SetActive(false);

        // Hacemos desaparecer la puerta (En el futuro aqu� puedes poner una animaci�n)
        mallaPuerta.SetActive(false);

        // Desactivamos este Trigger para que el quiz no vuelva a aparecer
        GetComponent<Collider>().enabled = false;

        GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerMovement>().SetMovement(true);

    }

}
