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
    private string apiUrl = "https://api.openai.com/v1/chat/completions";

    public IEnumerator EnviarPromptALaIA(string promptDocente, System.Action<WorldConfig> callback)
    {
        string systemPrompt = @"Eres un arquitecto de niveles para Unity. 
        Responde SOLO con JSON. IDs de Skybox disponibles: 'sky_day', 'sky_mars', 'sky_night', 'sky_sunset'.
        IDs de Prefabs: 'npc_guia', 'puerta_quiz'.
        Formato: { 'sky_id': '...', 'elementos': [ { 'prefab_id': '...', 'pos_x': 0, 'pos_z': 0, 'data': {...} } ] }";

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
                // Extraemos el JSON de la respuesta de OpenAI
                JObject response = JObject.Parse(request.downloadHandler.text);
                string content = response["choices"][0]["message"]["content"].ToString();
                WorldConfig config = JsonConvert.DeserializeObject<WorldConfig>(content);
                callback(config);
            }
            else
            {
                Debug.LogError("Error en IA: " + request.error);
            }
        }
    }
}
