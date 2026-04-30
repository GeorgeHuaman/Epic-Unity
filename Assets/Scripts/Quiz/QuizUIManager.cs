using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using UnityEngine.XR;

public class QuizUIManager : MonoBehaviour
{
    public static QuizUIManager Instance;

    [Header("Referencias UI")]
    public Canvas quizCanvas;
    public TextMeshProUGUI textoPregunta;
    public VerticalLayoutGroup contenedorRespuestas;
    public GameObject prefabBotonRespuesta;

    [Header("Configuración VR")]
    public Vector3 posicionVR = new Vector3(0, 1.5f, 2f);
    public float escalaVR = 0.002f;

    private NPCQuiz quizActual;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        if (quizCanvas == null)
        {
            quizCanvas = GetComponentInChildren<Canvas>();
            if (quizCanvas == null) CreateDefaultUI();
        }
        
        CerrarQuiz();
    }

    private void CreateDefaultUI()
    {
        // Crear Canvas
        GameObject canvasObj = new GameObject("QuizCanvas");
        canvasObj.transform.SetParent(this.transform);
        quizCanvas = canvasObj.AddComponent<Canvas>();
        quizCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvasObj.AddComponent<CanvasScaler>().uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        canvasObj.AddComponent<GraphicRaycaster>();

        // Fondo Pregunta
        GameObject panelPregunta = new GameObject("PanelPregunta", typeof(RectTransform), typeof(Image));
        panelPregunta.transform.SetParent(canvasObj.transform);
        panelPregunta.GetComponent<Image>().color = new Color(0, 0, 0, 0.8f);
        RectTransform rtPanel = panelPregunta.GetComponent<RectTransform>();
        rtPanel.anchorMin = new Vector2(0.5f, 0.7f);
        rtPanel.anchorMax = new Vector2(0.5f, 0.9f);
        rtPanel.sizeDelta = new Vector2(800, 150);
        rtPanel.anchoredPosition = Vector2.zero;

        // Texto Pregunta
        GameObject txtObj = new GameObject("TextoPregunta", typeof(RectTransform), typeof(TextMeshProUGUI));
        txtObj.transform.SetParent(panelPregunta.transform);
        textoPregunta = txtObj.GetComponent<TextMeshProUGUI>();
        textoPregunta.alignment = TextAlignmentOptions.Center;
        textoPregunta.fontSize = 36;
        textoPregunta.text = "Pregunta?";
        RectTransform rtTxt = txtObj.GetComponent<RectTransform>();
        rtTxt.anchorMin = Vector2.zero;
        rtTxt.anchorMax = Vector2.one;
        rtTxt.sizeDelta = Vector2.zero;

        // Contenedor Respuestas
        GameObject contObj = new GameObject("ContenedorRespuestas", typeof(RectTransform), typeof(VerticalLayoutGroup));
        contObj.transform.SetParent(canvasObj.transform);
        contenedorRespuestas = contObj.GetComponent<VerticalLayoutGroup>();
        contenedorRespuestas.childAlignment = TextAnchor.MiddleCenter;
        contenedorRespuestas.childControlHeight = true;
        contenedorRespuestas.childControlWidth = true;
        contenedorRespuestas.spacing = 10;
        RectTransform rtCont = contObj.GetComponent<RectTransform>();
        rtCont.anchorMin = new Vector2(0.5f, 0.2f);
        rtCont.anchorMax = new Vector2(0.5f, 0.6f);
        rtCont.sizeDelta = new Vector2(600, 300);
        rtCont.anchoredPosition = Vector2.zero;

        // Nota: Para prefabBotonRespuesta, el usuario deberá asignar uno o crear uno básico.
        // Por ahora, si es null, crearemos uno básico por código al mostrar.
    }

    public void MostrarQuiz(NPCQuiz data)
    {
        quizActual = data;
        textoPregunta.text = data.pregunta;

        // Limpiar respuestas anteriores
        foreach (Transform child in contenedorRespuestas.transform)
        {
            Destroy(child.gameObject);
        }

        // Crear nuevos botones de respuesta
        for (int i = 0; i < data.respuestas.Count; i++)
        {
            int index = i;
            GameObject btnObj;
            if (prefabBotonRespuesta != null)
            {
                btnObj = Instantiate(prefabBotonRespuesta, contenedorRespuestas.transform);
            }
            else
            {
                btnObj = CreateBasicButton(data.respuestas[i]);
            }
            
            btnObj.GetComponentInChildren<TextMeshProUGUI>().text = data.respuestas[i];
            
            Button btn = btnObj.GetComponent<Button>();
            btn.onClick.AddListener(() => OnClickRespuesta(index));
        }

        ConfigurarCanvasSegunPlataforma();
        quizCanvas.gameObject.SetActive(true);
    }

    private GameObject CreateBasicButton(string label)
    {
        GameObject btnObj = new GameObject("BotonRespuesta", typeof(RectTransform), typeof(Image), typeof(Button));
        btnObj.transform.SetParent(contenedorRespuestas.transform);
        btnObj.GetComponent<Image>().color = Color.white;
        
        GameObject txtObj = new GameObject("Texto", typeof(RectTransform), typeof(TextMeshProUGUI));
        txtObj.transform.SetParent(btnObj.transform);
        TextMeshProUGUI tmp = txtObj.GetComponent<TextMeshProUGUI>();
        tmp.text = label;
        tmp.color = Color.black;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.fontSize = 24;

        RectTransform rtTxt = txtObj.GetComponent<RectTransform>();
        rtTxt.anchorMin = Vector2.zero;
        rtTxt.anchorMax = Vector2.one;
        rtTxt.sizeDelta = Vector2.zero;

        return btnObj;
    }

    private void ConfigurarCanvasSegunPlataforma()
    {
        // Detectar si estamos en VR
        bool isVR = XRSettings.isDeviceActive;

        if (isVR)
        {
            quizCanvas.renderMode = RenderMode.WorldSpace;
            quizCanvas.transform.position = quizActual.transform.position + Vector3.up * 2f;
            quizCanvas.transform.LookAt(Camera.main.transform);
            quizCanvas.transform.Rotate(0, 180, 0); // Corregir rotación para que mire al jugador
            quizCanvas.transform.localScale = Vector3.one * escalaVR;
        }
        else
        {
            quizCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
            // El CanvasScaler se encargará de adaptar el tamaño en PC/Móvil
        }
    }

    private void OnClickRespuesta(int index)
    {
        if (index == quizActual.indiceCorrecto)
        {
            Debug.Log("¡Correcto!");
            // Podrías añadir feedback visual aquí
        }
        else
        {
            Debug.Log("Incorrecto.");
        }
        
        CerrarQuiz();
    }

    public void CerrarQuiz()
    {
        if (quizCanvas != null)
            quizCanvas.gameObject.SetActive(false);
    }
}
