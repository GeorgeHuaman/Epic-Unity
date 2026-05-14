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
    public GameObject player;

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

    [Header("UI Historial")]
    public GameObject panelHistorial;
    public Transform contenedorHistorial;
    public GameObject prefabBotonHistorial;

    public void MostrarHistorial()
    {
        if (panelHistorial == null) return;
        
        panelHistorial.SetActive(true);

        // Limpiar lista anterior
        foreach (Transform child in contenedorHistorial)
        {
            Destroy(child.gameObject);
        }

        // Obtener códigos
        string[] codigos = saveManager.ObtenerHistorialDeClases();

        foreach (string cod in codigos)
        {
            GameObject btnObj = Instantiate(prefabBotonHistorial, contenedorHistorial);
            btnObj.GetComponentInChildren<TMP_Text>().text = cod;
            
            // Al hacer clic, cargar esa clase
            string codigoParaBoton = cod; // Copia local para el closure
            btnObj.GetComponent<Button>().onClick.AddListener(() => {
                CargarDesdeHistorial(codigoParaBoton);
            });
        }
    }

    private void CargarDesdeHistorial(string codigo)
    {
        WorldConfig config = saveManager.CargarClase(codigo);
        if (config != null)
        {
            configuracionActual = config;
            worldBuilder.ConstruirMundo(config);
            if (textoCodigoGenerado != null) 
                textoCodigoGenerado.text = "Cargado desde historial: " + codigo;
            panelHistorial.SetActive(false);
        }
    }

    public void CerrarHistorial()
    {
        panelHistorial.SetActive(false);
    }

    public void ToggleModoJuego()
    {
        bool esModoJugador = !player.activeSelf;
        player.SetActive(esModoJugador);
        camaraEditor.SetActive(!esModoJugador);

        player.transform.position = GameObject.Find("Slot_Inicio").transform.position;

        // Ocultar Cursor si estamos en modo jugador
        Cursor.lockState = esModoJugador ? CursorLockMode.Locked : CursorLockMode.None;
        Cursor.visible = !esModoJugador;
    }
    }
