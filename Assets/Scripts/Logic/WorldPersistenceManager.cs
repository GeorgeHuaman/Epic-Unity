using UnityEngine;
using System.IO;
using Newtonsoft.Json;
using System;
using System.Runtime.InteropServices;

#if !UNITY_EDITOR && UNITY_STANDALONE_WIN
[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
public class OpenFileName
{
    public int structSize = 0;
    public IntPtr dlgOwner = IntPtr.Zero;
    public IntPtr instance = IntPtr.Zero;
    public string filter = null;
    public string customFilter = null;
    public int maxCustFilter = 0;
    public int filterIndex = 0;
    public string file = null;
    public int maxFile = 0;
    public string fileTitle = null;
    public int maxFileTitle = 0;
    public string initialDir = null;
    public string title = null;
    public int flags = 0;
    public short fileOffset = 0;
    public short fileExtension = 0;
    public string defExt = null;
    public IntPtr custData = IntPtr.Zero;
    public IntPtr hook = IntPtr.Zero;
    public string templateName = null;
    public IntPtr reservedPtr = IntPtr.Zero;
    public int reservedInt = 0;
    public int flagsEx = 0;
}

public class WinDialogs
{
    [DllImport("comdlg32.dll", SetLastError = true, CharSet = CharSet.Auto)]
    public static extern bool GetOpenFileName([In, Out] OpenFileName ofn);

    [DllImport("comdlg32.dll", SetLastError = true, CharSet = CharSet.Auto)]
    public static extern bool GetSaveFileName([In, Out] OpenFileName ofn);
}
#endif

public class WorldPersistenceManager : MonoBehaviour
{
    [Header("Referencias")]
    public OpenAIConnector connector;
    public WorldBuilder builder;

    [ContextMenu("Exportar Último JSON")]
    public void ExportarUltimoJSON()
    {
        // Usamos la configuración actual del UIManager, que ya incluye el ordenSala (procedural)
        if (UIManager.Instance == null || UIManager.Instance.configuracionActual == null)
        {
            Debug.LogWarning("No hay un mundo generado para exportar.");
            return;
        }

        string path = "";

    #if UNITY_EDITOR
        path = UnityEditor.EditorUtility.SaveFilePanel("Guardar Configuración de Mundo", "", "mundo_generado.json", "json");
    #elif UNITY_STANDALONE_WIN
        path = SaveFileWindows("Guardar Configuración de Mundo", "mundo_generado.json", "JSON Files\0*.json\0All Files\0*.*\0\0");
    #else
        path = Path.Combine(Application.persistentDataPath, "mundo_generado.json");
    #endif

        if (!string.IsNullOrEmpty(path))
        {
            try
            {
                // Serializamos la configuración actual con formato legible
                string jsonToExport = JsonConvert.SerializeObject(UIManager.Instance.configuracionActual, Formatting.Indented);
                File.WriteAllText(path, jsonToExport);
                Debug.Log($"<color=cyan>JSON exportado correctamente en: {path}</color>");
            }
            catch (Exception ex)
            {
                Debug.LogError("Error al guardar el archivo: " + ex.Message);
            }
        }
    }

    [ContextMenu("Importar y Generar Mundo")]
    public void ImportarYGenerarMundo()
    {
        string path = "";

#if UNITY_EDITOR
        path = UnityEditor.EditorUtility.OpenFilePanel("Seleccionar JSON de Mundo", "", "json");
#elif UNITY_STANDALONE_WIN
        path = OpenFileWindows("Seleccionar JSON de Mundo", "JSON Files\0*.json\0All Files\0*.*\0\0");
#else
        path = Path.Combine(Application.persistentDataPath, "mundo_generado.json");
#endif

        if (!string.IsNullOrEmpty(path))
        {
            if (File.Exists(path))
            {
                try
                {
                    string jsonContent = File.ReadAllText(path);
                    WorldConfig config = JsonConvert.DeserializeObject<WorldConfig>(jsonContent);

                    if (builder != null)
                    {
                        builder.ConstruirMundo(config);
                        Debug.Log("<color=green>Mundo cargado y generado desde archivo con éxito.</color>");
                    }
                    else
                    {
                        Debug.LogError("WorldBuilder no asignado en PersistenceManager.");
                    }
                }
                catch (Exception ex)
                {
                    Debug.LogError("Error al importar el JSON: " + ex.Message);
                }
            }
            else if (Application.isEditor == false)
            {
                Debug.LogError("El archivo seleccionado no existe: " + path);
            }
        }
    }

#if !UNITY_EDITOR && UNITY_STANDALONE_WIN
    private string OpenFileWindows(string title, string filter)
    {
        OpenFileName ofn = new OpenFileName();
        ofn.structSize = Marshal.SizeOf(ofn);
        ofn.filter = filter;
        ofn.file = new string(new char[256]);
        ofn.maxFile = ofn.file.Length;
        ofn.fileTitle = new string(new char[64]);
        ofn.maxFileTitle = ofn.fileTitle.Length;
        ofn.initialDir = Application.dataPath;
        ofn.title = title;
        ofn.defExt = "json";
        ofn.flags = 0x00080000 | 0x00001000 | 0x00000800 | 0x00000200 | 0x00000008; // OFN_EXPLORER | OFN_FILEMUSTEXIST | OFN_PATHMUSTEXIST

        if (WinDialogs.GetOpenFileName(ofn))
        {
            return ofn.file;
        }
        return string.Empty;
    }

    private string SaveFileWindows(string title, string defaultName, string filter)
    {
        OpenFileName ofn = new OpenFileName();
        ofn.structSize = Marshal.SizeOf(ofn);
        ofn.filter = filter;
        ofn.file = defaultName.PadRight(256, '\0');
        ofn.maxFile = ofn.file.Length;
        ofn.fileTitle = new string(new char[64]);
        ofn.maxFileTitle = ofn.fileTitle.Length;
        ofn.initialDir = Application.dataPath;
        ofn.title = title;
        ofn.defExt = "json";
        ofn.flags = 0x00080000 | 0x00000002 | 0x00000008; // OFN_EXPLORER | OFN_OVERWRITEPROMPT

        if (WinDialogs.GetSaveFileName(ofn))
        {
            return ofn.file.Replace("\0", "");
        }
        return string.Empty;
    }
#endif
}
