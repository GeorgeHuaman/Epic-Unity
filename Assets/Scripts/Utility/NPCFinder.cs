using UnityEngine;
using System.Collections.Generic;

public class NPCFinder : MonoBehaviour
{
    public static NPCFinder Instance { get; private set; }

    [Header("Posiciones Registradas")]
    public List<Vector3> npcPositions = new List<Vector3>();

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// Limpia la lista de posiciones registradas.
    /// </summary>
    public void LimpiarPosiciones()
    {
        npcPositions.Clear();
    }

    /// <summary>
    /// Registra una nueva posición de NPC en la lista.
    /// </summary>
    public void RegistrarPosicionNPC(Vector3 posicion)
    {
        npcPositions.Add(posicion);
    }

    [ContextMenu("Debug Log NPC Positions")]
    public void LogPosiciones()
    {
        for (int i = 0; i < npcPositions.Count; i++)
        {
            Debug.Log($"NPC {i}: {npcPositions[i]}");
        }
    }
}