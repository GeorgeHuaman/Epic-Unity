using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ProceduralLevelGenerator : MonoBehaviour
{
    private enum PieceType { Sala, Pasillo }

    [Header("Prefabs Principales")]
    public GameObject salaInicialPrefab;
    public GameObject salaFinalPrefab;
    public List<GameObject> blockingPrefabs;

    [Header("Colecciones")]
    public List<GameObject> salaPrefabs;
    public List<GameObject> pasilloPrefabs;

    [Header("Configuración de Generación")]
    [Range(1, 20)]
    public int mainPathLength = 10;

    [Range(0f, 1f)]
    public float branchProbability = 0.3f;

    [Header("Construcción Visual")]
    public float buildDelay = 0.15f;

    [Header("Detección de Colisiones")]
    public LayerMask detectionLayer;
    public Vector3 overlapBoxSize = new Vector3(4f, 4f, 4f);

    private List<GameObject> generatedPieces = new List<GameObject>();

    [Header("AI")]
    public string forcedRoomPrefabName;

    [ContextMenu("Generate")]
    public void Generate()
    {
        StopAllCoroutines();
        StartCoroutine(GenerateCoroutine());
    }

    private IEnumerator GenerateCoroutine()
    {
        Clear();

        if (salaInicialPrefab == null)
            yield break;

        // SALA INICIAL
        GameObject startRoom = Instantiate(
            salaInicialPrefab,
            transform.position,
            transform.rotation,
            transform
        );

        startRoom.name = "SALA_INICIAL";
        generatedPieces.Add(startRoom);

        yield return new WaitForSeconds(buildDelay);

        // GENERACIÓN
        yield return StartCoroutine(
            SpawnNextLevelCoroutine(startRoom, 0, PieceType.Sala)
        );
    }

    private IEnumerator SpawnNextLevelCoroutine(
        GameObject parentPiece,
        int iteration,
        PieceType parentType
    )
    {
        if (iteration >= mainPathLength)
        {
            yield return StartCoroutine(
                PlaceFinalRoomsOrWallsCoroutine(parentPiece)
            );

            yield break;
        }

        List<Transform> pivots = FindPivots(parentPiece.transform);

        if (pivots.Count == 0)
            yield break;

        for (int i = 0; i < pivots.Count; i++)
        {
            Transform pivot = pivots[i];

            bool isMainPath = (i == 0);

            bool shouldAttemptSpawn =
                isMainPath || (Random.value <= branchProbability);

            bool spawned = false;

            if (shouldAttemptSpawn)
            {
                PieceType nextType =
                    (parentType == PieceType.Sala)
                    ? PieceType.Pasillo
                    : PieceType.Sala;

                GameObject prefabToSpawn = GetRandomPrefab(nextType);

                if (prefabToSpawn != null &&
                    IsSpaceClear(pivot.position, pivot.rotation))
                {
                    GameObject nextPiece = Instantiate(
                        prefabToSpawn,
                        pivot.position,
                        pivot.rotation,
                        transform
                    );

                    nextPiece.name =
                        $"{nextType}_{iteration}_{prefabToSpawn.name}";

                    generatedPieces.Add(nextPiece);

                    Physics.SyncTransforms();

                    // ESPERA PARA VER LA CONSTRUCCIÓN
                    yield return new WaitForSeconds(buildDelay);

                    yield return StartCoroutine(
                        SpawnNextLevelCoroutine(
                            nextPiece,
                            iteration + 1,
                            nextType
                        )
                    );

                    spawned = true;
                }
            }

            if (!spawned)
            {
                PlaceBlockingObject(pivot);

                yield return new WaitForSeconds(buildDelay);
            }
        }
    }

    private IEnumerator PlaceFinalRoomsOrWallsCoroutine(GameObject parentPiece)
    {
        List<Transform> pivots = FindPivots(parentPiece.transform);

        foreach (Transform pivot in pivots)
        {
            if (IsSpaceClear(pivot.position, pivot.rotation) &&
                salaFinalPrefab != null)
            {
                GameObject final = Instantiate(
                    salaFinalPrefab,
                    pivot.position,
                    pivot.rotation,
                    transform
                );

                final.name = "SALA_FINAL";

                generatedPieces.Add(final);
            }
            else
            {
                PlaceBlockingObject(pivot);
            }

            yield return new WaitForSeconds(buildDelay);
        }
    }

    private void PlaceBlockingObject(Transform pivot)
    {
        if (blockingPrefabs != null && blockingPrefabs.Count > 0)
        {
            GameObject prefab =
                blockingPrefabs[Random.Range(0, blockingPrefabs.Count)];

            GameObject block = Instantiate(
                prefab,
                pivot.position,
                pivot.rotation,
                transform
            );

            block.name = "BLOCKING_" + prefab.name;

            generatedPieces.Add(block);
        }
    }

    private bool IsSpaceClear(Vector3 position, Quaternion rotation)
    {
        Vector3 checkPos =
            position +
            (rotation * Vector3.left * (overlapBoxSize.z * 0.5f));

        Collider[] hitColliders = Physics.OverlapBox(
            checkPos,
            overlapBoxSize * 0.5f,
            rotation,
            detectionLayer
        );

        return hitColliders.Length == 0;
    }

    private List<Transform> FindPivots(Transform root)
    {
        List<Transform> found = new List<Transform>();

        foreach (Transform child in root)
        {
            if (child.name.Contains("Pivot",
                System.StringComparison.OrdinalIgnoreCase))
            {
                found.Add(child);
            }
            else
            {
                CheckForPivotsRecursive(child, found);
            }
        }

        return found;
    }

    private void CheckForPivotsRecursive(
        Transform t,
        List<Transform> found
    )
    {
        foreach (Transform child in t)
        {
            if (child.name.Contains("Pivot",
                System.StringComparison.OrdinalIgnoreCase))
            {
                found.Add(child);
            }
            else
            {
                CheckForPivotsRecursive(child, found);
            }
        }
    }

    private GameObject GetRandomPrefab(PieceType type)
    {
        if (type == PieceType.Pasillo)
        {
            return (pasilloPrefabs.Count > 0)
                ? pasilloPrefabs[Random.Range(0, pasilloPrefabs.Count)]
                : null;
        }

        if (!string.IsNullOrEmpty(forcedRoomPrefabName))
        {
            GameObject forced =
                salaPrefabs.Find(x => x.name == forcedRoomPrefabName);

            if (forced != null)
                return forced;
        }

        return (salaPrefabs.Count > 0)
            ? salaPrefabs[Random.Range(0, salaPrefabs.Count)]
            : null;
    }

    public void Clear()
    {
        foreach (var p in generatedPieces)
        {
            if (p != null)
                DestroyImmediate(p);
        }

        generatedPieces.Clear();

        List<GameObject> toDestroy = new List<GameObject>();

        foreach (Transform child in transform)
            toDestroy.Add(child.gameObject);

        toDestroy.ForEach(c => DestroyImmediate(c));
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = new Color(1, 0, 0, 0.3f);

        foreach (var piece in generatedPieces)
        {
            if (piece == null) continue;

            List<Transform> pList = FindPivots(piece.transform);

            foreach (var p in pList)
            {
                Vector3 checkPos =
                    p.position +
                    (p.rotation *
                    Vector3.left *
                    (overlapBoxSize.z * 0.5f));

                Matrix4x4 rotationMatrix =
                    Matrix4x4.TRS(
                        checkPos,
                        p.rotation,
                        overlapBoxSize
                    );

                Gizmos.matrix = rotationMatrix;

                Gizmos.DrawWireCube(Vector3.zero, Vector3.one);
            }
        }
    }
}