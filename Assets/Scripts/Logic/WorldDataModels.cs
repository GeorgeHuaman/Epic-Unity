using System;
using System.Collections.Generic;

[Serializable]
public class WorldConfig
{
    public string sky_id; // ID del skybox que elegirá la IA
    public List<ElementoEscena> elementos;
}

[Serializable]
public class ElementoEscena
{
    public string prefab_id; // "npc_guia", "puerta_quiz", etc.
    public float pos_x;
    public float pos_z;
    public Dictionary<string, string> data; // Para textos de diálogos o preguntas
}
