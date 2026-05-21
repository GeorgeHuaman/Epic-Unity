using UnityEngine;

public class AiLevelManager : MonoBehaviour
{
    public OpenAIConnector ai;
    public ProceduralLevelGenerator generator;

    public void GenerateLevel(string prompt)
    {
        StartCoroutine(ai.EnviarPromptALaIA(prompt, OnWorldGenerated));
    }

    void OnWorldGenerated(WorldConfig config)
    {
        ApplyWorldConfig(config);

        // Nos suscribimos al evento para spawnear elementos solo cuando el nivel esté listo
        generator.OnGenerationComplete = () => {
            if (NPCFinder.Instance != null)
            {
                NPCFinder.Instance.InstanciarElementos(config.elementos);
            }
            generator.OnGenerationComplete = null; 
        };

        generator.Generate();
        Debug.Log("Generación procedural iniciada...");
    }

    void ApplyWorldConfig(WorldConfig config)
    {
        if (config.parameters == null)
            return;

        generator.mainPathLength = config.parameters.length;
        generator.branchProbability = config.parameters.branch_probability;
        generator.forcedRoomPrefabName = config.parameters.room_prefab;
        generator.useCorridors = config.parameters.use_corridors;
    }
    }