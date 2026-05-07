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

        // 1. Crear Sala Inicial usando la posición y rotación del objeto Generador
        GameObject startRoom = Instantiate(salaInicialPrefab, transform.position, transform.rotation, transform);
        startRoom.name = "SALA_INICIAL";
        generatedPieces.Add(startRoom);
        
        // 2. Iniciar la cadena: El padre es Sala, lo siguiente será Pasillo
        SpawnLevel(startRoom, 0, PieceType.Sala);
    }

    private void SpawnLevel(GameObject parentPiece, int iteration, PieceType parentType)
    {
        if (iteration >= maxIterations)
        {
            PlaceFinalRooms(parentPiece);
            return;
        }

        // Buscamos pivots solo en el nivel inmediatamente inferior para evitar recursiones infinitas o errores de jerarquía
        List<Transform> pivots = FindPivots(parentPiece.transform);
        
        foreach (Transform pivot in pivots)
        {
            // Alternar tipos: Sala -> Pasillo -> Sala
            PieceType nextType = (parentType == PieceType.Sala) ? PieceType.Pasillo : PieceType.Sala;
            
            GameObject prefabToSpawn = GetRandomPrefab(nextType);
            if (prefabToSpawn == null) continue;

            // INSTANCIACIÓN CRÍTICA: Se usa la posición y rotación DEL PIVOT.
            // Si el pivot está en un lateral y rotado 90°, el nuevo objeto se creará con esa rotación.
            GameObject nextPiece = Instantiate(prefabToSpawn, pivot.position, pivot.rotation, transform);
            
            // Opcional: Si el prefab tiene un "Pivot_Entrada", podríamos ajustar la posición para que coincidan.
            // Por defecto, asumimos que el (0,0,0) del prefab es su punto de entrada.
            
            nextPiece.name = $"{nextType}_{iteration}_{prefabToSpawn.name}";
            generatedPieces.Add(nextPiece);
            
            // Debug para verificar en consola que la rotación se está aplicando
            // Debug.Log($"Generando {nextPiece.name} en {pivot.position} con rotación {pivot.rotation.eulerAngles}");
            
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
        // Buscamos solo en hijos para evitar encontrar pivots de piezas que ya están conectadas a este padre
        foreach (Transform child in root)
        {
            if (child.name.Contains("Pivot", System.StringComparison.OrdinalIgnoreCase))
            {
                found.Add(child);
            }
            else
            {
                // Si el pivot no es hijo directo (está dentro de un sub-objeto), lo buscamos
                CheckForPivotsRecursive(child, found);
            }
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
        // Limpieza extra de hijos directos
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
        // Visualización de la dirección de los pivots
        Gizmos.color = Color.cyan;
        foreach (var piece in generatedPieces)
        {
            if (piece == null) continue;
            // Dibujamos los pivots de esta pieza
            List<Transform> pList = FindPivots(piece.transform);
            foreach (var p in pList)
            {
                // Dibujamos una línea indicando el frente (Forward) del pivot
                Gizmos.DrawRay(p.position, p.forward * 1.5f);
                Gizmos.DrawSphere(p.position, 0.1f);
            }
        }
    }
}
