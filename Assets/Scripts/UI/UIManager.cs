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
    public TMP_InputField inputNombreAlumno;
    public GameObject panelAlumno;
    public TMP_InputField inputCodigoAlumno;
    public Button btnJugarAlumno;

    [Header("Configuración de Cámaras")]
    public GameObject camaraEditor;
    public GameObject player;

    // Guardar la configuración actual temporalmente
    [HideInInspector]public WorldConfig configuracionActual; 

    private static UIManager _instance;
    public static UIManager Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = Object.FindAnyObjectByType<UIManager>();
            }
            return _instance;
        }
    }

    private void Awake()
    {
        if (_instance == null || _instance == this)
        {
            _instance = this;
        }
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
            textoCodigoGenerado.text = "Publicando en la nube...";
            
            // Usamos la nueva función de nube del SaveManager
            saveManager.GuardarClaseEnNube(configuracionActual, (codigo) => {
                if (codigo != "ERROR")
                {
                    textoCodigoGenerado.text = "¡Publicado! Comparte este código: " + codigo;
                }
                else
                {
                    textoCodigoGenerado.text = "Error al publicar. Intenta de nuevo.";
                }
            });
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
        string nombreInput = inputNombreAlumno.text.Trim();

        if (string.IsNullOrEmpty(codigoInput)) return;
        if (string.IsNullOrEmpty(nombreInput)) nombreInput = "Alumno_Sin_Nombre";

        // GUARDAMOS LOS DATOS EN LA SESIÓN GLOBAL
        SessionData.CodigoClaseActual = codigoInput;
        SessionData.NombreAlumno = nombreInput;

        btnJugarAlumno.interactable = false;
        inputCodigoAlumno.placeholder.GetComponent<TMP_Text>().text = "Buscando...";

        // Intentamos cargar de la nube primero
        saveManager.CargarClaseDeNube(codigoInput, (configCargada) => {
            btnJugarAlumno.interactable = true;

            if (configCargada != null)
            {
                Debug.Log("Cargando nivel exitosamente desde la NUBE: " + codigoInput);
                panelAlumno.SetActive(false); // Ocultar UI de alumno
                worldBuilder.ConstruirMundo(configCargada); // Construir el mundo visual
                ToggleModoJuego(); // Activar al jugador
            }
            else
            {
                // Si falla la nube, intentamos local (opcional)
                WorldConfig localConfig = saveManager.CargarClaseLocal(codigoInput);
                if (localConfig != null)
                {
                    Debug.Log("Cargando nivel exitosamente desde LOCAL: " + codigoInput);
                    panelAlumno.SetActive(false);
                    worldBuilder.ConstruirMundo(localConfig);
                    ToggleModoJuego();
                }
                else
                {
                    inputCodigoAlumno.text = "";
                    inputCodigoAlumno.placeholder.GetComponent<TMP_Text>().text = "Código no encontrado";
                }
            }
        });
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

        // Obtener códigos guardados localmente
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
        // El historial local lo cargamos localmente
        WorldConfig config = saveManager.CargarClaseLocal(codigo);
        if (config != null)
        {
            Debug.Log("Cargando desde el HISTORIAL LOCAL: " + codigo);
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
