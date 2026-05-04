using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

public class OpenAIConnector : MonoBehaviour
{
    public string apiKey = string.Empty;
    public string lastJsonReceived = string.Empty;
    private string apiUrl = "https://api.openai.com/v1/chat/completions";

    public IEnumerator EnviarPromptALaIA(string promptDocente, System.Action<WorldConfig> callback)
    {
        string systemPrompt = @"Eres un arquitecto de niveles experto en Unity. Tu objetivo es diseñar escenarios usando Plantillas (Templates) o cargando Niveles Base pre-construidos.

        PRIORIDAD DE CARGA:
        1. NIVELES BASE: Si el usuario pide un nivel estándar (como un laboratorio base), usa el campo 'template' con el nombre del nivel base. 
           - 'PFB_Lab': Laboratorio completo pre-construido.
        2. PLANTILLAS PROCEDURALES: Si el usuario pide una estructura personalizada, usa las plantillas:
           - 'linear': Crea un pasillo recto con habitaciones a los lados.
             Parámetros en 'parameters':
             - 'length': Segmentos de pasillo (10m c/u).
             - 'room_prefab': ID de la habitación ('Room_Tomograph').
             - 'side': 'left', 'right' o 'both'.

        IDs DE RECURSOS:
        - Niveles Base: 'PFB_Lab'.
        - Habitaciones: 'Room_Tomograph', 'Room_Corridor'.
        - Cielos: 'sky_day', 'sky_mars', 'sky_night', 'sky_sunset'.
        - Personajes: 'npc_guia'.

        EJEMPLO DE RESPUESTA PARA NIVEL BASE:
        Si el usuario pide 'Carga el laboratorio base', responde:
        {
          'sky_id': 'sky_day',
          'template': 'PFB_Lab',
          'parameters': {},
          'elementos': []
        }

        Responde SOLO con JSON siguiendo este formato:
        { 
          'sky_id': '...', 
          'template': '...', 
          'parameters': { 'key': 'value' },
          'elementos': [ { 'prefab_id': '...', 'pos_x': 0, 'pos_y': 0, 'pos_z': 0, 'rot_y': 0, 'data': {...} } ] 
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
                    JObject response = JObject.Parse(request.downloadHandler.text);
                    string content = response["choices"][0]["message"]["content"].ToString();
                    lastJsonReceived = content;
                    WorldConfig config = JsonConvert.DeserializeObject<WorldConfig>(content);
                    
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
