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
    public LayoutGroup contenedorRespuestas;
    public GameObject prefabBotonRespuesta;

    [Header("Configuración VR")]
    public float escalaVR = 0.002f;
    private Camera camaraPrincipal;
    private NPCQuiz quizActual;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else { Destroy(gameObject); return; }

        camaraPrincipal = Camera.main;
        ValidarReferencias();
        CerrarQuiz();
    }

    private void ValidarReferencias()
    {
        // Si ya están asignadas en el inspector, no hacemos nada más
        if (quizCanvas != null && textoPregunta != null && contenedorRespuestas != null) return;

        // Intentamos buscar en hijos
        if (quizCanvas == null) quizCanvas = GetComponentInChildren<Canvas>();
        if (textoPregunta == null) textoPregunta = GetComponentInChildren<TextMeshProUGUI>();
        if (contenedorRespuestas == null) contenedorRespuestas = GetComponentInChildren<LayoutGroup>();

    }
    private GameObject CrearUIElement(string nombre, Transform padre, Vector2 min, Vector2 max)
    {
        GameObject obj = new GameObject(nombre, typeof(RectTransform));
        obj.transform.SetParent(padre);
        RectTransform rt = obj.GetComponent<RectTransform>();
        rt.anchorMin = min;
        rt.anchorMax = max;
        rt.offsetMin = rt.offsetMax = Vector2.zero;
        return obj;
    }

    public void MostrarQuiz(NPCQuiz data)
    {
        quizActual = data;
        textoPregunta.text = data.pregunta;

        LimpiarRespuestas();

        for (int i = 0; i < data.respuestas.Count; i++)
        {
            char letra = (char)('a' + i);
            string contenido = $"{letra}) {data.respuestas[i]}";
            int index = i;

            GameObject btnObj = prefabBotonRespuesta != null ? 
                Instantiate(prefabBotonRespuesta, contenedorRespuestas.transform) : 
                CreateImprovedButton(contenido);

            btnObj.GetComponentInChildren<TextMeshProUGUI>().text = contenido;
            btnObj.GetComponent<Button>().onClick.AddListener(() => OnClickRespuesta(index));
        }

        ConfigurarCanvasSegunPlataforma();
        quizCanvas.gameObject.SetActive(true);
    }

    private void LimpiarRespuestas()
    {
        foreach (Transform child in contenedorRespuestas.transform) Destroy(child.gameObject);
    }

    private GameObject CreateImprovedButton(string label)
    {
        GameObject btnObj = new GameObject("Boton", typeof(RectTransform), typeof(Image), typeof(Button));
        btnObj.transform.SetParent(contenedorRespuestas.transform);
        
        // Estética del Botón
        Image img = btnObj.GetComponent<Image>();
        img.color = new Color(0.15f, 0.15f, 0.15f); // Gris oscuro moderno
        btnObj.AddComponent<Outline>().effectColor = Color.cyan;
        
        Button btn = btnObj.GetComponent<Button>();
        ColorBlock cb = btn.colors;
        cb.highlightedColor = new Color(0.3f, 0.3f, 0.3f);
        cb.pressedColor = Color.cyan;
        btn.colors = cb;

        // Texto
        GameObject txtObj = CrearUIElement("Texto", btnObj.transform, Vector2.zero, Vector2.one);
        TextMeshProUGUI tmp = txtObj.AddComponent<TextMeshProUGUI>();
        tmp.text = label;
        tmp.color = Color.white;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.fontSize = 28;

        return btnObj;
    }

    private void ConfigurarCanvasSegunPlataforma()
    {
        if (XRSettings.isDeviceActive)
        {
            quizCanvas.renderMode = RenderMode.WorldSpace;
            Transform tr = quizCanvas.transform;
            tr.position = quizActual.transform.position + Vector3.up * 2.2f;
            tr.LookAt(camaraPrincipal.transform);
            tr.Rotate(0, 180, 0);
            tr.localScale = Vector3.one * escalaVR;
        }
        else
        {
            quizCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
        }
    }

    private void OnClickRespuesta(int index)
    {
        Debug.Log(index == quizActual.indiceCorrecto ? "¡Correcto!" : "Incorrecto.");
        CerrarQuiz();
    }

    public void CerrarQuiz() => quizCanvas.gameObject.SetActive(false);
}
