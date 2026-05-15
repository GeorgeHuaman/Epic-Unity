using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class WorldConfig
{
    public string sky_id;
    public string template;

    public WorldParameters parameters;

    public List<ElementoEscena> elementos;
}

[Serializable]
public class ElementoEscena
{
    public string prefab_id; // "npc_guia", "puerta_quiz", etc.
    public float pos_x;
    public float pos_y;
    public float pos_z;
    public float rot_y; 
    public Dictionary<string, string> data; // Para textos de diálogos o preguntas

}
[Serializable]
public class WorldParameters
{
    public int length = 10;

    public float branch_probability = 0.3f;

    public string room_prefab = "";

    public string side = "both";

    public float maze_intensity = 0.5f;

    public int enemy_density = 0;
}