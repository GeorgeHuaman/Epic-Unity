using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

public class OpenAIConnector : MonoBehaviour
{
    // TODO: Move the API key to a secure location (e.g., environment variable or local config file)
    public string apiKey = string.Empty;
    public string lastJsonReceived = string.Empty;
    private string apiUrl = "https://api.openai.com/v1/chat/completions";

    public IEnumerator EnviarPromptALaIA(string promptDocente, System.Action<WorldConfig> callback)
    {
        string systemPrompt = @"Eres un arquitecto de precisión en Unity. Tu misión es crear layouts modulares donde los pasillos se conecten de forma lógica a los salones, evitando conexiones débiles en las esquinas.

    DATOS TÉCNICOS:
    - Piezas (Pivote en esquina +X, +Z): 'Struct_Floor_1x1', 'Struct_Floor_2x2', 'Struct_Floor_3x3', 'Struct_Floor_4x4'.
    - REGLA DE POSICIÓN: Para cubrir [Xmin, Zmin] a [Xmax, Zmax], la posición es (Xmax, Zmax).

    REGLAS DE CONEXIÓN (IMPORTANTE):
    1. RELLENAR EL COSTADO: No basta con que el pasillo toque un vértice. El pasillo debe compartir un segmento de línea con el salón.
    2. CENTRADO ARQUITECTÓNICO: 
       - Los pasillos deben estar CENTRADOS en el lado del salón al que se conectan, o estar alineados de forma que cubran una sección significativa del muro.
       - Ejemplo: Si un salón de 8x8 va de X=0 a 8, y el pasillo mide 4m de ancho, sitúa el pasillo entre X=2 y X=6 para que esté centrado.
    3. PRECISIÓN DE 0.01 CM: Para cumplir el deseo del usuario de 'estar a 0.01cm', usa un margen de 0.0001 unidades en la coordenada de contacto.
       Ejemplo: Si el salón termina en X=8, el pasillo empieza en X=8.0001.
    4. TILING: Divide áreas rectangulares en las piezas cuadradas más grandes posibles.

    PROCESO DE DISEÑO:
    - Paso 1: Definir salones.
    - Paso 2: Trazar pasillos centrados en las caras de los salones.
    - Paso 3: Calcular coordenadas exactas (pos_x, pos_z) para cada pieza del tiling.

    Responde en JSON:
    {
      ""sky_id"": ""..."",
      ""elementos"": [
        { 
          ""reasoning"": ""..."",
          ""prefab_id"": ""..."", 
          ""pos_x"": ..., 
          ""pos_z"": ..., 
          ""pos_y"": 0, 
          ""rot_y"": 0 
        }
      ]
    }";

        var requestBody = new
        {
            model = "gpt-4o",
            messages = new[] {
                new { role = "system", content = systemPrompt },
                new { role = "user", content = promptDocente }
            },
            response_format = new { type = "json_object" }
        };

        string jsonPayload = JsonConvert.SerializeObject(requestBody);

        using (UnityWebRequest request = new UnityWebRequest(apiUrl, "POST"))
        {
            byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonPayload);
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");
            request.SetRequestHeader("Authorization", "Bearer " + apiKey);

            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                try
                {
                    // Extraemos el JSON de la respuesta de OpenAI
                    JObject response = JObject.Parse(request.downloadHandler.text);
                    string content = response["choices"][0]["message"]["content"].ToString();
                    lastJsonReceived = content;
                    WorldConfig config = JsonConvert.DeserializeObject<WorldConfig>(content);
                    
                    // Si todo salió bien, detenemos la animación de carga (podemos poner un mensaje de éxito o vacío)
                    ManagerUI.Instance.StopTyping("¡Hecho!"); 
                    callback(config);
                }
                catch (JsonException ex)
                {
                    Debug.LogError("Error al parsear el JSON de la IA: " + ex.Message);
                    ManagerUI.Instance.StopTyping("No pude entenderte, explicalo mejor");
                }
            }
            else
            {
                Debug.LogError("Error en IA: " + request.error);
                ManagerUI.Instance.StopTyping("Error de conexión con la IA.");
            }
        }
    }
}
