using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;

public class ProceduralLevelGenerator : MonoBehaviour
{
    private enum PieceType { Sala, Pasillo }

    [System.Serializable]
    public class PieceData
    {
        public string prefabName;
        public string pieceType;
        public Vector3 position;
        public Vector3 rotation;
    }

    [System.Serializable]
    public class LevelData
    {
        public List<PieceData> pieces = new List<PieceData>();
    }

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

    [Header("JSON")]
    [TextArea(10, 30)]
    public string generatedJson;

    [Header("Save System")]
    public string jsonFileName = "level.json";
    public bool autoSaveJson = true;

    [Header("AI")]
    public string forcedRoomPrefabName;

    public static ProceduralLevelGenerator Instance;

    private List<GameObject> generatedPieces =
        new List<GameObject>();

    private LevelData currentLevelData =
        new LevelData();

    private void Start()
    {
        Instance = this;
    }

    [ContextMenu("Generate")]
    public void Generate()
    {
        StopAllCoroutines();
        StartCoroutine(GenerateCoroutine());
    }

    private IEnumerator GenerateCoroutine()
    {
        Clear();

        currentLevelData = new LevelData();

        if (salaInicialPrefab == null)
            yield break;

        GameObject startRoom = Instantiate(
            salaInicialPrefab,
            transform.position,
            transform.rotation,
            transform
        );

        startRoom.name = "SALA_INICIAL";

        generatedPieces.Add(startRoom);

        SavePieceData(startRoom, "SalaInicial");

        yield return new WaitForSeconds(buildDelay);

        yield return StartCoroutine(
            SpawnNextLevelCoroutine(
                startRoom,
                0,
                PieceType.Sala
            )
        );

        generatedJson = JsonUtility.ToJson(
            currentLevelData,
            true
        );

        UIManager.Instance.configuracionActual.ordenSala = generatedJson;
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
                PlaceFinalRoomsOrWallsCoroutine(
                    parentPiece
                )
            );

