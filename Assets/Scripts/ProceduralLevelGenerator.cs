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

    [Header("Configuración")]
    [Range(1, 20)]
    public int maxIterations = 5;
    
    private List<GameObject> generatedPieces = new List<GameObject>();

    void Start() => Generate();

    [ContextMenu("Generate")]
    public void Generate()
    {
        Clear();
        if (salaInicialPrefab == null) 
        {
            Debug.LogError("Falta el prefab de la Sala Inicial");
            return;
        }

        // 1. Crear Sala Inicial en la posición del Generador
        GameObject startRoom = Instantiate(salaInicialPrefab, transform.position, transform.rotation, transform);
        startRoom.name = "SALA_INICIAL";
        generatedPieces.Add(startRoom);
        
        // 2. Iniciar la cadena: El padre es Sala, lo siguiente será Pasillo
        SpawnLevel(startRoom, 0, PieceType.Sala);
    }

    private void SpawnLevel(GameObject parentPiece, int iteration, PieceType parentType)
    {
        // Detenerse si alcanzamos el máximo de iteraciones
        if (iteration >= maxIterations)
        {
            PlaceFinalRooms(parentPiece);
            return;
        }

        List<Transform> pivots = FindPivots(parentPiece.transform);
        
        foreach (Transform pivot in pivots)
        {
            // REGLA DE ORO: Alternar tipos
            PieceType nextType = (parentType == PieceType.Sala) ? PieceType.Pasillo : PieceType.Sala;
            
            GameObject prefabToSpawn = GetRandomPrefab(nextType);
            if (prefabToSpawn == null) continue;

            // Instanciar usando la posición y rotación del Pivot
            GameObject nextPiece = Instantiate(prefabToSpawn, pivot.position, pivot.rotation, transform);
            nextPiece.name = $"{nextType}_{iteration}_{prefabToSpawn.name}";
            generatedPieces.Add(nextPiece);
            
            // RECURSIÓN: Ahora el 'parentType' es el tipo que acabamos de crear
            SpawnLevel(nextPiece, iteration + 1, nextType);
        }
    }

    private void PlaceFinalRooms(GameObject parentPiece)
    {
        List<Transform> pivots = FindPivots(parentPiece.transform);
        foreach (Transform pivot in pivots)
        {
            if (salaFinalPrefab != null)
            {
                GameObject final = Instantiate(salaFinalPrefab, pivot.position, pivot.rotation, transform);
                final.name = "SALA_FINAL";
                generatedPieces.Add(final);
            }
        }
    }

    private List<Transform> FindPivots(Transform root)
    {
        List<Transform> found = new List<Transform>();
        FindPivotsRecursive(root, found);
        return found;
    }

    private void FindPivotsRecursive(Transform t, List<Transform> found)
    {
        foreach (Transform child in t)
        {
            // Buscamos "Pivot" o "Pivote"
            if (child.name.Contains("Pivot", System.StringComparison.OrdinalIgnoreCase))
            {
                found.Add(child);
            }
            else
            {
                FindPivotsRecursive(child, found);
            }
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
        while (transform.childCount > 0) DestroyImmediate(transform.GetChild(0).gameObject);
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
        Gizmos.color = Color.cyan;
        foreach (var piece in generatedPieces)
        {
            if (piece == null) continue;
            List<Transform> pivots = FindPivots(piece.transform);
            foreach (var p in pivots)
            {
                Gizmos.DrawRay(p.position, p.forward * 1.5f);
                Gizmos.DrawSphere(p.position, 0.1f);
            }
        }
    }
}
