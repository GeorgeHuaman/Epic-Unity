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

        generator.Generate();
        Debug.Log("1");

        SpawnSceneElements(config);
    }

    void ApplyWorldConfig(WorldConfig config)
    {
        if (config.parameters == null)
            return;

        generator.mainPathLength =
            config.parameters.length;

        generator.branchProbability =
            config.parameters.branch_probability;

        generator.forcedRoomPrefabName =
            config.parameters.room_prefab;
    }

    void SpawnSceneElements(WorldConfig config)
    {
        foreach (var e in config.elementos)
        {
            Debug.Log("Spawn: " + e.prefab_id);

            // aquí haces instantiate
        }
    }
}
