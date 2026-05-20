using UnityEngine;
using System.IO;
using Newtonsoft.Json;

public class SaveManager : MonoBehaviour
{
    [Header("Referencias")]
    public CloudManager cloudManager;
    private string savePath;

    void Start()
    {
        // persistentDataPath es una carpeta segura que Unity crea automáticamente
        savePath = Path.Combine(Application.persistentDataPath, "ClasesDocentes");
        
        if (!Directory.Exists(savePath))
        {
            Directory.CreateDirectory(savePath);
        }
    }

    // --- GUARDADO LOCAL ---

    public string GuardarClaseLocal(WorldConfig config)
    {
        string codigoClase = "";
        string filePath = "";
        bool codigoValido = false;

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

        string json = JsonConvert.SerializeObject(config, Formatting.Indented);
        File.WriteAllText(filePath, json);

        Debug.Log("Clase guardada localmente en: " + filePath);
        return codigoClase; 
    }

    public WorldConfig CargarClaseLocal(string codigoClase)
    {
        string filePath = Path.Combine(savePath, codigoClase + ".json");

        if (File.Exists(filePath))
        {
            string json = File.ReadAllText(filePath);
            WorldConfig config = JsonConvert.DeserializeObject<WorldConfig>(json);
            
            // Sincronizar con el generador procedural
            if (config != null && !string.IsNullOrEmpty(config.ordenSala))
            {
                ProceduralLevelGenerator.Instance.LoadJsonFromFile(config.ordenSala);
            }
            
            return config;
        }
        return null;
    }

    // --- GUARDADO EN LA NUBE ---

    public void GuardarClaseEnNube(WorldConfig config, System.Action<string> onComplete)
    {
        if (cloudManager == null)
        {
            Debug.LogError("CloudManager no asignado en SaveManager");
            onComplete?.Invoke("ERROR");
            return;
        }

        StartCoroutine(cloudManager.GuardarClaseEnNube(config, (codigo) => {
            if (codigo != "ERROR")
            {
                // Opcional: Guardar una copia local también
                string json = JsonConvert.SerializeObject(config, Formatting.Indented);
                File.WriteAllText(Path.Combine(savePath, codigo + ".json"), json);
            }
            onComplete?.Invoke(codigo);
        }));
    }

    public void CargarClaseDeNube(string codigo, System.Action<WorldConfig> onComplete)
    {
        if (cloudManager == null)
        {
            Debug.LogError("CloudManager no asignado en SaveManager");
            onComplete?.Invoke(null);
            return;
        }

        StartCoroutine(cloudManager.CargarClaseDeNube(codigo, (config) => {
            if (config != null)
            {
                // Sincronizar con el generador procedural
                if (!string.IsNullOrEmpty(config.ordenSala))
                {
                    ProceduralLevelGenerator.Instance.LoadJsonFromFile(config.ordenSala);
                }
                
                // Opcional: Guardar copia local para caché
                string json = JsonConvert.SerializeObject(config, Formatting.Indented);
                File.WriteAllText(Path.Combine(savePath, codigo + ".json"), json);
            }
            onComplete?.Invoke(config);
        }));
    }

    public string[] ObtenerHistorialDeClases()
    {
        if (!Directory.Exists(savePath)) return new string[0];
        string[] files = Directory.GetFiles(savePath, "*.json");
        string[] codigos = new string[files.Length];
        for (int i = 0; i < files.Length; i++)
        {
            codigos[i] = Path.GetFileNameWithoutExtension(files[i]);
        }
        return codigos;
    }
}
