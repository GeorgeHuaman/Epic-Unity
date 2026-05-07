using UnityEngine;
using System.IO;
using Newtonsoft.Json;

public class SaveManager : MonoBehaviour
{
    private string savePath;

    void Start()
    {
        // persistentDataPath es una carpeta segura que Unity crea automáticamente
        // Funciona en Windows, Mac, Android y iOS sin problemas de permisos.
        savePath = Path.Combine(Application.persistentDataPath, "ClasesDocentes");
        
        if (!Directory.Exists(savePath))
        {
            Directory.CreateDirectory(savePath);
        }
    }

    // 1. Guardar la clase (Usado por el Docente)
    public string GuardarClase(WorldConfig config)
    {
        // Generar un código aleatorio de 4 dígitos
        string codigoClase = "MEX-" + Random.Range(1000, 9999).ToString();
        string filePath = Path.Combine(savePath, codigoClase + ".json");

        // Convertir el objeto a JSON y guardarlo
        string json = JsonConvert.SerializeObject(config, Formatting.Indented);
        File.WriteAllText(filePath, json);

        Debug.Log("Clase guardada en: " + filePath);
        return codigoClase; // Devolvemos el código para mostrarlo en la UI
    }

    // 2. Cargar la clase (Usado por el Alumno)
    public WorldConfig CargarClase(string codigoClase)
    {
        string filePath = Path.Combine(savePath, codigoClase + ".json");

        if (File.Exists(filePath))
        {
            string json = File.ReadAllText(filePath);
            WorldConfig config = JsonConvert.DeserializeObject<WorldConfig>(json);
            return config;
        }
        else
        {
            Debug.LogError("Error: No se encontró la clase con el código " + codigoClase);
            return null;
        }
    }
}
