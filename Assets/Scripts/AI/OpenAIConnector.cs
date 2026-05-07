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
        string systemPrompt = @"Eres un arquitecto de niveles experto en Unity. Tu objetivo es diseñar escenarios. 

        REGLAS DE CARGA:
        1. TEMPLATE: Solo usa el campo 'template' si el usuario pide explícitamente un ambiente completo, laboratorio base o estructura predefinida. 
           - 'PFB_Lab': Laboratorio completo pre-construido.
           - 'linear': Pasillo procedural (requiere 'length', 'room_prefab' y 'side' en parameters).
        2. ELEMENTOS: Si el usuario pide objetos específicos sin pedir el laboratorio completo, deja 'template': '' y añade los objetos a la lista 'elementos'.
        3. NPC GUÍA: ID 'NPC_Guia_Tutor'. Datos: { 'texto': '...' }.
        4. PUERTA QUIZ: ID 'INT_Puerta_Quiz'. Datos: { 'pregunta': '...', 'respuesta_correcta': '...' }. Esta puerta bloquea el paso hasta que se responda correctamente.

        IDs DISPONIBLES:
        - Cielos (sky_id): 'sky_day', 'sky_mars', 'sky_night', 'sky_sunset'.
        - Templates: 'PFB_Lab', 'linear'.
        - Elementos (prefab_id): 'NPC_Guia_Tutor', 'INT_Puerta_Quiz', 'Room_Tomograph', 'Room_Corridor'.

        EJEMPLO (NPC y Puerta):
        {
          'sky_id': 'sky_day',
          'template': '',
          'elementos': [
            { 'prefab_id': 'NPC_Guia_Tutor', 'pos_x': 0, 'pos_y': 0, 'pos_z': 2, 'rot_y': 0, 'data': { 'texto': 'Bienvenido al examen' } },
            { 'prefab_id': 'INT_Puerta_Quiz', 'pos_x': 0, 'pos_y': 0, 'pos_z': 5, 'rot_y': 0, 'data': { 'pregunta': '¿Cuanto es 2+2?', 'respuesta_correcta': '4' } }
          ]
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
                    
                    callback(config);
                    }
                    catch (JsonException ex)
                    {
                    Debug.LogError("Error al parsear el JSON de la IA: " + ex.Message);
                    }
                    }
                    else
                    {
                    Debug.LogError("Error en IA: " + request.error);
                    }
                    }
                    }
                    }
