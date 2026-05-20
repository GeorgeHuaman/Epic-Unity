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
        string systemPrompt = @"Eres un arquitecto de niveles experto en Unity especializado en generación procedural.

            Tu objetivo es crear configuraciones JSON para construir escenarios en Unity.

            REGLAS GENERALES:

            1. TEMPLATE:
            - Usa 'template': 'linear' cuando el usuario pida:
              - niveles procedurales
              - salas (rooms)
              - pasillos
              - laberintos
              - estructuras largas
              - mapas dinámicos
              - hospitales
              - laboratorios
              - dungeons

            - Usa 'template': 'PFB_Lab' SOLO si el usuario pide explícitamente un laboratorio completo pre-construido.

            - Si el usuario solo pide objetos específicos, usa:
              'template': ''

            2. GENERADOR PROCEDURAL:
            El template 'linear' usa un ProceduralLevelGenerator.

            PARÁMETROS DISPONIBLES:
            {
              'length': int, (Este es el número TOTAL de salas que tendrá el nivel)
              'branch_probability': float,
              'room_prefab': string,
              'side': string,
              'maze_intensity': float,
              'enemy_density': int,
              'use_corridors': bool (Si es false, las salas se conectan directamente sin pasillos)
            }

            DESCRIPCIÓN:
            - length:
              Cantidad total de SALAS del nivel (incluyendo inicio y fin).
              Valores recomendados:
              3 = nivel muy corto
              5 = nivel corto
              10 = mediano
              20+ = largo

            - use_corridors:
              Define si se deben usar pasillos para conectar las salas.
              Por defecto es true.
              Si el usuario pide explícitamente ""sin pasillos"", ponlo en false.

            - branch_probability:
              Probabilidad de crear caminos secundarios.
              Rango:
              0.0 a 1.0

              0.1 = casi lineal
              0.5 = moderado
              0.8 = muy laberíntico

            - room_prefab:
              Nombre EXACTO del prefab de sala.

            SALAS DISPONIBLES:
            - 'Sala_01'
            - 'Sala_02'
            - 'Sala_03'

            PASILLOS DISPONIBLES:
            - 'Pasillo_01'
            - 'Pasillo_02'
            - 'Pasillo_03'

            - side:
              Puede ser:
              'left'
              'right'
              'both'
              'none'

            - maze_intensity:
              Qué tan complejo debe sentirse el nivel.
              Rango 0.0 a 1.0

            - enemy_density:
              Cantidad aproximada de enemigos.
              Rango:
              0 a 10

            3. ELEMENTOS:
            Si el usuario pide NPCs, puertas, quizzes u objetos especiales,
            añádelos en 'elementos'.

            4. NPC GUÍA:
            ID:
            'NPC_Guia_Tutor'

            DATA:
            {
              'texto': '...'
            }

            5. PUERTA QUIZ:
            ID:
            'INT_Puerta_Quiz'

            DATA:
            {
              'pregunta': '...',
              'respuesta_correcta': '...'
            }

            IDs DISPONIBLES:

            SKYBOX:
            - 'sky_day' (para día, sol)
            - 'sky_mars' (para Marte, rojo, desierto)
            - 'sky_night' (para noche, oscuridad)
            - 'sky_sunset' (para atardecer)

            TEMPLATES:
            - 'PFB_Lab'
            - 'linear'

            ELEMENTOS:
            - 'NPC_Guia_Tutor'
            - 'INT_Puerta_Quiz'
            - 'Room_Tomograph'
            - 'Room_Corridor'

            IMPORTANTE:
            - Responde SOLO JSON válido.
            - NO expliques nada.
            - NO uses markdown.
            - NO uses texto fuera del JSON.
            - Los números NO deben ir entre comillas.

            FORMATO:

            {
              'sky_id': 'sky_day',

              'template': 'linear',

              'parameters':
              {
                'length': 15,
                'branch_probability': 0.7,
                'room_prefab': 'Sala_02',
                'side': 'both',
                'maze_intensity': 0.6,
                'enemy_density': 3
              },

              'elementos':
              [
                {
                  'prefab_id': 'NPC_Guia_Tutor',
                  'pos_x': 0,
                  'pos_y': 0,
                  'pos_z': 2,
                  'rot_y': 0,

                  'data':
                  {
                    'texto': 'Bienvenido al laboratorio'
                  }
                }
              ]
            }

        REGLA CRÍTICA:

        Si el usuario especifica valores numéricos explícitos
        para:
        - length
        - branch_probability
        - enemy_density
        - maze_intensity

        DEBES usar EXACTAMENTE esos valores.

        NO los modifiques.
        NO los aumentes.
        NO los reduzcas.
        NO los reinterpretas según palabras como:
        'enorme', 'laberíntico', 'complejo', etc.

        Los números escritos por el usuario tienen prioridad absoluta.";

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
