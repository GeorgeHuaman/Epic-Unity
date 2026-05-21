using UnityEngine;
using TMPro;
using Newtonsoft.Json.Linq; // Necesario para leer la lista dinámica de Firebase

public class AnalyticsDashboard : MonoBehaviour
{
    public CloudManager cloudManager;
    public TMP_InputField inputCodigoConsulta; // Donde el docente pone el código que quiere revisar
    public TMP_Text textoResultados; // Un campo de texto grande con scroll

    public void ConsultarResultados()
    {
        string codigo = inputCodigoConsulta.text.Trim().ToUpper();
        textoResultados.text = "Descargando datos...";

        StartCoroutine(cloudManager.ObtenerMetricasClase(codigo, (jsonRespuesta) => {
            if (string.IsNullOrEmpty(jsonRespuesta) || jsonRespuesta == "null")
            {
                textoResultados.text = "No hay datos para esta clase aún.";
                return;
            }

            // Parsear el JSON dinámico de Firebase
            JObject metricasObj = JObject.Parse(jsonRespuesta);
            string formatoLegible = "RESULTADOS DE CLASE: " + codigo + "\n\n";

            foreach (var metrica in metricasObj)
            {
                var datos = metrica.Value;
                string alumno = datos["nombre_alumno"].ToString();
                string evento = datos["tipo_evento"].ToString();
                string detalle = datos["detalle"].ToString();

                formatoLegible += $"[{alumno}] - {evento}: {detalle}\n";
            }

            textoResultados.text = formatoLegible;
        }));
    }
}
