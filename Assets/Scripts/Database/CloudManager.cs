using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using Newtonsoft.Json;
using System.Text;

public class CloudManager : MonoBehaviour
{
    [Header("Configuración Firebase")]
    // URL de la base de datos (Nota: Asegúrate de usar la URL de la Database, no de la Consola)
    public string databaseURL = "https://edtech-perumex-default-rtdb.firebaseio.com/";

    // 1. Guardar en la nube (Ruta Docente)
    // Se usa IEnumerator para manejar la petición de forma asíncrona sin bloquear el juego
    public IEnumerator GuardarClaseEnNube(WorldConfig config, System.Action<string> onComplete)
    {
        // Generamos un código único aleatorio para identificar esta configuración en la base de datos
        string codigoClase = "MEX-" + Random.Range(1000, 9999).ToString();

        // Serialización: Convertimos el objeto C# 'config' a una cadena de texto en formato JSON
        string json = JsonConvert.SerializeObject(config);

        // Construcción de la URL: Firebase REST API requiere la ruta del nodo terminada en .json
        string url = databaseURL + "clases/" + codigoClase + ".json";

        // Usamos el verbo HTTP "PUT" para crear o sobrescribir el dato en la ruta exacta definida
        using (UnityWebRequest request = new UnityWebRequest(url, "PUT"))
        {
            // Convertimos el string JSON a un arreglo de bytes (UTF8) para el envío
            byte[] bodyRaw = Encoding.UTF8.GetBytes(json);
            
            // Asignamos los manejadores para subir los datos y recibir la respuesta
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            
            // Especificamos al servidor que estamos enviando contenido de tipo JSON
            request.SetRequestHeader("Content-Type", "application/json");

            // Enviamos la petición y esperamos a que el servidor responda
            yield return request.SendWebRequest();

            // Verificamos si la operación fue exitosa
            if (request.result == UnityWebRequest.Result.Success)
            {
                Debug.Log("Clase subida a la nube exitosamente.");
                // Ejecutamos el callback pasando el código generado para mostrarlo en la UI
                onComplete(codigoClase);
            }
            else
            {
                Debug.LogError("Error al subir a la nube: " + request.error);
                onComplete("ERROR");
            }
        }
    }

    // 2. Descargar de la nube (Ruta Alumno)
    // Recibe el código de la clase y devuelve el objeto WorldConfig mediante un callback
    public IEnumerator CargarClaseDeNube(string codigoClase, System.Action<WorldConfig> onComplete)
    {
        // Apuntamos a la ubicación exacta del archivo .json asociado al código
        string url = databaseURL + "clases/" + codigoClase + ".json";

        // Usamos el método Get estándar para descargar información
        using (UnityWebRequest request = UnityWebRequest.Get(url))
        {
            // Esperamos la respuesta del servidor
            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                // Obtenemos el cuerpo de la respuesta en formato texto
                string jsonResponse = request.downloadHandler.text;

                // Validación de Firebase: Si el código no existe en la DB, el servidor devuelve el string "null"
                if (jsonResponse == "null")
                {
                    onComplete(null);
                }
                else
                {
                    // Deserialización: Convertimos el texto JSON recibido de vuelta a un objeto WorldConfig
                    WorldConfig config = JsonConvert.DeserializeObject<WorldConfig>(jsonResponse);
                    onComplete(config);
                }
            }
            else
            {
                Debug.LogError("Error al descargar la clase: " + request.error);
                onComplete(null);
            }
        }
    }
}
