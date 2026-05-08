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
        string codigoClase = "";
        string filePath = "";
        bool codigoValido = false;

        // Bucle para asegurar que el código no exista previamente
        int intentos = 0;
        while (!codigoValido && intentos < 100)
        {
            codigoClase = "MEX-" + Random.Range(1000, 9999).ToString();
            filePath = Path.Combine(savePath, codigoClase + ".json");

            if (!File.Exists(filePath))
            {
                codigoValido = true;
            }
            intentos++;
        }

        // Convertir el objeto a JSON y guardarlo
        string json = JsonConvert.SerializeObject(config, Formatting.Indented);
        File.WriteAllText(filePath, json);

        Debug.Log("Clase guardada en: " + filePath);
        return codigoClase; 
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

    // 3. Obtener lista de códigos guardados (Para el Historial)
    public string[] ObtenerHistorialDeClases()
    {
        if (!Directory.Exists(savePath)) return new string[0];

        // Obtener todos los archivos .json en la carpeta
        string[] files = Directory.GetFiles(savePath, "*.json");
        string[] codigos = new string[files.Length];

        for (int i = 0; i < files.Length; i++)
        {
            codigos[i] = Path.GetFileNameWithoutExtension(files[i]);
        }

        return codigos;
    }
}
