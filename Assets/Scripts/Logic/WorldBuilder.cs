using UnityEngine;
using System.Collections.Generic;

public class WorldBuilder : MonoBehaviour
{
    public SkyboxManager skyboxManager;
    public List<GameObject> prefabLibrary;
    public ProceduralLevelGenerator proceduralGenerator;

    void Awake()
    {
    }

    public void ConstruirMundo(WorldConfig config)
    {
        if (skyboxManager != null)
            skyboxManager.ChangeSkybox(config.sky_id);

        // 2. Limpiar mundo anterior
        List<GameObject> children = new List<GameObject>();
        foreach (Transform child in transform) children.Add(child.gameObject);

        foreach (GameObject child in children)
        {
            if (Application.isPlaying)
                Destroy(child);
            else
                DestroyImmediate(child);
        }

        bool proceduralGenerated = false;

        // TEMPLATE PROCEDURAL
        if (config.template == "linear")
        {
            if (proceduralGenerator != null)
            {
                ApplyProceduralParameters(config);

                proceduralGenerator.Generate();

                proceduralGenerated = true;

                Debug.Log("Nivel procedural generado.");
            }
        }

        // TEMPLATE PREFAB
        if (!proceduralGenerated &&
            !string.IsNullOrEmpty(config.template))
        {
            GameObject baseLevelPrefab =
                prefabLibrary.Find(p => p != null && p.name == config.template);

            if (baseLevelPrefab != null)
            {
                Instantiate(baseLevelPrefab,
                    Vector3.zero,
                    Quaternion.identity,
                    transform);

                Debug.Log($"Cargado nivel base desde prefab: {config.template}");
            }
        }
        
        // ELEMENTOS
        if (config.elementos != null)
        {
            foreach (var item in config.elementos)
            {
                GameObject prefab =
                    prefabLibrary.Find(p => p != null && p.name == item.prefab_id);

                if (prefab != null)
                {
                    Vector3 posicion =
                        new Vector3(item.pos_x, item.pos_y, item.pos_z);

                    Quaternion rotacion =
                        Quaternion.Euler(0, item.rot_y, 0);

                    GameObject obj =
                        Instantiate(prefab,
                            posicion,
                            rotacion,
                            this.transform);

                    if (obj.TryGetComponent<IConfigurable>(out var configurable))
                    {
                        configurable.Setup(item.data);
                    }
                }
            }
        }

    }

    void ApplyProceduralParameters(WorldConfig config)
    {
        if (config.parameters == null)
            return;

        proceduralGenerator.mainPathLength =
            config.parameters.length;

        proceduralGenerator.branchProbability =
            config.parameters.branch_probability;

        proceduralGenerator.forcedRoomPrefabName =
            config.parameters.room_prefab;
    }

}
public interface IConfigurable
{
    void Setup(Dictionary<string, string> data);
}