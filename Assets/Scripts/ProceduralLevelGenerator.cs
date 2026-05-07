using UnityEngine;
using System.Collections.Generic;

public class ProceduralLevelGenerator : MonoBehaviour
{
    private enum PieceType { Sala, Pasillo }

    [Header("Prefabs Principales")]
    public GameObject salaInicialPrefab;
    public GameObject salaFinalPrefab;

    [Header("Colecciones")]
    public List<GameObject> salaPrefabs;
    public List<GameObject> pasilloPrefabs;

    [Header("Configuración de Generación")]
    [Range(1, 20)]
    public int mainPathLength = 10;
    [Range(0f, 1f)]
    public float branchProbability = 0.3f;
    
    [Header("Detección de Colisiones")]
    public LayerMask detectionLayer;
    public Vector3 overlapBoxSize = new Vector3(4f, 4f, 4f);

    private List<GameObject> generatedPieces = new List<GameObject>();

    void Start() => Generate();

    [ContextMenu("Generate")]
    public void Generate()
    {
        Clear();
        if (salaInicialPrefab == null) return;

        // 1. Crear Sala Inicial
        GameObject startRoom = Instantiate(salaInicialPrefab, transform.position, transform.rotation, transform);
        startRoom.name = "SALA_INICIAL";
        generatedPieces.Add(startRoom);
        
        // 2. Iniciar Generación con prioridad de camino principal
        SpawnNextLevel(startRoom, 0, PieceType.Sala);
    }

    private void SpawnNextLevel(GameObject parentPiece, int iteration, PieceType parentType)
    {
        if (iteration >= mainPathLength)
        {
            PlaceFinalRooms(parentPiece);
            return;
        }

        List<Transform> pivots = FindPivots(parentPiece.transform);
        if (pivots.Count == 0) return;

        // Estrategia 5: Priorizamos un pivot para que sea el "Camino Principal"
        // Los demás pivots serán "Ramas Secundarias" con probabilidad de fallo si chocan
        for (int i = 0; i < pivots.Count; i++)
        {
            Transform pivot = pivots[i];
            bool isMainPath = (i == 0); // El primer pivot encontrado se considera camino principal
            
            // Si no es camino principal, aplicamos probabilidad de rama
            if (!isMainPath && Random.value > branchProbability) continue;

            PieceType nextType = (parentType == PieceType.Sala) ? PieceType.Pasillo : PieceType.Sala;
            GameObject prefabToSpawn = GetRandomPrefab(nextType);
            if (prefabToSpawn == null) continue;

            // COMPROBACIÓN DE SOLAPAMIENTO (Idea 1 combinada con 5)
            // Antes de instanciar, verificamos si el espacio está libre
            if (!IsSpaceClear(pivot.position, pivot.rotation))
            {
                // Si el camino principal choca, intentamos cerrarlo con una sala final
                if (isMainPath) PlaceFinalRooms(parentPiece);
                continue; 
            }

            GameObject nextPiece = Instantiate(prefabToSpawn, pivot.position, pivot.rotation, transform);
            nextPiece.name = $"{nextType}_{iteration}_{prefabToSpawn.name}";
            generatedPieces.Add(nextPiece);
            
            SpawnNextLevel(nextPiece, iteration + 1, nextType);
        }
    }

    private bool IsSpaceClear(Vector3 position, Quaternion rotation)
    {
        // Lanzamos una caja invisible para ver si hay algo en la capa de detección
        // Ajustamos el centro de la caja un poco hacia adelante del pivot
        Vector3 checkPos = position + (rotation * Vector3.forward * (overlapBoxSize.z * 0.5f));
        Collider[] hitColliders = Physics.OverlapBox(checkPos, overlapBoxSize * 0.5f, rotation, detectionLayer);
        
        return hitColliders.Length == 0;
    }

