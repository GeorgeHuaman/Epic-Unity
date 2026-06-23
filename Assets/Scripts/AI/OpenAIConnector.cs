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
              'room_prefab': string, (Dejar vacío '' para que el generador use salas aleatorias. Solo usar si se pide una sala específica para todo el nivel)
              'side': string,
              'maze_intensity': float,
              'enemy_density': int,
              'use_corridors': bool, (Si es false, las salas se conectan directamente sin pasillos)
              'spawn_victory': bool (Si es true, se añade un punto de victoria al final del nivel)
            }

            DESCRIPCIÓN:
            - length:
              Cantidad total de SALAS del nivel (incluyendo inicio y fin).
              Valores recomendados:
              3 = nivel muy corto
              5 = nivel corto
              10 = mediano
              20+ = largo

            - spawn_victory:
              Define si se debe crear un punto de victoria o meta al final del nivel.
              Debe ser true si el usuario menciona 'victoria', 'ganar', 'meta', 'finalizar nivel', o similares.

            - use_corridors:
              Define si se deben usar pasillos para conectar las salas.
              Por defecto es true.
              Si el usuario pide explícitamente 'sin pasillos', ponlo en false.

            - branch_probability:
              Probabilidad de crear caminos secundarios.
              Rango:
              0.0 a 1.0

              0.1 = casi lineal
              0.5 = moderado
              0.8 = muy laberíntico

            - room_prefab:
              Nombre EXACTO del prefab de sala.
              REGLA IMPORTANTE: Déjalo vacío ('') por defecto para permitir variedad de salas. 
              Solo asígnale un valor si el usuario pide explícitamente que TODAS las salas sean de un tipo (ej: 'todas las salas tipo 01').

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
            añádelos TODOS en la lista ""elementos"". NO omitas ninguno.

            - room_index:
              Índice de la sala donde debe aparecer el elemento (basado en el orden de salas).
              DEBE ser un número entre 0 y (length - 1).

              0 = Sala Inicial
              1 = Sala 2
              2 = Sala 3
              ...
              length - 1 = Sala Final (Última sala)

              Si el usuario dice ""sala 3"", usa room_index: 2.
              Si el usuario dice ""al inicio"", usa room_index: 0.
              Si el usuario dice ""al final"" o ""en la última sala"", usa room_index: (length - 1).

            4. NPC (GUÍA o QUIZ):
            ID:
            'NPCs'

            Si el usuario pide un guía que hable, usa mode: 'guide'.
            Si el usuario pide un NPC que haga preguntas o sea un examen, usa mode: 'quiz'.

            DATA PARA 'guide':
            {
              'mode': 'guide',
              'texto': 'Mensaje que dirá el NPC'
            }

            DATA PARA 'quiz':
            {
              'mode': 'quiz',
              'pregunta': 'Texto de la pregunta',
              'opciones': 'opción1;opción2;opción3;opción4', (Separadas por punto y coma)
              'respuesta_correcta': 'int' (Índice de la respuesta correcta como STRING, empezando en '0')
            }

            FORMATO DE ELEMENTO NPC:
            {
              'prefab_id': 'NPCs',
              'room_index': int,
              'data': { ... }
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
            - 'NPCs'
            - 'INT_Puerta_Quiz'
            - 'Room_Tomograph'
            - 'Room_Corridor'
            - 'Victory'

            IMPORTANTE:
            - Responde SOLO JSON válido.
            - NO expliques nada.
            - NO uses markdown (```json ... ```).
            - NO envuelvas el objeto en un array [ ]. Responde solo el objeto { }.
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
                'room_prefab': '',
                'side': 'both',
                'maze_intensity': 0.6,
                'enemy_density': 3,
                'use_corridors': true,
                'spawn_victory': true
              },
              'elementos':
              [
                {
                  'prefab_id': 'NPCs',
                  'room_index': 0,
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
                    
                    // Si la IA responde con un array [ { ... } ], tomamos el primer objeto
                    if (content.Trim().StartsWith("[") && content.Trim().EndsWith("]"))
                    {
                        Debug.LogWarning("[OpenAIConnector] La IA devolvió un array en lugar de un objeto. Extrayendo el primer elemento.");
                        JArray array = JArray.Parse(content);
                        if (array.Count > 0)
                        {
                            content = array[0].ToString();
                        }
                    }

                    // Parseamos manualmente para asegurar que no perdemos elementos por tipos de datos
                    JObject jsonObj = JObject.Parse(content);
                    WorldConfig config = jsonObj.ToObject<WorldConfig>();
                    
                    if (config != null && config.elementos != null)
                    {
                        // Sincronizamos elementos desde el JObject por si acaso
                        JArray elementosArray = (JArray)jsonObj["elementos"];
                        if (elementosArray != null && elementosArray.Count != config.elementos.Count)
                        {
                            Debug.LogWarning($"[OpenAIConnector] Mismatch detectado: JArray tiene {elementosArray.Count} y List tiene {config.elementos.Count}. Reintentando mapeo manual.");
                            config.elementos = new List<ElementoEscena>();
                            foreach (var item in elementosArray)
                            {
                                ElementoEscena el = item.ToObject<ElementoEscena>();
                                // Aseguramos que 'data' se mapee correctamente
                                el.data = new Dictionary<string, string>();
                                JObject dataObj = (JObject)item["data"];
                                if (dataObj != null)
                                {
                                    foreach (var property in dataObj.Properties())
                                    {
                                        el.data[property.Name] = property.Value.ToString();
                                    }
                                }
                                config.elementos.Add(el);
                            }
                        }

                        Debug.Log($"[OpenAIConnector] Finalizado. Procesados {config.elementos.Count} elementos.");
                        foreach(var el in config.elementos)
                        {
                            Debug.Log($"  -> NPC en sala {el.room_index}");
                        }
                    }
                    
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
