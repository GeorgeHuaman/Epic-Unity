using UnityEngine;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class WorldBuilder : MonoBehaviour
{
    public SkyboxManager skyboxManager; 
    public List<GameObject> prefabLibrary = new List<GameObject>(); 

    [ContextMenu("Populate Library from Folder")]
    public void PopulateLibrary()
    {
#if UNITY_EDITOR
        prefabLibrary.Clear();
        string[] guids = AssetDatabase.FindAssets("t:Prefab", new[] { "Assets/Prefabs/Library/Structure" });
        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (prefab != null)
            {
                prefabLibrary.Add(prefab);
            }
        }
        EditorUtility.SetDirty(this);
        Debug.Log($"Library populated with {prefabLibrary.Count} prefabs.");
#endif
    }

    public void ConstruirMundo(WorldConfig config)
    {
        if (skyboxManager != null && !string.IsNullOrEmpty(config.sky_id))
            skyboxManager.ChangeSkybox(config.sky_id);

        // Clear existing children
        for (int i = transform.childCount - 1; i >= 0; i--)
        {
            DestroyImmediate(transform.GetChild(i).gameObject);
        }

        if (config.elementos == null) return;

        foreach (var item in config.elementos)
        {
            GameObject prefab = prefabLibrary.Find(p => p.name == item.prefab_id);
            if (prefab == null)
            {
                // Try finding by ID if it's not the exact name
                prefab = prefabLibrary.Find(p => p.name.Contains(item.prefab_id));
            }

            if (prefab != null)
            {
                Vector3 posicion = new Vector3(item.pos_x, item.pos_y, item.pos_z);
                Quaternion rotacion = Quaternion.Euler(0, item.rot_y, 0);
                GameObject obj = Instantiate(prefab, posicion, rotacion, this.transform);
                obj.name = $"{prefab.name}_{item.pos_x}_{item.pos_z}";
                
                if (!string.IsNullOrEmpty(item.reasoning))
                {
                    Debug.Log($"<color=cyan>[IA Reasoning]</color> {item.reasoning} -> Colocado {prefab.name} en {posicion}");
                }

                if (obj.TryGetComponent<IConfigurable>(out var configurable))
{
                    configurable.Setup(item.data);
                }
            }
else
            {
                Debug.LogWarning($"Prefab not found in library: {item.prefab_id}");
            }
        }
    }
}

public interface IConfigurable
{
    void Setup(Dictionary<string, string> data);
}