    private void PlaceFinalRooms(GameObject parentPiece)
    {
        List<Transform> pivots = FindPivots(parentPiece.transform);
        foreach (Transform pivot in pivots)
        {
            // Solo ponemos sala final si el espacio está libre
            if (IsSpaceClear(pivot.position, pivot.rotation))
            {
                if (salaFinalPrefab != null)
                {
                    GameObject final = Instantiate(salaFinalPrefab, pivot.position, pivot.rotation, transform);
                    final.name = "SALA_FINAL";
                    generatedPieces.Add(final);
                }
            }
        }
    }

    private List<Transform> FindPivots(Transform root)
    {
        List<Transform> found = new List<Transform>();
        foreach (Transform child in root)
        {
            if (child.name.Contains("Pivot", System.StringComparison.OrdinalIgnoreCase))
                found.Add(child);
            else
                CheckForPivotsRecursive(child, found);
        }
        return found;
    }

    private void CheckForPivotsRecursive(Transform t, List<Transform> found)
    {
        foreach (Transform child in t)
        {
            if (child.name.Contains("Pivot", System.StringComparison.OrdinalIgnoreCase))
                found.Add(child);
            else
                CheckForPivotsRecursive(child, found);
        }
    }

    private GameObject GetRandomPrefab(PieceType type)
    {
        if (type == PieceType.Pasillo)
            return (pasilloPrefabs.Count > 0) ? pasilloPrefabs[Random.Range(0, pasilloPrefabs.Count)] : null;
        else
            return (salaPrefabs.Count > 0) ? salaPrefabs[Random.Range(0, salaPrefabs.Count)] : null;
    }

    public void Clear()
    {
        foreach (var p in generatedPieces) if (p != null) DestroyImmediate(p);
        generatedPieces.Clear();
        List<GameObject> toDestroy = new List<GameObject>();
        foreach (Transform child in transform) toDestroy.Add(child.gameObject);
        toDestroy.ForEach(c => DestroyImmediate(c));
    }

    #if UNITY_EDITOR
    [ContextMenu("Auto Assign Prefabs")]
    public void AutoAssignPrefabs()
    {
        string folderPath = "Assets/Prefabs/Library/Props/Colegio";
        string[] guids = UnityEditor.AssetDatabase.FindAssets("t:Prefab", new[] { folderPath });
        
        salaPrefabs = new List<GameObject>();
        pasilloPrefabs = new List<GameObject>();

        foreach (string guid in guids)
        {
            string path = UnityEditor.AssetDatabase.GUIDToAssetPath(guid);
            GameObject prefab = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>(path);
            
            if (prefab.name.Equals("Sala_Incial", System.StringComparison.OrdinalIgnoreCase))
                salaInicialPrefab = prefab;
            else if (prefab.name.Equals("Sala_Final", System.StringComparison.OrdinalIgnoreCase))
                salaFinalPrefab = prefab;
            else if (prefab.name.StartsWith("Sala", System.StringComparison.OrdinalIgnoreCase))
                salaPrefabs.Add(prefab);
            else if (prefab.name.StartsWith("Pasillo", System.StringComparison.OrdinalIgnoreCase))
                pasilloPrefabs.Add(prefab);
        }
        UnityEditor.EditorUtility.SetDirty(this);
        Debug.Log("Prefabs auto-asignados.");
    }
    #endif

    private void OnDrawGizmos()
    {
        // Dibujamos el área de detección para debuguear el tamaño del OverlapBox
        Gizmos.color = new Color(1, 0, 0, 0.3f);
        foreach (var piece in generatedPieces)
        {
            if (piece == null) continue;
            List<Transform> pList = FindPivots(piece.transform);
            foreach (var p in pList)
            {
                Vector3 checkPos = p.position + (p.rotation * Vector3.forward * (overlapBoxSize.z * 0.5f));
                Matrix4x4 rotationMatrix = Matrix4x4.TRS(checkPos, p.rotation, overlapBoxSize);
                Gizmos.matrix = rotationMatrix;
                Gizmos.DrawWireCube(Vector3.zero, Vector3.one);
            }
        }
    }
}
