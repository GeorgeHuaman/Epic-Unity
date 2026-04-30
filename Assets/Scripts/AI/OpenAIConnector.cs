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
        string systemPrompt = @"Eres un arquitecto de laboratorios semi-modulares en Unity. 
        En lugar de construir pared por pared, usarás habitaciones completas (PRE-BUILT ROOMS).
        
        REGLAS DE CONSTRUCCIÓN (GRILLA DE 10 METROS):
        1. GRILLA: Todas las posiciones (pos_x, pos_z) DEBEN ser múltiplos de 10 (0, 10, 20, -10, etc.) para que las habitaciones encajen perfectamente.
        2. CONEXIÓN: Coloca las habitaciones una al lado de la otra. 
           Ejemplo: Si pones una 'Room_Entrance' en (0,0), la siguiente 'Room_Corridor' debería estar en (10,0) o (0,10).
        3. ROTACIÓN: Usa 'rot_y' (0, 90, 180, 270) para orientar las puertas de las habitaciones y que se conecten entre sí.
        4. COHERENCIA: Empieza siempre con una 'Room_Entrance'. Usa 'Room_Corridor' para conectar salas grandes como 'Room_Laboratory' o 'Room_Tomograph'.

        IDs DE HABITACIONES DISPONIBLES:
        - Básicas: 'Room_Entrance', 'Room_Corridor', 'Room_Hall'.
        - Especializadas: 'Room_Laboratory', 'Room_Laboratory_2', 'Room_Freezer', 'Room_Tomograph', 'Room_Tomograph_Control'.

        Responde SOLO con JSON siguiendo este formato:
        { 'sky_id': '...', 'elementos': [ { 'prefab_id': '...', 'pos_x': 0, 'pos_y': 0, 'pos_z': 0, 'rot_y': 0, 'data': {...} } ] }";

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
