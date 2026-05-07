using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class UIManager : MonoBehaviour
{
    [Header("Referencias Principales")]
    public OpenAIConnector aiConnector;
    public WorldBuilder worldBuilder;
    public SaveManager saveManager;

    [Header("Referencias de Guardado (Docente)")]
    public TMP_Text textoCodigoGenerado; // Donde el docente verá su código
    public TMP_InputField inputPrompt; // Para el prompt de generación

    [Header("UI Alumno")]
    public GameObject panelAlumno;
    public TMP_InputField inputCodigoAlumno;
    public Button btnJugarAlumno;

    [Header("Configuración de Cámaras")]
    public GameObject camaraEditor;
    public GameObject jugador;

    // Guardar la configuración actual temporalmente
    private WorldConfig configuracionActual; 

    public static UIManager Instance;

    private void Awake()
    {
        if (Instance == null) Instance = this;
    }

    // Función para el botón de Generar
    public void AlPresionarGenerar()
    {
        string prompt = inputPrompt.text;
        if (string.IsNullOrEmpty(prompt)) return;

        // Feedback visual
        if (textoCodigoGenerado != null) textoCodigoGenerado.text = "Generando mundo... espera un momento.";
        
        // Si existe el sistema de chat, podemos activar los puntitos
        if (ManagerUI.Instance != null) ManagerUI.Instance.StartTyping();

        StartCoroutine(aiConnector.EnviarPromptALaIA(prompt, (config) => {
            configuracionActual = config;
            worldBuilder.ConstruirMundo(config);
            
            if (textoCodigoGenerado != null) textoCodigoGenerado.text = "¡Mundo listo! Ahora puedes publicarlo.";
            if (ManagerUI.Instance != null) ManagerUI.Instance.StopTyping("¡Mundo generado!");
        }));
    }

    // Nueva función: Conectarla al botón "Publicar" del Docente
    public void AlPresionarPublicar()
    {
        if (configuracionActual != null)
        {
            string codigo = saveManager.GuardarClase(configuracionActual);
            textoCodigoGenerado.text = "¡Comparte este código con tus alumnos!: " + codigo;
        }
        else
        {
            Debug.LogWarning("No hay configuración para publicar. Genera un mundo primero.");
        }
    }

    // Nueva función: Conectarla al botón "Jugar" del Alumno
    public void AlPresionarJugarAlumno()
    {
        string codigoInput = inputCodigoAlumno.text.Trim().ToUpper();
        WorldConfig configCargada = saveManager.CargarClase(codigoInput);

        if (configCargada != null)
        {
            panelAlumno.SetActive(false); // Ocultar UI de alumno
            worldBuilder.ConstruirMundo(configCargada); // Construir el mundo
            ToggleModoJuego(); // Activar al jugador y esconder la cámara de editor
        }
        else
        {
            // Mostrar feedback de error al alumno
            inputCodigoAlumno.text = "";
            inputCodigoAlumno.placeholder.GetComponent<TMP_Text>().text = "Código inválido";
        }
    }

    public void ToggleModoJuego()
    {
        bool esModoJugador = !jugador.activeSelf;
        jugador.SetActive(esModoJugador);
        camaraEditor.SetActive(!esModoJugador);
        
        // Ocultar Cursor si estamos en modo jugador
        Cursor.lockState = esModoJugador ? CursorLockMode.Locked : CursorLockMode.None;
        Cursor.visible = !esModoJugador;
    }
}
