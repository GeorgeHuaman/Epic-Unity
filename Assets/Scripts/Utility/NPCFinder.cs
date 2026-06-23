using UnityEngine;
using System.Collections.Generic;

public class NPCFinder : MonoBehaviour
{
    private static NPCFinder _instance;
    public static NPCFinder Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = Object.FindAnyObjectByType<NPCFinder>();
            }
            return _instance;
        }
    }

    [Header("Posiciones Registradas")]
    public Dictionary<int, List<Vector3>> npcPositionsByRoom = new Dictionary<int, List<Vector3>>();
    public List<Vector3> npcPositions = new List<Vector3>();
    private Dictionary<int, int> roomUsageCounter = new Dictionary<int, int>();

    private void Awake()
    {
        if (_instance == null || _instance == this)
        {
            _instance = this;
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
        Debug.Log("[NPCFinder] Limpiando posiciones.");
        npcPositions.Clear();
        npcPositionsByRoom.Clear();
        roomUsageCounter.Clear();
    }

    /// <summary>
    /// Registra una nueva posición de NPC asociada a un índice de sala.
    /// </summary>
    public void RegistrarPosicionNPC(int roomIndex, Vector3 posicion)
    {
        Debug.Log($"[NPCFinder] Registrando punto en sala {roomIndex}: {posicion}");
        npcPositions.Add(posicion);
        if (!npcPositionsByRoom.ContainsKey(roomIndex))
        {
            npcPositionsByRoom.Add(roomIndex, new List<Vector3>());
        }
        npcPositionsByRoom[roomIndex].Add(posicion);
    }

    /// <summary>
    /// Obtiene la posición de un NPC para una sala específica.
    /// </summary>
    public Vector3? ObtenerPosicionEnSala(int roomIndex)
    {
        if (npcPositionsByRoom.TryGetValue(roomIndex, out List<Vector3> positions))
        {
            if (!roomUsageCounter.ContainsKey(roomIndex))
            {
                roomUsageCounter[roomIndex] = 0;
            }

            int index = roomUsageCounter[roomIndex];
            if (index < positions.Count)
            {
                roomUsageCounter[roomIndex]++;
                return positions[index];
            }
            else
            {
                // Si ya usamos todos los puntos, repetimos el primero para no fallar
                return positions[0];
            }
        }
        return null;
    }

    [ContextMenu("Debug Log NPC Positions")]
    public void LogPosiciones()
    {
        foreach (var entry in npcPositionsByRoom)
        {
            Debug.Log($"Sala {entry.Key}: {entry.Value.Count} puntos registrados");
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
        WorldBuilder builder = Object.FindAnyObjectByType<WorldBuilder>();

        foreach (var e in elementos)
        {
            Vector3 spawnPos = Vector3.zero;
            bool posFound = false;

            // Si tiene índice de sala, buscamos la posición guardada
            if (e.room_index >= 0)
            {
                Vector3? pos = ObtenerPosicionEnSala(e.room_index);
                if (pos.HasValue)
                {
                    spawnPos = pos.Value;
                    posFound = true;
                }
                else
                {
                    Debug.LogWarning($"[NPCFinder] No se encontró punto NPC_ en la sala {e.room_index} para {e.prefab_id}.");
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
                    GameObject instance = Instantiate(prefab, spawnPos, Quaternion.Euler(0, e.rot_y, 0), builder != null ? builder.transform : null);
                    instance.name = e.prefab_id;

                    IConfigurable configurable = instance.GetComponent<IConfigurable>();
                    if (configurable == null) configurable = instance.GetComponentInChildren<IConfigurable>();

                    if (configurable != null)
                    {
                        configurable.Setup(e.data);
                    }
                    
                    Debug.Log($"[NPCFinder] Spawneado {e.prefab_id} en sala {e.room_index} en {spawnPos}");
                }
                else
                {
                    Debug.LogError($"[NPCFinder] ERROR: No se encontró prefab para ID '{e.prefab_id}'. Asegúrate de que esté en el prefabLibrary de WorldBuilder o en Resources/Prefabs/");
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