            yield break;
        }

        List<Transform> pivots =
            FindPivots(parentPiece.transform);

        if (pivots.Count == 0)
            yield break;

        for (int i = 0; i < pivots.Count; i++)
        {
            Transform pivot = pivots[i];

            bool isMainPath = (i == 0);

            bool shouldAttemptSpawn =
                isMainPath ||
                (Random.value <= branchProbability);

            bool spawned = false;

            if (shouldAttemptSpawn)
            {
                PieceType nextType =
                    (parentType == PieceType.Sala)
                    ? PieceType.Pasillo
                    : PieceType.Sala;

                GameObject prefabToSpawn =
                    GetRandomPrefab(nextType);

                if (prefabToSpawn != null &&
                    IsSpaceClear(
                        pivot.position,
                        pivot.rotation
                    ))
                {
                    GameObject nextPiece = Instantiate(
                        prefabToSpawn,
                        pivot.position,
                        pivot.rotation,
                        transform
                    );

                    nextPiece.name =
                       nextPiece.name = prefabToSpawn.name;

                    generatedPieces.Add(nextPiece);

                    SavePieceData(
                        nextPiece,
                        nextType.ToString()
                    );

                    Physics.SyncTransforms();

                    yield return new WaitForSeconds(
                        buildDelay
                    );

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

                yield return new WaitForSeconds(
                    buildDelay
                );
            }
        }
    }

    private IEnumerator PlaceFinalRoomsOrWallsCoroutine(
        GameObject parentPiece
    )
    {
        List<Transform> pivots =
            FindPivots(parentPiece.transform);

        foreach (Transform pivot in pivots)
        {
            if (IsSpaceClear(
                    pivot.position,
                    pivot.rotation
                ) &&
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

                SavePieceData(
                    final,
                    "SalaFinal"
                );
            }
            else
            {
                PlaceBlockingObject(pivot);
            }

            yield return new WaitForSeconds(
                buildDelay
            );
        }
    }

    private void SavePieceData(
        GameObject obj,
        string type
    )
    {
        PieceData data = new PieceData();

        data.prefabName =
            RemoveClone(obj.name);

        data.pieceType = type;

        data.position =
            obj.transform.position;

        data.rotation =
            obj.transform.eulerAngles;

        currentLevelData.pieces.Add(data);
    }

    private string RemoveClone(string value)
    {
        return value
            .Replace("(Clone)", "")
            .Trim();
    }

    private IEnumerator BuildLevelFromJsonCoroutine(
        string json
    )
    {
        Clear();

        if (string.IsNullOrEmpty(json))
            yield break;

        LevelData data =
            JsonUtility.FromJson<LevelData>(json);

        foreach (PieceData piece in data.pieces)
        {
            GameObject prefab =
                FindPrefabByName(piece.prefabName);

            if (prefab == null)
            {
                Debug.LogWarning(
                    "No se encontró prefab: " +
                    piece.prefabName
                );

                continue;
            }

            GameObject spawned = Instantiate(
                prefab,
                piece.position,
                Quaternion.Euler(piece.rotation),
                transform
            );

            spawned.name = piece.prefabName;

            generatedPieces.Add(spawned);

            yield return new WaitForSeconds(
                buildDelay
            );
        }
    }

    public void LoadJsonFromFile(string json)
    {
        generatedJson = json;
        // GENERAR NIVEL AUTOMÁTICAMENTE
        StopAllCoroutines();

        StartCoroutine(
            BuildLevelFromJsonCoroutine(generatedJson)
        );
    }

    private GameObject FindPrefabByName(
        string prefabName
    )
    {
        if (salaInicialPrefab != null &&
            salaInicialPrefab.name == prefabName)
            return salaInicialPrefab;

        if (salaFinalPrefab != null &&
            salaFinalPrefab.name == prefabName)
            return salaFinalPrefab;

        foreach (GameObject g in salaPrefabs)
        {
            if (g.name == prefabName)
                return g;
        }

        foreach (GameObject g in pasilloPrefabs)
        {
            if (g.name == prefabName)
                return g;
        }

        foreach (GameObject g in blockingPrefabs)
        {
            if (g.name == prefabName)
                return g;
        }

        return null;
    }

    private void PlaceBlockingObject(
        Transform pivot
    )
    {
        if (blockingPrefabs != null &&
            blockingPrefabs.Count > 0)
        {
            GameObject prefab =
                blockingPrefabs[
                    Random.Range(
                        0,
                        blockingPrefabs.Count
                    )
                ];

            GameObject block = Instantiate(
                prefab,
                pivot.position,
                pivot.rotation,
                transform
            );

            block.name =
                "BLOCKING_" + prefab.name;

            generatedPieces.Add(block);

            SavePieceData(
                block,
                "Blocking"
            );
        }
    }

    private bool IsSpaceClear(
        Vector3 position,
        Quaternion rotation
    )
    {
        Vector3 checkPos =
            position +
            (
                rotation *
                Vector3.left *
                (overlapBoxSize.z * 0.5f)
            );

        Collider[] hitColliders =
            Physics.OverlapBox(
                checkPos,
                overlapBoxSize * 0.5f,
                rotation,
                detectionLayer
            );

        return hitColliders.Length == 0;
    }

    private List<Transform> FindPivots(
        Transform root
    )
    {
        List<Transform> found =
            new List<Transform>();

        foreach (Transform child in root)
        {
            if (child.name.Contains(
                "Pivot",
                System.StringComparison
                    .OrdinalIgnoreCase
            ))
            {
                found.Add(child);
            }
            else
            {
                CheckForPivotsRecursive(
                    child,
                    found
                );
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
            if (child.name.Contains(
                "Pivot",
                System.StringComparison
                    .OrdinalIgnoreCase
            ))
            {
                found.Add(child);
            }
            else
            {
                CheckForPivotsRecursive(
                    child,
                    found
                );
            }
        }
    }

    private GameObject GetRandomPrefab(
        PieceType type
    )
    {
        if (type == PieceType.Pasillo)
        {
            return (pasilloPrefabs.Count > 0)
                ? pasilloPrefabs[
                    Random.Range(
                        0,
                        pasilloPrefabs.Count
                    )
                ]
                : null;
        }

        if (!string.IsNullOrEmpty(
                forcedRoomPrefabName
            ))
        {
            GameObject forced =
                salaPrefabs.Find(
                    x =>
                        x.name ==
                        forcedRoomPrefabName
                );

            if (forced != null)
                return forced;
        }

        return (salaPrefabs.Count > 0)
            ? salaPrefabs[
                Random.Range(
                    0,
                    salaPrefabs.Count
                )
            ]
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

        List<GameObject> toDestroy =
            new List<GameObject>();

        foreach (Transform child in transform)
        {
            toDestroy.Add(child.gameObject);
        }

        toDestroy.ForEach(
            c => DestroyImmediate(c)
        );
    }

    private void OnDrawGizmos()
    {
        Gizmos.color =
            new Color(1, 0, 0, 0.3f);

        foreach (var piece in generatedPieces)
        {
            if (piece == null)
                continue;

            List<Transform> pList =
                FindPivots(piece.transform);

            foreach (var p in pList)
            {
                Vector3 checkPos =
                    p.position +
                    (
                        p.rotation *
                        Vector3.left *
                        (
                            overlapBoxSize.z *
                            0.5f
                        )
                    );

                Matrix4x4 rotationMatrix =
                    Matrix4x4.TRS(
                        checkPos,
                        p.rotation,
                        overlapBoxSize
                    );

                Gizmos.matrix =
                    rotationMatrix;

                Gizmos.DrawWireCube(
                    Vector3.zero,
                    Vector3.one
                );
            }
        }
    }
}