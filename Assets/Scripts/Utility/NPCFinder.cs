using UnityEngine;
using System.Collections.Generic;

public class NPCFinder : MonoBehaviour
{
    public static NPCFinder Instance { get; private set; }

    [Header("Posiciones Registradas")]
    public Dictionary<int, Vector3> npcPositionsByRoom = new Dictionary<int, Vector3>();
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
        npcPositionsByRoom.Clear();
    }

    /// <summary>
    /// Registra una nueva posición de NPC asociada a un índice de sala.
    /// </summary>
    public void RegistrarPosicionNPC(int roomIndex, Vector3 posicion)
    {
        npcPositions.Add(posicion);
        if (!npcPositionsByRoom.ContainsKey(roomIndex))
        {
            npcPositionsByRoom.Add(roomIndex, posicion);
        }
    }

    /// <summary>
    /// Obtiene la posición de un NPC para una sala específica.
    /// </summary>
    public Vector3? ObtenerPosicionEnSala(int roomIndex)
    {
        if (npcPositionsByRoom.TryGetValue(roomIndex, out Vector3 pos))
        {
            return pos;
        }
        return null;
    }

    [ContextMenu("Debug Log NPC Positions")]
    public void LogPosiciones()
    {
        foreach (var entry in npcPositionsByRoom)
        {
            Debug.Log($"Sala {entry.Key}: {entry.Value}");
        }
    }

    /// <summary>
    /// Instancia los elementos de la escena (NPCs, objetos) en las posiciones registradas por sala.
    /// </summary>
    public void InstanciarElementos(List<ElementoEscena> elementos)
    {
        if (elementos == null || elementos.Count == 0)
        {
            Debug.LogWarning("[NPCFinder] No hay elementos para instanciar.");
            return;
        }

        Debug.Log($"[NPCFinder] Iniciando instanciación de {elementos.Count} elementos.");

        foreach (var e in elementos)
        {
            Vector3 spawnPos = Vector3.zero;
            bool posFound = false;

            Debug.Log($"[NPCFinder] Procesando {e.prefab_id}, room_index: {e.room_index}");

            // Si tiene índice de sala, buscamos la posición guardada
            if (e.room_index >= 0)
            {
                Vector3? pos = ObtenerPosicionEnSala(e.room_index);
                if (pos.HasValue)
                {
                    spawnPos = pos.Value;
                    posFound = true;
                    Debug.Log($"[NPCFinder] Posición encontrada para sala {e.room_index}: {spawnPos}");
                }
                else
                {
                    Debug.LogWarning($"[NPCFinder] No se encontró punto NPC_ en la sala {e.room_index} para {e.prefab_id}. Posiciones registradas: {npcPositionsByRoom.Count}");
                }
            }
            else
            {
                // Posición absoluta si no hay índice de sala
                spawnPos = new Vector3(e.pos_x, e.pos_y, e.pos_z);
                posFound = true;
            }

            if (posFound)
            {
                GameObject prefab = LoadPrefab(e.prefab_id);
                if (prefab != null)
                {
                    GameObject instance = Instantiate(prefab, spawnPos, Quaternion.Euler(0, e.rot_y, 0));
                    instance.name = e.prefab_id;

                    if (instance.TryGetComponent<IConfigurable>(out var configurable))
                    {
                        configurable.Setup(e.data);
                    }
                    
                    Debug.Log($"[NPCFinder] Spawneado {e.prefab_id} en sala {e.room_index}");
                }
                else
                {
                    Debug.LogWarning($"[NPCFinder] No se encontró prefab para {e.prefab_id}");
                }
            }
        }
    }

    private GameObject LoadPrefab(string id)
    {
        WorldBuilder builder = Object.FindAnyObjectByType<WorldBuilder>();
        if (builder != null)
        {
            foreach (var p in builder.prefabLibrary)
            {
                if (p != null && p.name == id) return p;
            }
        }
        return Resources.Load<GameObject>("Prefabs/" + id);
    }
    }