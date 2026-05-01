using System;
using System.Collections.Generic;

[Serializable]
public class WorldConfig
{
    public string sky_id; 
    public List<ElementoEscena> elementos;
}

[Serializable]
public class ElementoEscena
{
    public string reasoning; // Pensamiento de la IA para cada pieza
    public string prefab_id; 
    public float pos_x;
    public float pos_y; 
    public float pos_z;
    public float rot_y; 
    public Dictionary<string, string> data; 
}