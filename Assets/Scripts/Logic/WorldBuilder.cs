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

                // Nos suscribimos al evento para spawnear elementos solo cuando el nivel esté listo
                proceduralGenerator.OnGenerationComplete = () => {
                    SpawnElementos(config);
                    proceduralGenerator.OnGenerationComplete = null; 
                };

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

            SpawnElementos(config);
        }
        else if (!proceduralGenerated)
        {
            SpawnElementos(config);
        }

    }

    void SpawnElementos(WorldConfig config)
    {
        if (config.elementos != null && NPCFinder.Instance != null)
        {
            NPCFinder.Instance.InstanciarElementos(config.elementos);
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

        proceduralGenerator.useCorridors =
            config.parameters.use_corridors;

        proceduralGenerator.spawnVictoryAtEnd =
            config.parameters.spawn_victory;
    }

}
public interface IConfigurable
{
    void Setup(Dictionary<string, string> data);
}