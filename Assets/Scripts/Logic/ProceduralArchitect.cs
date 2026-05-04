using UnityEngine;
using System.Collections.Generic;

public class ProceduralArchitect : MonoBehaviour
{
    private WorldBuilder _builder;
    private WorldBuilder builder 
    {
        get {
            if (_builder == null) _builder = GetComponent<WorldBuilder>();
            return _builder;
        }
    }

    public void GenerateFromTemplate(WorldConfig config)
    {
        if (string.IsNullOrEmpty(config.template)) return;

        switch (config.template.ToLower())
        {
            case "linear":
                BuildLinear(config.parameters);
                break;
            default:
                Debug.LogWarning($"Template no reconocido: {config.template}");
                break;
        }
    }

    private void BuildLinear(Dictionary<string, string> parameters)
    {
        int length = 1;
        int.TryParse(GetParam(parameters, "length", "1"), out length);
        string roomPrefab = GetParam(parameters, "room_prefab", "Room_Tomograph");
        string side = GetParam(parameters, "side", "both");

        float segmentLength = 10f;
        float corridorZOffset = 0f;
        float sideOffset = 2.0f;

        for (int i = 0; i < length; i++)
        {
            PlaceRoom("Room_Corridor", i * segmentLength, 0, corridorZOffset, 90);

            if (side == "left" || side == "both")
            {
                PlaceRoom(roomPrefab, i * segmentLength, 0, sideOffset, 0);
            }
            if (side == "right" || side == "both")
            {
                PlaceRoom(roomPrefab, i * segmentLength, 0, -sideOffset, 180);
            }
        }
    }

    private void PlaceRoom(string id, float x, float y, float z, float rot)
    {
        if (builder == null || builder.prefabLibrary == null) return;

        GameObject prefab = builder.prefabLibrary.Find(p => p.name == id);
        if (prefab != null)
        {
            GameObject obj = Instantiate(prefab, new Vector3(x, y, z), Quaternion.Euler(0, rot, 0), builder.transform);
            obj.name = id;
        }
    }

    private string GetParam(Dictionary<string, string> dict, string key, string defaultValue)
    {
        if (dict != null && dict.ContainsKey(key)) return dict[key];
        return defaultValue;
    }
}
